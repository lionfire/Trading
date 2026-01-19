public class IngestOptions
{
    public const string ConfigurationLocation = "Trading:Ingest";

    /// <summary>
    /// Set this to true on the injestion host.
    /// Affects:
    ///  - BacktestFileMover
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Legacy backtest results root (OLD: symbol/bot/tf structure).
    /// Configured via Trading:Ingest:Windows:BacktestsRoot_Old or Trading:Ingest:Unix:BacktestsRoot_Old in appsettings.json.
    /// </summary>
    public string? BacktestsRoot_Old { get; set; }

    /// <summary>
    /// Directories containing results from multiple machines.
    /// Configured via Trading:Ingest:Windows:MultiMachineResultDirs or Trading:Ingest:Unix:MultiMachineResultDirs in appsettings.json.
    /// </summary>
    public List<string> MultiMachineResultDirs { get; set; } = [];

    //public List<string> MarketsResultDirs { get; set; } = [];
}
