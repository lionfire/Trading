using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Tracks a bot harness execution session for log organization and history.
/// </summary>
[GenerateSerializer]
[Alias("bot-harness-session")]
public class BotHarnessSession
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    [Id(0)]
    public Guid SessionId { get; set; }

    /// <summary>
    /// UTC timestamp when the session started.
    /// </summary>
    [Id(1)]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// UTC timestamp when the session ended. Null if still running.
    /// </summary>
    [Id(2)]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// The bot type name that was running in this session.
    /// </summary>
    [Id(3)]
    public string BotTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Final state when the session ended.
    /// </summary>
    [Id(4)]
    public RealtimeBotState FinalState { get; set; }

    /// <summary>
    /// Relative path to the archived log file (relative to bot logs directory).
    /// </summary>
    [Id(5)]
    public string? ArchiveFilePath { get; set; }

    /// <summary>
    /// Total number of log entries captured during the session.
    /// </summary>
    [Id(6)]
    public int TotalLogCount { get; set; }

    /// <summary>
    /// Number of error-level or higher log entries.
    /// </summary>
    [Id(7)]
    public int ErrorCount { get; set; }
}
