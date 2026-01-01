using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Observer interface for real-time log updates from bot harness grains.
/// Clients implement this interface to receive push notifications of new logs.
/// </summary>
public interface IBotLogObserver : IGrainObserver
{
    /// <summary>
    /// Called when new log entries are available.
    /// </summary>
    /// <param name="entries">Batch of new log entries.</param>
    void OnLogsReceived(List<BotLogEntry> entries);

    /// <summary>
    /// Called when the harness state changes.
    /// </summary>
    /// <param name="newState">The new harness state.</param>
    void OnStateChanged(RealtimeBotState newState);
}
