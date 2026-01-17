using Orleans;

namespace LionFire.Trading.Streaming;

/// <summary>
/// Consumer grain for tick (aggTrade) streaming.
/// Manages subscriptions and activates exchange-specific scrapers on demand.
/// </summary>
/// <remarks>
/// Grain key format: "{exchange}:{symbol}" (e.g., "binance.futures:BTCUSDT", "hyperliquid:BTC")
///
/// This grain follows the demand-driven activation pattern:
/// - Activates the appropriate exchange scraper when the first subscriber arrives
/// - Deactivates the scraper when the last subscriber leaves
/// - Routes incoming trades from the scraper to all subscribers
///
/// <example>
/// <code>
/// // Subscribe to BTCUSDT futures ticks
/// var tickStream = grainFactory.GetGrain&lt;ITickStreamG&gt;("binance.futures:BTCUSDT");
/// var result = await tickStream.Subscribe(this.AsReference&lt;ITickSubscriber&gt;());
///
/// // Later, unsubscribe
/// await tickStream.Unsubscribe(this.AsReference&lt;ITickSubscriber&gt;());
/// </code>
/// </example>
/// </remarks>
public interface ITickStreamG : IGrainWithStringKey
{
    /// <summary>
    /// Subscribe to tick updates.
    /// </summary>
    /// <param name="subscriber">The grain observer to receive tick batches.</param>
    /// <param name="options">Subscription options (batching, backpressure). Uses defaults if null.</param>
    /// <returns>Result indicating success/failure and subscription details.</returns>
    /// <remarks>
    /// The first subscriber triggers activation of the underlying exchange scraper.
    /// </remarks>
    Task<TickSubscriptionResult> Subscribe(
        ITickSubscriber subscriber,
        TickSubscriptionOptions? options = null);

    /// <summary>
    /// Unsubscribe from tick updates.
    /// </summary>
    /// <param name="subscriber">The subscriber to remove.</param>
    /// <returns>True if the subscriber was found and removed.</returns>
    /// <remarks>
    /// The last unsubscribe triggers deactivation of the underlying scraper.
    /// </remarks>
    Task<bool> Unsubscribe(ITickSubscriber subscriber);

    /// <summary>
    /// Explicitly request that this stream be activated.
    /// Useful for prewarming before subscribers arrive.
    /// </summary>
    /// <returns>Task that completes when the stream is active.</returns>
    Task EnsureActive();

    /// <summary>
    /// Gets the current number of active subscribers.
    /// </summary>
    /// <returns>The subscriber count.</returns>
    Task<int> GetSubscriberCount();

    /// <summary>
    /// Gets the current state of the tick stream.
    /// </summary>
    /// <returns>Stream state information.</returns>
    Task<TickStreamState> GetState();
}

/// <summary>
/// State information for a tick stream.
/// </summary>
[GenerateSerializer]
[Alias("TickStreamState")]
public sealed record TickStreamState
{
    /// <summary>
    /// Whether the stream is currently active (scraper connected).
    /// </summary>
    [Id(0)]
    public required bool IsActive { get; init; }

    /// <summary>
    /// Number of active subscribers.
    /// </summary>
    [Id(1)]
    public required int SubscriberCount { get; init; }

    /// <summary>
    /// State of the underlying scraper.
    /// </summary>
    [Id(2)]
    public required ScraperState ScraperState { get; init; }

    /// <summary>
    /// Time of the last trade received.
    /// </summary>
    [Id(3)]
    public DateTimeOffset? LastTradeTime { get; init; }

    /// <summary>
    /// Total trades delivered since activation.
    /// </summary>
    [Id(4)]
    public long TotalTradesDelivered { get; init; }

    /// <summary>
    /// Total batches delivered since activation.
    /// </summary>
    [Id(5)]
    public long TotalBatchesDelivered { get; init; }

    /// <summary>
    /// The exchange this stream is connected to.
    /// </summary>
    [Id(6)]
    public string? Exchange { get; init; }

    /// <summary>
    /// The symbol being streamed.
    /// </summary>
    [Id(7)]
    public string? Symbol { get; init; }
}

/// <summary>
/// Callback interface for scraper to deliver trades to the consumer grain.
/// </summary>
/// <remarks>
/// This is the interface that tick scrapers call to push trades into the stream.
/// The consumer grain implements this internally to receive data from its scraper.
/// </remarks>
public interface ITickScraperCallback : IGrainWithStringKey
{
    /// <summary>
    /// Called by the scraper when a batch of trades is received.
    /// </summary>
    /// <param name="batch">The trade batch from the exchange.</param>
    /// <returns>Task representing the async delivery.</returns>
    Task OnTrades(AggTradeBatch batch);

    /// <summary>
    /// Called by the scraper when its state changes.
    /// </summary>
    /// <param name="state">The new scraper state.</param>
    /// <returns>Task representing the async notification.</returns>
    Task OnScraperStateChanged(ScraperState state);
}

/// <summary>
/// Base interface for exchange-specific tick scrapers.
/// </summary>
public interface ITickScraperG : IGrainWithStringKey
{
    /// <summary>
    /// Activates the scraper to start streaming ticks.
    /// </summary>
    /// <param name="callbackKey">The grain key of the callback to deliver ticks to.</param>
    /// <returns>True if activation succeeded.</returns>
    Task<bool> Activate(string callbackKey);

    /// <summary>
    /// Deactivates the scraper, stopping the stream.
    /// </summary>
    /// <returns>Task representing the async deactivation.</returns>
    Task Deactivate();

    /// <summary>
    /// Gets whether the scraper is currently active.
    /// </summary>
    Task<bool> IsActive();

    /// <summary>
    /// Gets the current scraper state.
    /// </summary>
    Task<ScraperState> GetState();
}

/// <summary>
/// Binance Futures-specific tick scraper interface.
/// </summary>
public interface IBinanceFuturesTickScraperG : ITickScraperG
{
}

/// <summary>
/// Hyperliquid-specific tick scraper interface.
/// </summary>
public interface IHyperliquidTickScraperG : ITickScraperG
{
}
