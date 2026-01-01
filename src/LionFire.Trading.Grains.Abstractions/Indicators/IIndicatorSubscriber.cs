using Orleans;

namespace LionFire.Trading.Indicators.Grains;

/// <summary>
/// Observer interface for receiving indicator value updates from indicator grains.
/// </summary>
/// <remarks>
/// Implementations can receive:
/// - New indicator values as they are calculated
/// - Ready notifications when the indicator has caught up to real-time
/// - Error notifications for processing failures
/// </remarks>
public interface IIndicatorSubscriber : IGrainObserver
{
    /// <summary>
    /// Called when a new indicator value is calculated.
    /// </summary>
    /// <param name="indicatorKey">The key identifying the indicator</param>
    /// <param name="value">The calculated indicator value</param>
    /// <param name="barTime">The timestamp of the bar this value corresponds to</param>
    void OnValue(string indicatorKey, object value, DateTime barTime);

    /// <summary>
    /// Called when the indicator has caught up to real-time data and is ready.
    /// </summary>
    /// <param name="indicatorKey">The key identifying the indicator</param>
    void OnReady(string indicatorKey);

    /// <summary>
    /// Called when an error occurs during indicator processing.
    /// </summary>
    /// <param name="indicatorKey">The key identifying the indicator</param>
    /// <param name="error">The exception that occurred</param>
    void OnError(string indicatorKey, Exception error);
}

/// <summary>
/// Typed observer interface for receiving strongly-typed indicator value updates.
/// </summary>
/// <typeparam name="TOutput">The type of indicator value being observed</typeparam>
public interface IIndicatorSubscriber<TOutput> : IGrainObserver
{
    /// <summary>
    /// Called when a new indicator value is calculated.
    /// </summary>
    /// <param name="indicatorKey">The key identifying the indicator</param>
    /// <param name="value">The calculated indicator value</param>
    /// <param name="barTime">The timestamp of the bar this value corresponds to</param>
    void OnValue(string indicatorKey, TOutput value, DateTime barTime);

    /// <summary>
    /// Called when the indicator has caught up to real-time data and is ready.
    /// </summary>
    /// <param name="indicatorKey">The key identifying the indicator</param>
    void OnReady(string indicatorKey);

    /// <summary>
    /// Called when an error occurs during indicator processing.
    /// </summary>
    /// <param name="indicatorKey">The key identifying the indicator</param>
    /// <param name="error">The exception that occurred</param>
    void OnError(string indicatorKey, Exception error);
}
