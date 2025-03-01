namespace LionFire.Trading.Automation.Optimization.Analysis;

public class OptimizationAnalysisOptions
{
    public bool AutoAnalyze { get; set; } = true;

    public int MaxIngestErrors { get; set; } = 10;
    public string AnalyzedDir { get; set; } = @"F:\st\Investing-Output\.local\Analyzed\";

}
