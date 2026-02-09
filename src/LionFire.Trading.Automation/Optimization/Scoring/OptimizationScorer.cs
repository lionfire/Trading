namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Calculates scores for optimization runs based on backtest results.
/// </summary>
public class OptimizationScorer
{
    private readonly IReadOnlyList<BacktestBatchJournalEntry> _results;
    private readonly List<double> _adValues;
    private readonly ScoringFormulaParser _formulaParser;

    /// <summary>
    /// Creates a new scorer for the given backtest results.
    /// </summary>
    public OptimizationScorer(IEnumerable<BacktestBatchJournalEntry> results)
    {
        _results = results.ToList();
        _adValues = AdCalculator.ExtractAdValues(_results);
        _formulaParser = new ScoringFormulaParser(_results);
    }

    /// <summary>
    /// Calculates the optimization score using the specified formula.
    /// </summary>
    /// <param name="formula">Scoring formula (e.g., "countWhere(ad >= 1.0)")</param>
    /// <param name="threshold">Default threshold for summary statistics</param>
    /// <returns>Calculated optimization score with histogram and summary</returns>
    /// <remarks>
    /// Supported formula features:
    /// - Metrics: ad, winRate, tradeCount
    /// - Functions: countWhere(metric op value), percentWhere(metric op value), avg(metric), max(metric), min(metric), sum(metric), pow(x, y), log(x), sqrt(x)
    /// - Operators: +, -, *, /
    /// - Parentheses for grouping
    ///
    /// Examples:
    /// - countWhere(ad >= 1.0)
    /// - countWhere(ad >= 1.0) * 0.7 + countWhere(winRate >= 0.5) * 0.3
    /// - pow(countWhere(ad >= 2.0), 1.5) * log(avg(tradeCount) + 1)
    /// </remarks>
    public OptimizationScore Calculate(string formula = "countWhere(ad >= 1.0)", double threshold = 1.0)
    {
        double scoreValue;
        try
        {
            scoreValue = _formulaParser.Evaluate(formula);
        }
        catch (FormulaParseException)
        {
            // For backwards compatibility, fall back to simple evaluation for basic formulas
            scoreValue = EvaluateSimpleFormula(formula, threshold);
        }

        var histogram = HistogramGenerator.GenerateAdHistogram(_adValues);
        var summary = AdCalculator.CalculateSummary(_adValues, threshold);

        return new OptimizationScore
        {
            Value = scoreValue,
            Formula = formula,
            AdHistogram = histogram,
            Summary = summary,
            CalculatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Evaluates a formula directly using the formula parser.
    /// </summary>
    /// <param name="formula">The formula to evaluate</param>
    /// <returns>The calculated score value</returns>
    /// <exception cref="FormulaParseException">Thrown when the formula cannot be parsed or evaluated</exception>
    public double EvaluateFormula(string formula)
    {
        return _formulaParser.Evaluate(formula);
    }

    /// <summary>
    /// Evaluates a simple formula without full parsing (legacy support).
    /// Supports: countWhere(ad >= X), percentWhere(ad >= X), avg(ad), max(ad), min(ad)
    /// </summary>
    private double EvaluateSimpleFormula(string formula, double defaultThreshold)
    {
        formula = formula.Trim().ToLowerInvariant();

        // countWhere(ad >= X)
        if (formula.StartsWith("countwhere(ad >="))
        {
            var threshold = ParseThreshold(formula, "countwhere(ad >=", defaultThreshold);
            return _adValues.Count(ad => ad >= threshold);
        }

        // countWhere(ad > X)
        if (formula.StartsWith("countwhere(ad >"))
        {
            var threshold = ParseThreshold(formula, "countwhere(ad >", defaultThreshold);
            return _adValues.Count(ad => ad > threshold);
        }

        // percentWhere(ad >= X)
        if (formula.StartsWith("percentwhere(ad >="))
        {
            var threshold = ParseThreshold(formula, "percentwhere(ad >=", defaultThreshold);
            var count = _adValues.Count(ad => ad >= threshold);
            return _adValues.Count > 0 ? (count * 100.0 / _adValues.Count) : 0;
        }

        // avg(ad)
        if (formula == "avg(ad)" || formula == "average(ad)")
        {
            return _adValues.Count > 0 ? _adValues.Average() : 0;
        }

        // max(ad)
        if (formula == "max(ad)")
        {
            return _adValues.Count > 0 ? _adValues.Max() : 0;
        }

        // min(ad)
        if (formula == "min(ad)")
        {
            return _adValues.Count > 0 ? _adValues.Min() : 0;
        }

        // Default: countWhere(ad >= threshold)
        return _adValues.Count(ad => ad >= defaultThreshold);
    }

    private static double ParseThreshold(string formula, string prefix, double defaultValue)
    {
        try
        {
            var rest = formula.Substring(prefix.Length).Trim();
            var closeIndex = rest.IndexOf(')');
            if (closeIndex > 0)
            {
                var valueStr = rest.Substring(0, closeIndex).Trim();
                if (double.TryParse(valueStr, out var value))
                    return value;
            }
        }
        catch
        {
            // Fall through to default
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the count of backtests passing a threshold.
    /// </summary>
    public int CountWhere(double threshold) => _adValues.Count(ad => ad >= threshold);

    /// <summary>
    /// Gets the percentage of backtests passing a threshold.
    /// </summary>
    public double PercentWhere(double threshold) =>
        _adValues.Count > 0 ? (CountWhere(threshold) * 100.0 / _adValues.Count) : 0;

    /// <summary>
    /// Gets the AD values for external use.
    /// </summary>
    public IReadOnlyList<double> AdValues => _adValues;

    /// <summary>
    /// Gets the total number of results.
    /// </summary>
    public int TotalCount => _adValues.Count;
}
