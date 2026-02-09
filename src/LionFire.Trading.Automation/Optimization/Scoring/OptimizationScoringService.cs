using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Service for calculating and managing optimization scores.
/// Bridges between the journal-based scoring infrastructure and the result storage system.
/// </summary>
public class OptimizationScoringService
{
    /// <summary>
    /// Calculates a score for an optimization run using journal entries.
    /// </summary>
    /// <param name="entries">The backtest journal entries to score</param>
    /// <param name="formula">The scoring formula (default: countWhere(ad >= 1.0))</param>
    /// <param name="threshold">Threshold for summary statistics</param>
    /// <returns>The calculated optimization score</returns>
    public OptimizationScore CalculateScore(
        IEnumerable<BacktestBatchJournalEntry> entries,
        string formula = "countWhere(ad >= 1.0)",
        double threshold = 1.0)
    {
        var scorer = new OptimizationScorer(entries);
        return scorer.Calculate(formula, threshold);
    }

    /// <summary>
    /// Calculates a score from a CSV file in an optimization output directory.
    /// </summary>
    /// <param name="outputDirectory">Directory containing backtests.csv</param>
    /// <param name="formula">The scoring formula</param>
    /// <param name="threshold">Threshold for summary statistics</param>
    /// <returns>The calculated optimization score, or null if no results found</returns>
    public OptimizationScore? CalculateScoreFromDirectory(
        string outputDirectory,
        string formula = "countWhere(ad >= 1.0)",
        double threshold = 1.0)
    {
        var entries = BacktestResultsReader.ReadFromDirectory(outputDirectory);
        if (entries.Count == 0)
            return null;

        return CalculateScore(entries, formula, threshold);
    }

    /// <summary>
    /// Evaluates a formula against backtest entries and returns just the score value.
    /// </summary>
    /// <param name="entries">The backtest journal entries</param>
    /// <param name="formula">The scoring formula</param>
    /// <returns>The calculated score value</returns>
    public double EvaluateFormula(IEnumerable<BacktestBatchJournalEntry> entries, string formula)
    {
        var parser = new ScoringFormulaParser(entries);
        return parser.Evaluate(formula);
    }

    /// <summary>
    /// Gets the AD histogram for a set of journal entries.
    /// </summary>
    public AdHistogram GetHistogram(IEnumerable<BacktestBatchJournalEntry> entries)
    {
        var adValues = AdCalculator.ExtractAdValues(entries);
        return HistogramGenerator.GenerateAdHistogram(adValues);
    }

    /// <summary>
    /// Gets the text-based histogram visualization.
    /// </summary>
    public string GetTextHistogram(IEnumerable<BacktestBatchJournalEntry> entries, int maxBarWidth = 30)
    {
        var histogram = GetHistogram(entries);
        return HistogramGenerator.GenerateTextHistogram(histogram, maxBarWidth);
    }

    /// <summary>
    /// Calculates summary statistics for a set of journal entries.
    /// </summary>
    public ScoreSummary GetSummary(IEnumerable<BacktestBatchJournalEntry> entries, double threshold = 1.0)
    {
        var adValues = AdCalculator.ExtractAdValues(entries);
        return AdCalculator.CalculateSummary(adValues, threshold);
    }

    /// <summary>
    /// Evaluates multiple formulas and returns a dictionary of results.
    /// </summary>
    public Dictionary<string, double> EvaluateFormulas(
        IEnumerable<BacktestBatchJournalEntry> entries,
        IEnumerable<string> formulas)
    {
        var parser = new ScoringFormulaParser(entries);
        var results = new Dictionary<string, double>();

        foreach (var formula in formulas)
        {
            try
            {
                results[formula] = parser.Evaluate(formula);
            }
            catch (FormulaParseException)
            {
                results[formula] = double.NaN;
            }
        }

        return results;
    }

    /// <summary>
    /// Standard formulas that can be evaluated for comparison.
    /// </summary>
    public static readonly IReadOnlyList<string> StandardFormulas = new[]
    {
        "countWhere(ad >= 1.0)",
        "countWhere(ad >= 2.0)",
        "percentWhere(ad >= 1.0)",
        "avg(ad)",
        "max(ad)",
        "countWhere(winRate >= 0.5)",
        // Composite formula: weighted AD + win rate
        "countWhere(ad >= 1.0) * 0.7 + countWhere(winRate >= 0.5) * 0.3",
    };
}
