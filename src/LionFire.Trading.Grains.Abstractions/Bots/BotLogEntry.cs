using Microsoft.Extensions.Logging;
using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Represents a single log entry captured from the bot harness grain.
/// </summary>
[GenerateSerializer]
[Alias("bot-log-entry")]
public class BotLogEntry
{
    /// <summary>
    /// UTC timestamp when the log entry was created.
    /// </summary>
    [Id(0)]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Log severity level.
    /// </summary>
    [Id(1)]
    public LogLevel Level { get; set; }

    /// <summary>
    /// The formatted log message.
    /// </summary>
    [Id(2)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Logger category name (typically the type name).
    /// </summary>
    [Id(3)]
    public string? Category { get; set; }

    /// <summary>
    /// Exception type name if an exception was logged.
    /// </summary>
    [Id(4)]
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Exception message if an exception was logged.
    /// </summary>
    [Id(5)]
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// Exception stack trace if an exception was logged.
    /// </summary>
    [Id(6)]
    public string? ExceptionStackTrace { get; set; }

    /// <summary>
    /// Structured properties from the log event (key-value pairs).
    /// </summary>
    [Id(7)]
    public Dictionary<string, string>? Properties { get; set; }

    /// <summary>
    /// Session ID for grouping logs by harness execution session.
    /// </summary>
    [Id(8)]
    public Guid SessionId { get; set; }
}
