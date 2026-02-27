using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Observer interface for real-time signal updates from bot harness grains.
/// Clients implement this interface to receive push notifications when bots
/// open, close, or change positions.
/// </summary>
/// <remarks>
/// Follows the same pattern as <see cref="IBotLogObserver"/>. The
/// <c>SignalCollectorG</c> grain implements this to aggregate signals.
/// </remarks>
public interface IBotSignalObserver : IGrainObserver
{
    /// <summary>
    /// Called when a bot emits a new signal (position opened, closed, or flipped).
    /// </summary>
    /// <param name="signal">The signal event with direction, strength, and metadata.</param>
    void OnSignalEmitted(BotSignalEvent signal);
}
