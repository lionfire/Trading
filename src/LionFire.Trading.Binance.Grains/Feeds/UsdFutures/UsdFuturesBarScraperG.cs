﻿using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using LionFire.ExtensionMethods.Dumping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
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


}

public class UsdFuturesBarScraperG : Grain, IUsdFuturesBarScraperG
{
    public IPersistentState<UsdFuturesBarScraperOptions> Options { get; }
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

    IDisposable barsSubscription;
    IDisposable? timer;

    public UsdFuturesBarScraperG(
        [PersistentState("BinanceUsdFuturesBarScraperOptions", "Trading")] IPersistentState<UsdFuturesBarScraperOptions> options,
        [PersistentState("BinanceUsdFuturesBarScrapeState", "Trading")] IPersistentState<BarScraperState> state,
        ILogger<UsdFuturesBarScraperG> logger,
        IBinanceRestClient binanceRestClient,
        IClusterClient clusterClient
        )
    {
        Logger = logger;
        BinanceRestClient = binanceRestClient;
        TentativeChannelProvider = clusterClient.GetBroadcastChannelProvider(BinanceBroadcastChannelNames.TentativeBars);
        ConfirmedChannelProvider = clusterClient.GetBroadcastChannelProvider(BinanceBroadcastChannelNames.ConfirmedBars);
        RevisionChannelProvider = clusterClient.GetBroadcastChannelProvider(BinanceBroadcastChannelNames.RevisionBars);
        InProgressChannelProvider = clusterClient.GetBroadcastChannelProvider(BinanceBroadcastChannelNames.InProgressBars);

        Options = options;
        State = state;
        var s = this.GetPrimaryKeyString().Split('^');
        if (s.Length != 2) throw new ArgumentException("Key must be in the format of {Symbol}^{TimeFrameString}");

        Symbol = s[0];
        TimeFrame = TimeFrame.Parse(s[1]);

        Scraper = ActivatorUtilities.CreateInstance<UsdFuturesBarScraper>(ServiceProvider, Symbol, TimeFrame);

        //Logger.LogInformation("Created {key} with TimeFrame: {tf}", this.GetPrimaryKeyString(), TimeFrame.ToShortString());
    }

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
            Logger.LogTrace("Next retrieve for {symbol}^{tf} in {dueTime}", Symbol, TimeFrame.ToShortString(), dueTime);
            timer = RegisterTimer(OnTimer, null!, dueTime, TimeFrame.TimeSpan!.Value * Options.State.Interval);
        }
    }

    #endregion

    private async Task BarsToBroadcastChannel(IEnumerable<BarEnvelope> bars)
    {
        var channelId = this.GetPrimaryKeyString();
        Logger.LogInformation("Broadcasting to channel: {channelId}", channelId);

        var confirmed = ConfirmedChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BinanceBroadcastChannelNames.ConfirmedBars, channelId));
        var tentative = TentativeChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BinanceBroadcastChannelNames.TentativeBars, channelId));
        var inProgress = InProgressChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BinanceBroadcastChannelNames.InProgressBars, channelId));
        var revision = RevisionChannelProvider.GetChannelWriter<IEnumerable<BarEnvelope>>(ChannelId.Create(BinanceBroadcastChannelNames.RevisionBars, channelId));

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
        InitTimer();
        await task;
    }

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

