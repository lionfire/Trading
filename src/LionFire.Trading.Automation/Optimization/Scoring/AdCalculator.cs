namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Calculates AD (Annualized ROI / Drawdown) metrics from backtest results.
/// </summary>
/// <remarks>
/// AD measures risk-adjusted returns:
/// - AD >= 1.0: Annualized gain exceeds max drawdown (acceptable)
/// - AD >= 2.0: Good risk-adjusted return
/// - AD >= 3.0: Strong candidate
/// - AD >= 5.0: Exceptional (verify not overfitting)
/// </remarks>
public static class AdCalculator
{
    /// <summary>
    /// Calculates AD from individual backtest metrics.
    /// </summary>
    /// <param name="netProfit">Net profit from the backtest</param>
    /// <param name="initialBalance">Starting balance</param>
    /// <param name="maxDrawdownPercent">Maximum drawdown as a percentage (0-100)</param>
    /// <param name="tradingPeriod">Duration of the backtest</param>
    /// <returns>AD value, or 0 if inputs are invalid</returns>
    public static double Calculate(double netProfit, double initialBalance, double maxDrawdownPercent, TimeSpan tradingPeriod)
    {
        if (maxDrawdownPercent <= 0) return 0; // Avoid division by zero
        if (tradingPeriod.TotalDays <= 0) return 0;
        if (initialBalance <= 0) return 0;

        var tradingDays = tradingPeriod.TotalDays;
        var roiPercent = (netProfit / initialBalance) * 100;
        var annualizedRoi = roiPercent * (365.0 / tradingDays);

        return annualizedRoi / maxDrawdownPercent;
    }

    /// <summary>
    /// Extracts AD values from a collection of backtest results.
    /// </summary>
    /// <param name="results">Collection of backtest results with AD property</param>
    /// <returns>List of AD values</returns>
    public static List<double> ExtractAdValues(IEnumerable<BacktestBatchJournalEntry> results)
    {
        return results
            .Where(r => !r.IsAborted)
            .Select(r => r.AD)
            .ToList();
    }

    /// <summary>
    /// Calculates summary statistics for a set of AD values.
    /// </summary>
    public static ScoreSummary CalculateSummary(IReadOnlyList<double> adValues, double threshold = 1.0)
    {
        if (adValues.Count == 0)
        {
            return new ScoreSummary
            {
                TotalBacktests = 0,
                Threshold = threshold
            };
        }

        var sorted = adValues.OrderBy(x => x).ToList();
        var passingCount = adValues.Count(ad => ad >= threshold);

        return new ScoreSummary
        {
            TotalBacktests = adValues.Count,
            Threshold = threshold,
            PassingCount = passingCount,
            PassingPercent = adValues.Count > 0 ? (passingCount * 100.0 / adValues.Count) : 0,
            MaxAd = adValues.Max(),
            MinAd = adValues.Min(),
            AvgAd = adValues.Average(),
            MedianAd = CalculateMedian(sorted),
            StdDevAd = CalculateStdDev(adValues),
            GoodCount = adValues.Count(ad => ad >= 2.0),
            StrongCount = adValues.Count(ad => ad >= 3.0),
            ExceptionalCount = adValues.Count(ad => ad >= 5.0)
        };
    }

    private static double CalculateMedian(IReadOnlyList<double> sorted)
    {
        if (sorted.Count == 0) return 0;
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2
            : sorted[mid];
    }

    private static double CalculateStdDev(IReadOnlyList<double> values)
    {
        if (values.Count <= 1) return 0;
        var avg = values.Average();
        var sumSquares = values.Sum(v => (v - avg) * (v - avg));
        return Math.Sqrt(sumSquares / (values.Count - 1));
    }
}
