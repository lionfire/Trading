namespace LionFire.Trading.Hosting;

/// <summary>
/// Configuration options for Trading logging.
/// </summary>
public class TradingLoggingOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Trading:Logging";

    /// <summary>
    /// Base directory for log files.
    /// If not set, defaults to %AppData%/LionFire/Trading on Windows
    /// or ~/.local/share/LionFire/Trading on Linux.
    /// </summary>
    public string? LogDir { get; set; }

    /// <summary>
    /// Whether to write JSONL (structured) log files. Default: true.
    /// </summary>
    public bool WriteJsonl { get; set; } = true;

    /// <summary>
    /// Whether to write human-readable text log files. Default: true.
    /// </summary>
    public bool WriteText { get; set; } = true;

    /// <summary>
    /// Number of log sessions to keep per bot. When set, older log files are deleted
    /// when creating new sessions. A value of 0 means keep none (delete all old logs).
    /// Null disables cleanup (keep all logs). Default: null (cleanup disabled).
    /// </summary>
    public int? KeepLastNLogs { get; set; }

    /// <summary>
    /// Gets the effective log directory, using the configured path or the default.
    /// </summary>
    public string GetEffectiveLogDir()
    {
        if (!string.IsNullOrWhiteSpace(LogDir))
        {
            return LogDir;
        }

        // Default to AppData/LionFire/Trading
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LionFire", "Trading");
    }
}
