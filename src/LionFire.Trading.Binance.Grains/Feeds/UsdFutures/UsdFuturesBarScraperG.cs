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

/// <summary>
/// Orleans grain interface for scraping bar (candlestick) data from Binance USD-M Futures.
/// Retrieves historical and real-time bar data and publishes it to broadcast channels.
/// </summary>
/// <remarks>
/// <para>
/// Grain key format: "{Symbol}^{TimeFrameString}" (e.g., "BTCUSDT^m1" for 1-minute bars)
/// </para>
/// <para>
/// This grain periodically polls Binance REST API for bar data and distributes it to
/// broadcast channels for consumption by other grains (e.g., LastBarsG, IndicatorG).
/// </para>
/// <para>
/// The scraper implements adaptive timing - it automatically adjusts retrieval delay
/// based on whether bars are being missed to optimize between latency and API efficiency.
/// </para>
/// </remarks>
public interface IUsdFuturesBarScraperG : IGrainWithStringKey
{
    /// <summary>
    /// Initializes the bar scraper and starts the polling timer.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Init();

    /// <summary>
    /// Manually triggers bar retrieval from Binance REST API.
    /// </summary>
    /// <returns>A task representing the asynchronous retrieval operation.</returns>
    Task RetrieveBars();

    /// <summary>
    /// Gets the current polling interval multiplier.
    /// </summary>
    /// <returns>The interval multiplier (number of timeframe periods between polls).</returns>
    /// <remarks>
    /// A value of 1 means poll every timeframe period (e.g., every 1 minute for m1).
    /// A value of 5 means poll every 5 timeframe periods (e.g., every 5 minutes for m1).
    /// A value of 0 disables automatic polling.
    /// </remarks>
    [ReadOnly]
    Task<int> Interval();

    /// <summary>
    /// Sets the polling interval multiplier.
    /// </summary>
    /// <param name="interval">The new interval multiplier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Interval(int interval);

    /// <summary>
    /// Gets the current time offset for bar retrieval relative to bar close.
    /// </summary>
    /// <returns>The offset in number of timeframe periods.</returns>
    /// <remarks>
    /// Used to control when bars are retrieved relative to their close time.
    /// For example, offset=1 retrieves bars one period after they close.
    /// </remarks>
    [ReadOnly]
    Task<int> Offset();

    /// <summary>
    /// Sets the time offset for bar retrieval.
    /// </summary>
    /// <param name="newValue">The new offset value.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Offset(int newValue);

    /// <summary>
    /// Callback invoked when missing bars are detected during scraping.
    /// Adjusts retrieval delay adaptively based on success/failure patterns.
    /// </summary>
    /// <param name="x">Tuple containing: missing bar count, trade count, and whether in-progress bar is missing.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnMissing((int missingCount, int tradeCount, bool missingInProgressBar) x);

}

/// <summary>
/// Adapter that wraps Orleans IPersistentState as IOptionsMonitor for compatibility with
/// components expecting options pattern monitoring.
/// </summary>
/// <typeparam name="T">The type of options to monitor.</typeparam>
/// <remarks>
/// This allows grain persistent state to be used with code that expects IOptionsMonitor,
/// though change notifications and named options are not supported.
/// </remarks>
public class GrainOptionsMonitor<T> : IOptionsMonitor<T>
{
    /// <summary>
    /// Gets the underlying persistent state.
    /// </summary>
    IPersistentState<T> State { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrainOptionsMonitor{T}"/> class.
    /// </summary>
    /// <param name="state">The persistent state to wrap.</param>
    public GrainOptionsMonitor(IPersistentState<T> state)
    {
        State = state;
    }

    /// <summary>
    /// Gets the current options value from persistent state.
    /// </summary>
    public T CurrentValue => State.State;

    /// <summary>
    /// Gets a named options instance. Not implemented - throws NotImplementedException.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <returns>The options instance.</returns>
    /// <exception cref="NotImplementedException">Named options are not supported.</exception>
    public T Get(string? name) { throw new NotImplementedException(); }

    /// <summary>
    /// Registers a change notification callback. Not implemented - throws NotImplementedException.
    /// </summary>
    /// <param name="listener">The callback to invoke when options change.</param>
    /// <returns>A disposable to unregister the callback.</returns>
    /// <exception cref="NotImplementedException">Change notifications are not supported.</exception>
    public IDisposable? OnChange(Action<T, string?> listener) { throw new NotImplementedException(); }
}

/// <summary>
/// Orleans grain implementation for scraping bar data from Binance USD-M Futures.
/// Polls Binance REST API and publishes bars to broadcast channels with adaptive timing.
/// </summary>
/// <remarks>
/// <para>
/// This grain manages the complete lifecycle of bar scraping including:
/// - Periodic polling based on configurable interval
/// - Adaptive retrieval delay optimization based on success/failure patterns
/// - Publishing bars to multiple broadcast channels (Confirmed, Tentative, InProgress, Revision)
/// - State persistence for scraper position and configuration
/// </para>
/// <para>
/// The grain uses an adaptive algorithm to minimize latency while avoiding excessive API calls:
/// - Increases delay when bars are missed (slow down to ensure bars are ready)
/// - Decreases delay after consecutive successes (speed up to reduce latency)
/// </para>
/// </remarks>
public class UsdFuturesBarScraperG : Grain, IUsdFuturesBarScraperG
{
    /// <summary>
    /// Gets the persistent polling configuration options.
    /// </summary>
    public IPersistentState<BarPollerOptions> Options { get; }

    /// <summary>
    /// Gets the persistent scraper state (last retrieved bar positions).
    /// </summary>
    public IPersistentState<BarScraperState> State { get; }

    /// <summary>
    /// Gets the logger for this grain.
    /// </summary>
    public ILogger<UsdFuturesBarScraperG> Logger { get; }

    /// <summary>
    /// Gets the Binance REST API client for retrieving bar data.
    /// </summary>
    public IBinanceRestClient BinanceRestClient { get; }

    /// <summary>
    /// Gets the broadcast channel provider for tentative (unconfirmed) bars.
    /// </summary>
    public IBroadcastChannelProvider TentativeChannelProvider { get; }

    /// <summary>
    /// Gets the broadcast channel provider for revision bars (bars that changed after initial publish).
    /// </summary>
    public IBroadcastChannelProvider RevisionChannelProvider { get; }

    /// <summary>
    /// Gets the broadcast channel provider for in-progress bars (current bar being formed).
    /// </summary>
    public IBroadcastChannelProvider InProgressChannelProvider { get; }

    /// <summary>
    /// Gets the broadcast channel provider for confirmed bars.
    /// </summary>
    public IBroadcastChannelProvider ConfirmedChannelProvider { get; }

    /// <summary>
    /// Gets the underlying bar scraper implementation.
    /// </summary>
    UsdFuturesBarScraper Scraper { get; }

    #region Parameters from key

    /// <summary>
    /// Gets the trading symbol (e.g., "BTCUSDT").
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Gets the timeframe for bars (e.g., m1, m5, h1).
    /// </summary>
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Subscription to bar scraper's bar stream.
    /// </summary>
    IDisposable? barsSubscription;

    /// <summary>
    /// Periodic timer for triggering bar retrieval.
    /// </summary>
    IDisposable? timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsdFuturesBarScraperG"/> class.
    /// </summary>
    /// <param name="options">Persistent state for polling options.</param>
    /// <param name="state">Persistent state for scraper position.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="binanceRestClient">Binance REST API client.</param>
    /// <param name="clusterClient">Orleans cluster client for accessing broadcast channels.</param>
    /// <exception cref="ArgumentException">Thrown when grain key is not in expected format "{Symbol}^{TimeFrameString}".</exception>
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

    /// <summary>
    /// Subscription to scraper's revision notifications.
    /// </summary>
    IDisposable scraperRevisionsSub;

    /// <summary>
    /// Called when the grain is activated. Initializes scraper state and subscribes to bar stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the activation operation.</param>
    /// <returns>A task representing the asynchronous activation.</returns>
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Scraper.State = State.State;

        Logger.Log(Options.State.Interval > 0 ? LogLevel.Information : LogLevel.Trace, "Activated {symbol}^{tf} with settings: {settings}", Symbol, TimeFrame.ToShortString(), Options.State.Dump());
        barsSubscription = Scraper.Bars.Subscribe(async bars => await OnBars(bars));

        return base.OnActivateAsync(cancellationToken);
    }

    /// <summary>
    /// Called when the grain is deactivated. Cleans up subscriptions and timer.
    /// </summary>
    /// <param name="reason">The reason for deactivation.</param>
    /// <param name="cancellationToken">Cancellation token for the deactivation operation.</param>
    /// <returns>A task representing the asynchronous deactivation.</returns>
    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        barsSubscription?.Dispose();
        scraperRevisionsSub?.Dispose();
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    /// <inheritdoc/>
    public Task Init()
    {
        this.GrainFactory.GetGrain<IUsdFuturesInfoG>("0").LastDayStats();

        InitTimer();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes or reinitializes the periodic polling timer based on current options.
    /// </summary>
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
            if (TimeFrame.TimeSpan <= TimeSpan.Zero) throw new NotImplementedException();
            var dueTime = TimeFrame.TimeUntilBarClose(now) + (TimeFrame.TimeSpan * Options.State.Offset) + b;
            dueTime = dueTime.Add(Options.State.RetrieveDelay);
            Logger.LogTrace("Next retrieve for {symbol}^{tf} in {dueTime} (retrieve delay: {retrieveDelay}s)", Symbol, TimeFrame.ToShortString(), dueTime, Options.State.RetrieveDelay.TotalSeconds.ToString("0.000"));
            timer = RegisterTimer(OnTimer, null!, dueTime, TimeFrame.TimeSpan * Options.State.Interval);
        }
    }

    #endregion

    /// <summary>
    /// Publishes bars to the appropriate broadcast channels based on their status.
    /// </summary>
    /// <param name="bars">The bars to publish.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    /// <remarks>
    /// Bars are routed to different channels based on their status:
    /// - Confirmed: Bars that are finalized and won't change
    /// - Tentative: Bars that may still be revised
    /// - InProgress: The current bar being formed
    /// - Revision: Bars that changed after initial publish
    /// </remarks>
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

    /// <summary>
    /// Handles newly retrieved bars from the scraper by publishing them to broadcast channels.
    /// </summary>
    /// <param name="bars">The bars retrieved from Binance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnBars(IEnumerable<BarEnvelope> bars)
    {
        await BarsToBroadcastChannel(bars);
    }

    /// <summary>
    /// Timer callback that triggers periodic bar retrieval.
    /// </summary>
    /// <param name="state">Timer state (not used).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OnTimer(object state)
    {
        var task = this.AsReference<IUsdFuturesBarScraperG>().RetrieveBars();
        await task;
        InitTimer();
    }

    #region Revisions

    /// <summary>
    /// Number of consecutive successful retrievals before attempting to decrease delay.
    /// </summary>
    public static int DecreaseDelayAfterSuccessCount = 30;

    /// <inheritdoc/>
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

    /// <summary>
    /// Counter tracking consecutive successful bar retrievals without missing data.
    /// Used to determine when to decrease retrieval delay for lower latency.
    /// </summary>
    public int ConsecutiveNotMissing = 0;

    /// <summary>
    /// Minimum consecutive successes required before optimizing delay (not currently used).
    /// </summary>
    public int ConsecutiveNotMissingBeforeOptimizing = 5;

    #endregion

    #endregion

    #region Methods

    /// <inheritdoc/>
    public async Task RetrieveBars()
    {
        await Scraper.RetrieveBars();
    }

    #endregion

    #region Accessors

    /// <inheritdoc/>
    public Task<int> Interval() => Task.FromResult(Options.State.Interval);

    /// <inheritdoc/>
    public async Task Interval(int interval)
    {
        Options.State.Interval = interval;
        await Options.WriteStateAsync();
    }

    /// <inheritdoc/>
    public Task<int> Offset() => Task.FromResult(Options.State.Offset);

    /// <inheritdoc/>
    public async Task Offset(int newValue)
    {
        Options.State.Offset = newValue;
        await Options.WriteStateAsync();
    }

    #endregion
}

