namespace LionFire.Trading.Streaming;

/// <summary>
/// Overflow policy when a bounded buffer is full.
/// </summary>
public enum OverflowPolicy
{
    /// <summary>
    /// Drop the oldest item in the buffer to make room for the new item.
    /// Best for trading data where stale data is worthless.
    /// </summary>
    DropOldest,

    /// <summary>
    /// Drop the newest item (don't add it to the buffer).
    /// Best for audit/logging where historical data matters more.
    /// </summary>
    DropNewest,

    /// <summary>
    /// Block until space becomes available.
    /// Use with caution - can cause deadlocks in high-throughput scenarios.
    /// </summary>
    Block
}

/// <summary>
/// Configuration options for time and count-based batching.
/// </summary>
/// <param name="MaxDelay">Maximum time to wait before flushing a batch (even if not full).</param>
/// <param name="MaxBatchSize">Maximum items per batch before flushing.</param>
/// <param name="OverflowPolicy">Policy when the internal buffer is full.</param>
/// <param name="InternalBufferCapacity">Size of the internal buffer (affects memory usage).</param>
public record BatchOptions(
    TimeSpan MaxDelay,
    int MaxBatchSize,
    OverflowPolicy OverflowPolicy = OverflowPolicy.DropOldest,
    int InternalBufferCapacity = 10000)
{
    /// <summary>
    /// High throughput preset: larger batches, less frequent flushes.
    /// </summary>
    public static BatchOptions HighThroughput { get; } = new(
        MaxDelay: TimeSpan.FromMilliseconds(100),
        MaxBatchSize: 1000,
        OverflowPolicy: OverflowPolicy.DropOldest,
        InternalBufferCapacity: 50000);

    /// <summary>
    /// Low latency preset: smaller batches, frequent flushes.
    /// </summary>
    public static BatchOptions LowLatency { get; } = new(
        MaxDelay: TimeSpan.FromMilliseconds(10),
        MaxBatchSize: 50,
        OverflowPolicy: OverflowPolicy.DropOldest,
        InternalBufferCapacity: 1000);

    /// <summary>
    /// Balanced preset: moderate batch sizes and delays.
    /// </summary>
    public static BatchOptions Balanced { get; } = new(
        MaxDelay: TimeSpan.FromMilliseconds(50),
        MaxBatchSize: 200,
        OverflowPolicy: OverflowPolicy.DropOldest,
        InternalBufferCapacity: 10000);

    /// <summary>
    /// Default options.
    /// </summary>
    public static BatchOptions Default { get; } = Balanced;
}

/// <summary>
/// Configuration options for per-subscriber buffering.
/// </summary>
/// <param name="BufferCapacity">Size of the subscriber's dedicated buffer.</param>
/// <param name="OverflowPolicy">Policy when this subscriber's buffer is full.</param>
/// <param name="MaxConsecutiveFailures">Number of consecutive delivery failures before eviction.</param>
/// <param name="DeliveryTimeout">Timeout for each delivery attempt.</param>
public record SubscriberOptions(
    int BufferCapacity = 100,
    OverflowPolicy OverflowPolicy = OverflowPolicy.DropOldest,
    int MaxConsecutiveFailures = 5,
    TimeSpan? DeliveryTimeout = null)
{
    /// <summary>
    /// Default delivery timeout if not specified.
    /// </summary>
    public TimeSpan EffectiveDeliveryTimeout => DeliveryTimeout ?? TimeSpan.FromSeconds(5);
}

/// <summary>
/// Default streaming options for trading data types.
/// </summary>
public static class StreamingDefaults
{
    /// <summary>
    /// Tick data: stale data is worthless, drop oldest.
    /// </summary>
    public static SubscriberOptions TickSubscriber { get; } = new(
        BufferCapacity: 100,           // ~5 seconds at 20 batches/sec
        OverflowPolicy: OverflowPolicy.DropOldest,
        MaxConsecutiveFailures: 5,
        DeliveryTimeout: TimeSpan.FromSeconds(5));

    /// <summary>
    /// L2 orderbook: gaps detected via sequence, can recover.
    /// </summary>
    public static SubscriberOptions L2Subscriber { get; } = new(
        BufferCapacity: 50,            // Smaller buffer, gaps trigger re-snapshot
        OverflowPolicy: OverflowPolicy.DropOldest,
        MaxConsecutiveFailures: 3,
        DeliveryTimeout: TimeSpan.FromSeconds(10));

    /// <summary>
    /// Default ingestion buffer capacity (~50 seconds buffer at high throughput).
    /// </summary>
    public static int IngestBufferCapacity { get; } = 1000;
}
