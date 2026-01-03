namespace LionFire.Trading.Indicators.Grains;

/// <summary>
/// Unified interface for all indicator grains.
/// Uses a single generic grain that can handle any indicator type.
/// </summary>
/// <remarks>
/// <para>
/// Grain ID format: {exchange}:{area}:{symbol}:{timeframe}:{indicatorKey}
/// Where indicatorKey is like: SMA(14), EMA(21), RSI(14), MACD(12,26,9)
/// </para>
/// <para>
/// Example: binance:futures:BTCUSDT:m1:SMA(20)
/// </para>
/// <para>
/// For composite indicators that depend on other indicators:
/// Example: binance:futures:BTCUSDT:m1:LC(8;RSI(14),ATR(14))
/// </para>
/// </remarks>
public interface IIndicatorG : IGrainWithStringKey
{
    /// <summary>
    /// Gets the current (most recent) indicator value.
    /// Returns null if no value has been calculated yet.
    /// </summary>
    ValueTask<double?> GetCurrentValue();

    /// <summary>
    /// Gets indicator values for a date range.
    /// </summary>
    /// <param name="start">Start of the range (inclusive)</param>
    /// <param name="endExclusive">End of the range (exclusive)</param>
    /// <returns>Indicator values in chronological order, or null if no data available</returns>
    ValueTask<IEnumerable<(DateTimeOffset Time, double Value)>?> GetValues(DateTimeOffset start, DateTimeOffset endExclusive);

    /// <summary>
    /// Gets the most recent N indicator values.
    /// </summary>
    /// <param name="count">Number of values to retrieve</param>
    /// <returns>Most recent values in chronological order (oldest first), or null if no data</returns>
    ValueTask<IEnumerable<(DateTimeOffset Time, double Value)>?> GetRecentValues(int count);

    /// <summary>
    /// Returns whether this grain is receiving live data updates.
    /// </summary>
    ValueTask<bool> IsLive();

    /// <summary>
    /// Returns whether the indicator has completed its warmup period and is producing valid values.
    /// </summary>
    ValueTask<bool> IsReady();

    /// <summary>
    /// Returns the available date range for this indicator.
    /// </summary>
    /// <returns>Tuple of (earliest, latest) timestamps, either can be null if no data</returns>
    ValueTask<(DateTimeOffset?, DateTimeOffset?)> AvailableRange();

    /// <summary>
    /// Gets metadata about this indicator (type, category, period, etc.).
    /// </summary>
    ValueTask<IndicatorMetadata> GetMetadata();

    #region Observer Pattern

    /// <summary>
    /// Subscribes an observer to receive indicator value updates.
    /// </summary>
    /// <param name="observer">The observer to receive notifications</param>
    /// <returns>A subscription handle that can be used to unsubscribe</returns>
    ValueTask<Guid> Subscribe(IIndicatorSubscriber observer);

    /// <summary>
    /// Unsubscribes an observer from receiving indicator value updates.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID returned from Subscribe</param>
    ValueTask Unsubscribe(Guid subscriptionId);

    /// <summary>
    /// Gets the current number of active subscribers.
    /// </summary>
    ValueTask<int> GetSubscriberCount();

    #endregion
}

#region Legacy interfaces - kept for compatibility

/// <summary>
/// Legacy interface for typed indicator grains.
/// Use IIndicatorG instead for new code.
/// </summary>
[Obsolete("Use IIndicatorG instead")]
public interface IIndicatorG<TValue> : IGrainWithStringKey
{
    ValueTask<TValue?> GetCurrentValue();
    ValueTask<IEnumerable<TValue>?> GetValues(DateTimeOffset start, DateTimeOffset endExclusive);
    ValueTask<bool> IsLive();
    ValueTask<(DateTimeOffset?, DateTimeOffset?)> AvailableRange();
}

#endregion
