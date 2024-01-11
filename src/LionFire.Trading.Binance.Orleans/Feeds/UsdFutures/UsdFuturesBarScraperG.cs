using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public IServiceProvider ServiceProvider { get; }

    UsdFuturesBarScraper Scraper { get; }

    #region Parameters from key

    public string Symbol { get; }
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    public UsdFuturesBarScraperG(
        [PersistentState("BinanceUsdFuturesBarScraperOptions", "Trading")] IPersistentState<UsdFuturesBarScraperOptions> options,
        [PersistentState("BinanceUsdFuturesBarScrapeState", "Trading")] IPersistentState<BarScraperState> state,
        ILogger<UsdFuturesBarScraperG> logger,
        IBinanceRestClient binanceRestClient,
        IServiceProvider serviceProvider)
    {
        Logger = logger;
        BinanceRestClient = binanceRestClient;
        ServiceProvider = serviceProvider;
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
        return base.OnActivateAsync(cancellationToken);
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
            var b = TimeSpan.FromMilliseconds(5000);
            var now = DateTimeOffset.UtcNow; // ENH: use server time
            var dueTime = TimeFrame.TimeUntilBarClose(now) + (TimeFrame.TimeSpan!.Value * Options.State.Offset) + b;
            Logger.LogTrace("Next retrieve for {symbol}^{tf} in {dueTime}", Symbol, TimeFrame.ToShortString(), dueTime);
            timer = RegisterTimer(OnTimer, null!, dueTime, TimeFrame.TimeSpan!.Value * Options.State.Interval);
        }
    }

    #endregion

    IDisposable? timer;

    #region Event Handling

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

