namespace LionFire.Trading.Streaming;

/// <summary>
/// Metrics interface for WebSocket connection manager observability.
/// </summary>
public interface IWebSocketManagerMetrics
{
    /// <summary>
    /// Total time the connection has been up (across all reconnects).
    /// </summary>
    TimeSpan TotalUptime { get; }

    /// <summary>
    /// Number of times the connection has been re-established.
    /// </summary>
    long ReconnectCount { get; }

    /// <summary>
    /// Last measured ping round-trip time in milliseconds.
    /// </summary>
    double? LastPingLatencyMs { get; }

    /// <summary>
    /// Average ping round-trip time in milliseconds.
    /// </summary>
    double? AveragePingLatencyMs { get; }

    /// <summary>
    /// Total messages received across all streams.
    /// </summary>
    long TotalMessagesReceived { get; }

    /// <summary>
    /// Total bytes received across all streams.
    /// </summary>
    long TotalBytesReceived { get; }

    /// <summary>
    /// Time since the last message was received.
    /// </summary>
    TimeSpan TimeSinceLastMessage { get; }

    /// <summary>
    /// Current connection state.
    /// </summary>
    ConnectionState CurrentState { get; }

    /// <summary>
    /// Time when the current connection was established.
    /// </summary>
    DateTime? ConnectedSince { get; }

    /// <summary>
    /// Active subscription count.
    /// </summary>
    int ActiveSubscriptions { get; }

    /// <summary>
    /// Active WebSocket connection count.
    /// </summary>
    int ActiveConnections { get; }
}
