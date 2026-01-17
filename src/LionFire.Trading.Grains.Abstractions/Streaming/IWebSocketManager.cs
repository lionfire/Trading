namespace LionFire.Trading.Streaming;

/// <summary>
/// Manages shared WebSocket connections for real-time market data streaming.
/// Supports connection pooling and multiplexing of multiple streams over shared connections.
/// </summary>
public interface IWebSocketManager
{
    /// <summary>
    /// Subscribes to a specific stream (e.g., "@aggTrade", "@depth").
    /// </summary>
    /// <param name="stream">The stream name to subscribe to (e.g., "btcusdt@aggTrade").</param>
    /// <param name="onMessage">Callback invoked when a message is received for this stream.</param>
    /// <param name="cancellationToken">Token to cancel the subscription.</param>
    /// <returns>A subscription handle that can be used to unsubscribe.</returns>
    Task<IWebSocketSubscription> SubscribeAsync(
        string stream,
        Action<string> onMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribes from a stream.
    /// </summary>
    /// <param name="subscription">The subscription handle returned from <see cref="SubscribeAsync"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UnsubscribeAsync(IWebSocketSubscription subscription);

    /// <summary>
    /// Observable that emits connection state changes.
    /// </summary>
    IObservable<ConnectionState> ConnectionStateChanges { get; }

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    ConnectionState CurrentState { get; }

    /// <summary>
    /// Gets the count of active subscriptions.
    /// </summary>
    int ActiveSubscriptionCount { get; }

    /// <summary>
    /// Gets the count of active WebSocket connections.
    /// </summary>
    int ActiveConnectionCount { get; }
}

/// <summary>
/// Represents an active subscription to a WebSocket stream.
/// </summary>
public interface IWebSocketSubscription : IAsyncDisposable
{
    /// <summary>
    /// Gets the stream name this subscription is for.
    /// </summary>
    string Stream { get; }

    /// <summary>
    /// Gets whether this subscription is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the time when this subscription was created.
    /// </summary>
    DateTime SubscribedAt { get; }

    /// <summary>
    /// Gets the count of messages received on this subscription.
    /// </summary>
    long MessageCount { get; }
}

/// <summary>
/// Connection state for WebSocket manager.
/// </summary>
public enum ConnectionState
{
    /// <summary>Not connected to any WebSocket.</summary>
    Disconnected,

    /// <summary>Attempting to connect.</summary>
    Connecting,

    /// <summary>Connected and ready to receive data.</summary>
    Connected,

    /// <summary>Connection was lost, attempting to reconnect.</summary>
    Reconnecting,

    /// <summary>Permanently failed (max retries exceeded).</summary>
    Failed
}

/// <summary>
/// Configuration options for WebSocket connections.
/// </summary>
public record WebSocketOptions
{
    /// <summary>
    /// Maximum number of streams per WebSocket connection.
    /// Binance limit is 200 streams per connection.
    /// </summary>
    public int MaxStreamsPerConnection { get; init; } = 200;

    /// <summary>
    /// Initial delay before first reconnect attempt.
    /// </summary>
    public TimeSpan InitialReconnectDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between reconnect attempts.
    /// </summary>
    public TimeSpan MaxReconnectDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of reconnect attempts before giving up.
    /// </summary>
    public int MaxReconnectAttempts { get; init; } = 10;

    /// <summary>
    /// Timeout for ping/pong heartbeat.
    /// </summary>
    public TimeSpan PingTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Interval between ping/pong heartbeats.
    /// </summary>
    public TimeSpan PingInterval { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Default options for standard use.
    /// </summary>
    public static WebSocketOptions Default { get; } = new();
}
