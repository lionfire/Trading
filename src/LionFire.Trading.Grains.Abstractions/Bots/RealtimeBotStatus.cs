using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Status information for a realtime bot harness, including current state and progress.
/// </summary>
[GenerateSerializer]
[Alias("realtime-bot-status")]
public class RealtimeBotStatus
{
    /// <summary>
    /// Current lifecycle state of the bot harness.
    /// </summary>
    [Id(0)]
    public RealtimeBotState State { get; set; }

    /// <summary>
    /// Timestamp of the last bar processed by the bot.
    /// Null if no bars have been processed yet.
    /// </summary>
    [Id(1)]
    public DateTime? LastBarTime { get; set; }

    /// <summary>
    /// Error message if the bot is in Faulted state.
    /// Null if no error has occurred.
    /// </summary>
    [Id(2)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of historical bars processed during catch-up phase.
    /// Used to track progress when transitioning from Starting to Running state.
    /// </summary>
    [Id(3)]
    public int BarsCaughtUp { get; set; }
}

/// <summary>
/// Lifecycle states for a realtime bot harness.
/// </summary>
public enum RealtimeBotState
{
    /// <summary>
    /// Bot harness is stopped and not processing any data.
    /// This is the initial state and the state after Stop() is called.
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// Bot harness is starting up and initializing subscriptions.
    /// Transitioning from Stopped to CatchingUp.
    /// </summary>
    Starting = 1,

    /// <summary>
    /// Bot harness is catching up on historical bars from the last saved position
    /// to the current real-time position. BarsCaughtUp tracks progress.
    /// Transitions to Running when caught up.
    /// </summary>
    CatchingUp = 2,

    /// <summary>
    /// Bot harness is running and processing real-time bar updates.
    /// This is the normal operational state.
    /// </summary>
    Running = 3,

    /// <summary>
    /// Bot harness encountered an error and is in a faulted state.
    /// ErrorMessage contains details. Requires Stop() and restart to recover.
    /// </summary>
    Faulted = 4
}
