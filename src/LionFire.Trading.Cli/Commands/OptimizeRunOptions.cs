namespace LionFire.Trading.Cli.Commands;

/// <summary>
/// Options for the 'optimize run' command.
/// Property names are mapped to command-line options (e.g., Bot -> --bot)
/// </summary>
public class OptimizeRunOptions
{
    public string? Bot { get; set; }
    public string Symbol { get; set; } = "BTCUSDT";
    public string Exchange { get; set; } = "Binance";
    public string Area { get; set; } = "futures";
    public string Timeframe { get; set; } = "h1";
    public DateTime From { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime To { get; set; } = DateTime.UtcNow;
    public int ProgressInterval { get; set; } = 5;
    public bool Json { get; set; }
    public bool Quiet { get; set; }
    public long MaxBacktests { get; set; } = 50000;
    public int BatchSize { get; set; } = 1024;
}
