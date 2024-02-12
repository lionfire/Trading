using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using LionFire.ExtensionMethods.Dumping;
using LionFire.Trading.Feeds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Binance_;

public interface IUsdFuturesBarScraperG : IGrainWithStringKey
{
    Task Init();
    Task RetrieveBars();

    [ReadOnly]
    Task<int> Interval();
    Task Interval(int interval);

    [ReadOnly]
    Task<int> Offset();
    Task Offset(int newValue);


    Task OnMissing((int missingCount, int tradeCount, bool missingInProgressBar) x);

}

public class GrainOptionsMonitor<T> : IOptionsMonitor<T>
{
    IPersistentState<T> State { get; }

    public GrainOptionsMonitor(IPersistentState<T> state)
    {
        State = state;
    }

    public T CurrentValue => State.State;

    public T Get(string? name) { throw new NotImplementedException(); }

    public IDisposable? OnChange(Action<T, string?> listener) { throw new NotImplementedException(); }
}

public class UsdFuturesBarScraperG : Grain, IUsdFuturesBarScraperG
{
    public IPersistentState<BarPollerOptions> Options { get; }
    public IPersistentState<BarScraperState> State { get; }
    public ILogger<UsdFuturesBarScraperG> Logger { get; }
    public IBinanceRestClient BinanceRestClient { get; }
    public IBroadcastChannelProvider TentativeChannelProvider { get; }
    public IBroadcastChannelProvider RevisionChannelProvider { get; }
    public IBroadcastChannelProvider InProgressChannelProvider { get; }
    public IBroadcastChannelProvider ConfirmedChannelProvider { get; }
    UsdFuturesBarScraper Scraper { get; }

    #region Parameters from key

    public string Symbol { get; }
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    IDisposable? barsSubscription;
    IDisposable? timer;

    public UsdFuturesBarScraperG(
        [PersistentState("BinanceUsdFuturesBarScraperOptions", "Trading")] IPersistentState<BarPollerOptions> options,
        [PersistentState("BinanceUsdFuturesBarScrapeState", "Trading")] IPersistentState<BarScraperState> state,
        ILogger<UsdFuturesBarScraperG> logger,
        IBinanceRestClient binanceRestClient,
        IClusterClient clusterClient
        )
    {
        Logger = logger;
        BinanceRestClient = binanceRestClient;
        TentativeChannelProvider = clusterClient.GetBroadcastChannelProvider(BarsBroadcastChannelNames.TentativeBars);
        ConfirmedChannelProvider = clusterClient.GetBroadcastChannelProvider(BarsBroadcastChannelNames.ConfirmedBars);
        RevisionChannelProvider = clusterClient.GetBroadcastChannelProvider(BarsBroadcastChannelNames.RevisionBars);
        InProgressChannelProvider = clusterClient.GetBroadcastChannelProvider(BarsBroadcastChannelNames.InProgressBars);

        Options = options;
        State = state;
        var s = this.GetPrimaryKeyString().Split('^');
        if (s.Length != 2) throw new ArgumentException("Key must be in the format of {Symbol}^{TimeFrameString}");

        Symbol = s[0];
        TimeFrame = TimeFrame.Parse(s[1]);

        Scraper = ActivatorUtilities.CreateInstance<UsdFuturesBarScraper>(ServiceProvider, Symbol, TimeFrame);
        scraperRevisionsSub = Scraper.Revisions.Subscribe(x => this.AsReference<IUsdFuturesBarScraperG>().OnMissing(x));

        //Logger.LogInformation("Created {key} with TimeFrame: {tf}", this.GetPrimaryKeyString(), TimeFrame.ToShortString());
    }

    IDisposable scraperRevisionsSub;


    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Scraper.State = State.State;

        Logger.Log(Options.State.Interval > 0 ? LogLevel.Information : LogLevel.Trace, "Activated {symbol}^{tf} with settings: {settings}", Symbol, TimeFrame.ToShortString(), Options.State.Dump());
        barsSubscription = Scraper.Bars.Subscribe(async bars => await OnBars(bars));

        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        barsSubscription?.Dispose();
        scraperRevisionsSub?.Dispose();
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public Task Init()
    {
        this.GrainFactory.GetGrain<IUsdFuturesInfoG>("0").LastDayStats();

        InitTimer();

        return Task.CompletedTask;
    }
    private void InitTimer()
    {
        timer?.Dispose();
        if (Options.State.Interval <= 0)
        {
            Logger.LogTrace("Disabled scraper for {symbol} with TimeFrame: {tf}", Symbol, TimeFrame.ToShortString());
        }
        else
        {
            //var b = TimeSpan.FromMilliseconds(Options.State.PollOffsetMilliseconds);
            var b = TimeSpan.FromMilliseconds(0); // TEMP HARDCODE
            var now = DateTimeOffset.UtcNow; // ENH: use server time
            var dueTime = TimeFrame.TimeUntilBarClose(now) + (TimeFrame.TimeSpan!.Value * Options.State.Offset) + b;
            dueTime = dueTime.Add(Options.State.RetrieveDelay);
            Logger.LogTrace("Next retrieve for {symbol}^{tf} in {dueTime} (retrieve delay: {retrieveDelay}s)", Symbol, TimeFrame.ToShortString(), dueTime, Options.State.RetrieveDelay.TotalSeconds.ToString("0.000"));
            timer = RegisterTimer(OnTimer, null!, dueTime, TimeFrame.TimeSpan!.Value * Options.State.Interval);
        }
    }

    #endregion

    private async Task BarsToBroadcastChannel(IEnumerable<BarEnvelope> bars)
    {
        var r = new ExchangeSymbolTimeFrame("binance", "futures", Symbol, TimeFrame);
        var channelId = r.ToId();
        Logger.LogTrace("Broadcasting to channel: {channelId}", channelId);

        var confirmed = ConfirmedChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BarsBroadcastChannelNames.ConfirmedBars, channelId));
        var tentative = TentativeChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BarsBroadcastChannelNames.TentativeBars, channelId));
        var inProgress = InProgressChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BarsBroadcastChannelNames.InProgressBars, channelId));
        var revision = RevisionChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BarsBroadcastChannelNames.RevisionBars, channelId));

        await Task.WhenAll(
           confirmed.Publish(bars.Where(b => b.Status.HasFlag(BarStatus.Confirmed)).ToArray()),
           tentative.Publish(bars.Where(b => b.Status.HasFlag(BarStatus.Tentative) || b.Status == BarStatus.Unspecified).ToArray()),
           //tentative.Publish(bars.ToArray()),
           revision.Publish(bars.Where(b => b.Status.HasFlag(BarStatus.Revision)).ToArray()),
           inProgress.Publish(bars.Where(b => b.Status.HasFlag(BarStatus.InProgress)).ToArray())
           );
    }

    #region Event Handling

    private async Task OnBars(IEnumerable<BarEnvelope> bars)
    {
        await BarsToBroadcastChannel(bars);
    }

    private async Task OnTimer(object state)
    {
        var task = this.AsReference<IUsdFuturesBarScraperG>().RetrieveBars();
        await task;
        InitTimer();
    }

    #region Revisions

    public static int DecreaseDelayAfterSuccessCount = 30;
    async Task IUsdFuturesBarScraperG.OnMissing((int missingCount, int tradeCount, bool missingInProgressBar) x)
    {
        if (x.missingInProgressBar)
        {
            Debug.WriteLine("OnMissing: MissingInProgressBar" + x.missingCount);
        }
        if (x.missingCount == 0 && !x.missingInProgressBar)
        {

            if (x.tradeCount > 0)
            {
                if (ConsecutiveNotMissing++ > DecreaseDelayAfterSuccessCount)
                {
                    ConsecutiveNotMissing = 0;
                    var oldDelay = Options.State.RetrieveDelay;
                    Options.State.RetrieveDelay -= TimeSpan.FromMilliseconds(150);
                    if (Options.State.RetrieveDelay < TimeSpan.Zero) { Options.State.RetrieveDelay = TimeSpan.Zero; }

                    if (oldDelay != Options.State.RetrieveDelay)
                    {
                        Logger.LogInformation("{id} Decreased RetrieveDelay to {delay}s", this.GetPrimaryKeyString(), Options.State.RetrieveDelay.TotalSeconds.ToString("0.000"));
                        await Options.WriteStateAsync();
                    }
                }
            }
        }
        else
        {
            ConsecutiveNotMissing = 0;
            if (Options.State.RetrieveDelay.TotalMilliseconds < 2000)
            {
                Options.State.RetrieveDelay += TimeSpan.FromMilliseconds(1000);
            }
            else
            {
                Options.State.RetrieveDelay += TimeSpan.FromMilliseconds(200);
            }
            Logger.LogInformation("{id} Increased RetrieveDelay to: {delay}s", this.GetPrimaryKeyString(), Options.State.RetrieveDelay.TotalSeconds.ToString("0.000"));
            await Options.WriteStateAsync();
        }
    }
    public int ConsecutiveNotMissing = 0;
    public int ConsecutiveNotMissingBeforeOptimizing = 5;

    #endregion

    #endregion

    #region Methods

    public async Task RetrieveBars()
    {
        await Scraper.RetrieveBars();
    }

    #endregion

    #region Accessors

    public Task<int> Interval() => Task.FromResult(Options.State.Interval);
    public async Task Interval(int interval)
    {
        Options.State.Interval = interval;
        await Options.WriteStateAsync();
    }

    public Task<int> Offset() => Task.FromResult(Options.State.Offset);
    public async Task Offset(int newValue)
    {
        Options.State.Offset = newValue;
        await Options.WriteStateAsync();
    }

    #endregion
}

