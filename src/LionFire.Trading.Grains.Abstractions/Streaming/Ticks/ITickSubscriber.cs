using Orleans;

namespace LionFire.Trading.Streaming;

/// <summary>
/// Observer interface for receiving aggregated trade (tick) updates from tick stream grains.
/// </summary>
/// <remarks>
/// Implementations receive batched trade data for efficient processing.
/// The callback should complete quickly to avoid blocking other subscribers.
/// Use internal buffering if processing is slow.
///
/// <example>
/// <code>
/// public class MyTickConsumer : Grain, IMyTickConsumerG, ITickSubscriber
/// {
///     public Task OnTrades(AggTradeBatch batch)
///     {
///         foreach (var trade in batch.Trades)
///         {
///             // Process trade
///             Console.WriteLine($"{trade.Timestamp}: {trade.Price} x {trade.Quantity}");
///         }
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface ITickSubscriber : IGrainObserver
{
    /// <summary>
    /// Called when a batch of trades is received.
    /// </summary>
    /// <param name="batch">The batch of aggregated trades.</param>
    /// <returns>A task representing the async operation.</returns>
    /// <remarks>
    /// Important: This method should return quickly. If processing is slow,
    /// buffer internally and process asynchronously to avoid blocking other subscribers.
    /// </remarks>
    Task OnTrades(AggTradeBatch batch);
}

/// <summary>
/// Options for tick stream subscriptions.
/// </summary>
[GenerateSerializer]
[Alias("TickSubscriptionOptions")]
public sealed record TickSubscriptionOptions
{
    /// <summary>
    /// Maximum number of trades per batch delivered to this subscriber.
    /// Default: 100
    /// </summary>
    [Id(0)]
    public int MaxBatchSize { get; init; } = 100;

    /// <summary>
    /// Maximum delay before flushing a partial batch.
    /// Default: 50ms
    /// </summary>
    [Id(1)]
    public TimeSpan MaxBatchDelay { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Whether to drop trades on backpressure rather than blocking.
    /// Default: true (drop old data, keep subscriber responsive)
    /// </summary>
    [Id(2)]
    public bool DropOnBackpressure { get; init; } = true;

    /// <summary>
    /// Buffer capacity for this subscriber.
    /// Default: 100 batches
    /// </summary>
    [Id(3)]
    public int BufferCapacity { get; init; } = 100;

    /// <summary>
    /// Default subscription options.
    /// </summary>
    public static TickSubscriptionOptions Default { get; } = new();

    /// <summary>
    /// Low-latency options for fast subscribers.
    /// </summary>
    public static TickSubscriptionOptions LowLatency { get; } = new()
    {
        MaxBatchSize = 10,
        MaxBatchDelay = TimeSpan.FromMilliseconds(10),
        DropOnBackpressure = true,
        BufferCapacity = 50
    };

    /// <summary>
    /// High-throughput options for batch processing.
    /// </summary>
    public static TickSubscriptionOptions HighThroughput { get; } = new()
    {
        MaxBatchSize = 500,
        MaxBatchDelay = TimeSpan.FromMilliseconds(100),
        DropOnBackpressure = true,
        BufferCapacity = 200
    };
}

/// <summary>
/// Result returned from subscription operations.
/// </summary>
[GenerateSerializer]
[Alias("TickSubscriptionResult")]
public sealed record TickSubscriptionResult
{
    /// <summary>
    /// Whether the subscription was successful.
    /// </summary>
    [Id(0)]
    public required bool Success { get; init; }

    /// <summary>
    /// Unique identifier for this subscription (for tracking and unsubscribe).
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
    public ScraperState ScraperState { get; init; }

    /// <summary>
    /// Creates a successful subscription result.
    /// </summary>
    public static TickSubscriptionResult Succeeded(Guid subscriptionId, bool isFirst, ScraperState state) => new()
    {
        Success = true,
        SubscriptionId = subscriptionId,
        IsFirstSubscriber = isFirst,
        ScraperState = state
    };

    /// <summary>
    /// Creates a failed subscription result.
    /// </summary>
    public static TickSubscriptionResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        ScraperState = ScraperState.Unknown
    };
}

/// <summary>
/// State of the tick scraper.
/// </summary>
public enum ScraperState
{
    /// <summary>State unknown.</summary>
    Unknown,

    /// <summary>Scraper is not active.</summary>
    Inactive,

    /// <summary>Scraper is connecting to exchange.</summary>
    Connecting,

    /// <summary>Scraper is active and streaming data.</summary>
    Active,

    /// <summary>Scraper is reconnecting after disconnection.</summary>
    Reconnecting,

    /// <summary>Scraper has failed and is not streaming.</summary>
    Failed
}
