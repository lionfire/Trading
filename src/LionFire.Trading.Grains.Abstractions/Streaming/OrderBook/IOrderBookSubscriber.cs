using Orleans;

namespace LionFire.Trading.Streaming;

/// <summary>
/// Observer interface for receiving orderbook updates from L2 stream grains.
/// </summary>
/// <remarks>
/// Implementations receive both snapshots (full state) and deltas (incremental updates).
/// The callback should complete quickly to avoid blocking other subscribers.
/// Use internal buffering if processing is slow.
///
/// <example>
/// <code>
/// public class MyOrderBookConsumer : Grain, IMyOrderBookConsumerG, IOrderBookSubscriber
/// {
///     public Task OnSnapshot(OrderBookSnapshot snapshot)
///     {
///         // Replace entire orderbook state
///         _orderBook = new MutableOrderBook(snapshot);
///         Console.WriteLine($"Snapshot: {snapshot.BestBid} / {snapshot.BestAsk}");
///         return Task.CompletedTask;
///     }
///
///     public Task OnDelta(OrderBookDelta delta)
///     {
///         // Apply incremental update
///         _orderBook.ApplyDelta(delta);
///         Console.WriteLine($"Delta: {delta.UpdateCount} updates");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IOrderBookSubscriber : IGrainObserver
{
    /// <summary>
    /// Called when a full orderbook snapshot is received.
    /// </summary>
    /// <param name="snapshot">The complete orderbook state.</param>
    /// <returns>A task representing the async operation.</returns>
    /// <remarks>
    /// Snapshots are delivered:
    /// - On initial subscription
    /// - After detecting a sequence gap
    /// - On WebSocket reconnection
    /// </remarks>
    Task OnSnapshot(OrderBookSnapshot snapshot);

    /// <summary>
    /// Called when an incremental orderbook update is received.
    /// </summary>
    /// <param name="delta">The incremental changes to apply.</param>
    /// <returns>A task representing the async operation.</returns>
    /// <remarks>
    /// Deltas should be applied in sequence using the update IDs.
    /// If a gap is detected, wait for the next snapshot.
    /// </remarks>
    Task OnDelta(OrderBookDelta delta);
}

/// <summary>
/// Options for orderbook subscriptions.
/// </summary>
[GenerateSerializer]
[Alias("OrderBookSubscriptionOptions")]
public sealed record OrderBookSubscriptionOptions
{
    /// <summary>
    /// Requested depth for the orderbook (number of price levels per side).
    /// Supported values: 5, 10, 20, 50, 100, 500, 1000 (exchange dependent).
    /// Default: 20
    /// </summary>
    [Id(0)]
    public int Depth { get; init; } = 20;

    /// <summary>
    /// Whether to include the initial snapshot in the subscription result.
    /// Default: true
    /// </summary>
    [Id(1)]
    public bool IncludeSnapshot { get; init; } = true;

    /// <summary>
    /// Minimum interval between delta deliveries (for rate limiting).
    /// Default: 0ms (no throttling)
    /// </summary>
    [Id(2)]
    public TimeSpan UpdateThrottle { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Interval for periodic re-snapshots to ensure consistency.
    /// Default: null (no periodic snapshots)
    /// </summary>
    [Id(3)]
    public TimeSpan? SnapshotInterval { get; init; }

    /// <summary>
    /// Default subscription options.
    /// </summary>
    public static OrderBookSubscriptionOptions Default { get; } = new();

    /// <summary>
    /// Shallow depth options for fast best bid/ask updates.
    /// </summary>
    public static OrderBookSubscriptionOptions Shallow { get; } = new()
    {
        Depth = 5
    };

    /// <summary>
    /// Deep depth options for detailed analysis.
    /// </summary>
    public static OrderBookSubscriptionOptions Deep { get; } = new()
    {
        Depth = 100
    };

    /// <summary>
    /// Validates the depth value.
    /// </summary>
    public bool IsValidDepth => Depth is 5 or 10 or 20 or 50 or 100 or 500 or 1000;
}

/// <summary>
/// Result returned from orderbook subscription operations.
/// </summary>
[GenerateSerializer]
[Alias("OrderBookSubscriptionResult")]
public sealed record OrderBookSubscriptionResult
{
    /// <summary>
    /// Whether the subscription was successful.
    /// </summary>
    [Id(0)]
    public required bool Success { get; init; }

    /// <summary>
    /// Unique identifier for this subscription.
    /// </summary>
    [Id(1)]
    public Guid SubscriptionId { get; init; }

    /// <summary>
    /// Error message if subscription failed.
    /// </summary>
    [Id(2)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Whether this subscriber triggered the scraper to activate.
    /// </summary>
    [Id(3)]
    public bool IsFirstSubscriber { get; init; }

    /// <summary>
    /// Current state of the underlying scraper.
    /// </summary>
    [Id(4)]
    public OrderBookScraperState ScraperState { get; init; }

    /// <summary>
    /// The initial snapshot, if requested.
    /// </summary>
    [Id(5)]
    public OrderBookSnapshot? InitialSnapshot { get; init; }

    /// <summary>
    /// The actual depth being streamed (may differ from requested).
    /// </summary>
    [Id(6)]
    public int ActiveDepth { get; init; }

    /// <summary>
    /// Creates a successful subscription result.
    /// </summary>
    public static OrderBookSubscriptionResult Succeeded(
        Guid subscriptionId,
        bool isFirst,
        OrderBookScraperState state,
        int activeDepth,
        OrderBookSnapshot? snapshot = null) => new()
    {
        Success = true,
        SubscriptionId = subscriptionId,
        IsFirstSubscriber = isFirst,
        ScraperState = state,
        ActiveDepth = activeDepth,
        InitialSnapshot = snapshot
    };

    /// <summary>
    /// Creates a failed subscription result.
    /// </summary>
    public static OrderBookSubscriptionResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        ScraperState = OrderBookScraperState.Unknown
    };
}
