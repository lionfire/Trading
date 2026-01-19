namespace LionFire.Trading.Automation.Optimization.Analysis;

public class OptimizationAnalysisOptions
{
    public const string ConfigurationLocation = "Trading:OptimizationAnalysis";

    public bool AutoAnalyze { get; set; } = true;

    public int MaxIngestErrors { get; set; } = 10;

    /// <summary>
    /// Directory for storing analyzed optimization results.
    /// Configured via Trading:OptimizationAnalysis:Windows:AnalyzedDir or Trading:OptimizationAnalysis:Unix:AnalyzedDir in appsettings.json.
    /// </summary>
    public string? AnalyzedDir { get; set; }

}
