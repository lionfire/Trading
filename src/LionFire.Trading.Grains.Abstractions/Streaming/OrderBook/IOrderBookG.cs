using Orleans;

namespace LionFire.Trading.Streaming;

/// <summary>
/// Consumer grain for L2 orderbook streaming.
/// Manages subscriptions and activates exchange-specific L2 scrapers on demand.
/// </summary>
/// <remarks>
/// Grain key format: "{exchange}:{symbol}" (e.g., "binance.futures:BTCUSDT", "hyperliquid:BTC")
///
/// This grain follows the demand-driven activation pattern:
/// - Activates the appropriate exchange L2 scraper when the first subscriber arrives
/// - Deactivates the scraper when the last subscriber leaves
/// - Routes incoming orderbook updates from the scraper to all subscribers
/// - Maintains local orderbook state for GetCurrentSnapshot()
///
/// <example>
/// <code>
/// // Subscribe to BTCUSDT L2 orderbook
/// var orderBook = grainFactory.GetGrain&lt;IOrderBookG&gt;("binance.futures:BTCUSDT");
/// var result = await orderBook.Subscribe(this.AsReference&lt;IOrderBookSubscriber&gt;());
///
/// // Get current state
/// var snapshot = await orderBook.GetCurrentSnapshot();
/// Console.WriteLine($"Best bid: {snapshot.BestBid}, Best ask: {snapshot.BestAsk}");
///
/// // Later, unsubscribe
/// await orderBook.Unsubscribe(this.AsReference&lt;IOrderBookSubscriber&gt;());
/// </code>
/// </example>
/// </remarks>
public interface IOrderBookG : IGrainWithStringKey
{
    /// <summary>
    /// Subscribe to orderbook updates.
    /// </summary>
    /// <param name="subscriber">The grain observer to receive orderbook updates.</param>
    /// <param name="options">Subscription options (depth, snapshot delivery). Uses defaults if null.</param>
    /// <returns>Result indicating success/failure and subscription details.</returns>
    /// <remarks>
    /// The first subscriber triggers activation of the underlying exchange scraper.
    /// If IncludeSnapshot is true, the initial snapshot is included in the result.
    /// </remarks>
    Task<OrderBookSubscriptionResult> Subscribe(
        IOrderBookSubscriber subscriber,
        OrderBookSubscriptionOptions? options = null);

    /// <summary>
    /// Unsubscribe from orderbook updates.
    /// </summary>
    /// <param name="subscriber">The subscriber to remove.</param>
    /// <returns>True if the subscriber was found and removed.</returns>
    /// <remarks>
    /// The last unsubscribe triggers deactivation of the underlying scraper.
    /// </remarks>
    Task<bool> Unsubscribe(IOrderBookSubscriber subscriber);

    /// <summary>
    /// Explicitly request that this orderbook stream be activated.
    /// Useful for prewarming before subscribers arrive.
    /// </summary>
    /// <param name="depth">The depth to pre-warm with. Default: 20</param>
    /// <returns>Task that completes when the orderbook is ready.</returns>
    Task EnsureActive(int depth = 20);

    /// <summary>
    /// Gets the current orderbook snapshot.
    /// </summary>
    /// <returns>The current orderbook state, or null if not active.</returns>
    Task<OrderBookSnapshot?> GetCurrentSnapshot();

    /// <summary>
    /// Gets the current number of active subscribers.
    /// </summary>
    /// <returns>The subscriber count.</returns>
    Task<int> GetSubscriberCount();

    /// <summary>
    /// Gets the current state of the orderbook stream.
    /// </summary>
    /// <returns>Stream state information.</returns>
    Task<OrderBookStreamState> GetState();
}

/// <summary>
/// State information for an orderbook stream.
/// </summary>
[GenerateSerializer]
[Alias("OrderBookStreamState")]
public sealed record OrderBookStreamState
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
    public required OrderBookScraperState ScraperState { get; init; }

    /// <summary>
    /// Time of the last update received.
    /// </summary>
    [Id(3)]
    public DateTimeOffset? LastUpdateTime { get; init; }

    /// <summary>
    /// Total snapshots delivered since activation.
    /// </summary>
    [Id(4)]
    public long TotalSnapshotsDelivered { get; init; }

    /// <summary>
    /// Total deltas delivered since activation.
    /// </summary>
    [Id(5)]
    public long TotalDeltasDelivered { get; init; }

    /// <summary>
    /// Current active depth level.
    /// </summary>
    [Id(6)]
    public int ActiveDepth { get; init; }

    /// <summary>
    /// The exchange this stream is connected to.
    /// </summary>
    [Id(7)]
    public string? Exchange { get; init; }

    /// <summary>
    /// The symbol being streamed.
    /// </summary>
    [Id(8)]
    public string? Symbol { get; init; }

    /// <summary>
    /// Current best bid price.
    /// </summary>
    [Id(9)]
    public decimal? BestBid { get; init; }

    /// <summary>
    /// Current best ask price.
    /// </summary>
    [Id(10)]
    public decimal? BestAsk { get; init; }
}

/// <summary>
/// Callback interface for L2 scraper to deliver updates to the consumer grain.
/// </summary>
/// <remarks>
/// This is the interface that L2 scrapers call to push orderbook updates into the stream.
/// The consumer grain implements this internally to receive data from its scraper.
/// </remarks>
public interface IOrderBookScraperCallback : IGrainWithStringKey
{
    /// <summary>
    /// Called by the scraper when a full snapshot is received.
    /// </summary>
    /// <param name="snapshot">The full orderbook snapshot.</param>
    /// <returns>Task representing the async delivery.</returns>
    Task OnSnapshot(OrderBookSnapshot snapshot);

    /// <summary>
    /// Called by the scraper when an incremental update is received.
    /// </summary>
    /// <param name="delta">The incremental changes.</param>
    /// <returns>Task representing the async delivery.</returns>
    Task OnDelta(OrderBookDelta delta);

    /// <summary>
    /// Called by the scraper when its state changes.
    /// </summary>
    /// <param name="state">The new scraper state.</param>
    /// <returns>Task representing the async notification.</returns>
    Task OnScraperStateChanged(OrderBookScraperState state);
}

/// <summary>
/// Base interface for exchange-specific L2 orderbook scrapers.
/// </summary>
public interface IOrderBookScraperG : IGrainWithStringKey
{
    /// <summary>
    /// Activates the scraper to start streaming L2 data.
    /// </summary>
    /// <param name="callbackKey">The grain key of the callback to deliver updates to.</param>
    /// <param name="depth">The depth level to stream (5, 10, 20, etc.).</param>
    /// <returns>True if activation succeeded.</returns>
    Task<bool> Activate(string callbackKey, int depth);

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
    Task<OrderBookScraperState> GetState();

    /// <summary>
    /// Gets the active depth level.
    /// </summary>
    Task<int> GetActiveDepth();

    /// <summary>
    /// Requests a fresh snapshot (triggers re-sync).
    /// </summary>
    Task RequestSnapshot();
}

/// <summary>
/// Binance Futures-specific L2 orderbook scraper interface.
/// </summary>
/// <remarks>
/// Grain key format: "{symbol}" (e.g., "BTCUSDT")
/// </remarks>
public interface IBinanceFuturesL2ScraperG : IOrderBookScraperG
{
}

/// <summary>
/// Hyperliquid-specific L2 orderbook scraper interface.
/// </summary>
/// <remarks>
/// Grain key format: "{symbol}" (e.g., "BTC")
/// </remarks>
public interface IHyperliquidL2ScraperG : IOrderBookScraperG
{
}
