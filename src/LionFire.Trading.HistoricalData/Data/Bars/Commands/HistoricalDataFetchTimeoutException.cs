namespace LionFire.Trading.HistoricalData.Retrieval;

/// <summary>
/// Thrown when fetching historical data from an exchange has stalled beyond acceptable limits.
/// </summary>
public class HistoricalDataFetchTimeoutException : Exception
{
    /// <summary>
    /// The exchange that timed out (e.g., "binance").
    /// </summary>
    public string? Exchange { get; init; }

    /// <summary>
    /// How long since any progress was made.
    /// </summary>
    public TimeSpan? StallDuration { get; init; }

    /// <summary>
    /// Symbol being retrieved when the timeout occurred.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Number of retry attempts made before giving up.
    /// </summary>
    public int RetryAttemptsExhausted { get; init; }

    public HistoricalDataFetchTimeoutException() { }
    public HistoricalDataFetchTimeoutException(string message) : base(message) { }
    public HistoricalDataFetchTimeoutException(string message, Exception inner) : base(message, inner) { }
}
