namespace LionFire.Trading.Cli.Commands;

/// <summary>
/// Options for the 'backtest run' command.
/// Property names are mapped to command-line options (e.g., Bot -> --bot)
/// </summary>
public class BacktestRunOptions
{
    // Config file options
    public string? Config { get; set; }
    public string? Preset { get; set; }

    // Core options
    public string? Bot { get; set; }
    public string Symbol { get; set; } = "BTCUSDT";
    public string Exchange { get; set; } = "Binance";
    public string Area { get; set; } = "futures";
    public string Timeframe { get; set; } = "h1";
    public DateTime From { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime To { get; set; } = DateTime.UtcNow;

    // Output options
    public bool Json { get; set; }
    public bool Quiet { get; set; }

    // Backtest-specific: fixed parameter values loaded from config
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Tracks which properties were explicitly set via command line
    /// (as opposed to having default values)
    /// </summary>
    internal HashSet<string> ExplicitlySetProperties { get; } = new();
}
