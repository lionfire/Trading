namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Summary statistics for an optimization run's scoring results.
/// </summary>
public record ScoreSummary
{
    /// <summary>
    /// Total number of backtests evaluated.
    /// </summary>
    public int TotalBacktests { get; init; }

    /// <summary>
    /// The AD threshold used for pass/fail determination.
    /// </summary>
    public double Threshold { get; init; } = 1.0;

    /// <summary>
    /// Number of backtests that meet or exceed the threshold.
    /// </summary>
    public int PassingCount { get; init; }

    /// <summary>
    /// Percentage of backtests that pass the threshold.
    /// </summary>
    public double PassingPercent { get; init; }

    /// <summary>
    /// Maximum AD value across all backtests.
    /// </summary>
    public double MaxAd { get; init; }

    /// <summary>
    /// Average AD value across all backtests.
    /// </summary>
    public double AvgAd { get; init; }

    /// <summary>
    /// Median AD value across all backtests.
    /// </summary>
    public double MedianAd { get; init; }

    /// <summary>
    /// Minimum AD value across all backtests.
    /// </summary>
    public double MinAd { get; init; }

    /// <summary>
    /// Standard deviation of AD values.
    /// </summary>
    public double StdDevAd { get; init; }

    /// <summary>
    /// Number of backtests with AD >= 2.0 (good candidates).
    /// </summary>
    public int GoodCount { get; init; }

    /// <summary>
    /// Number of backtests with AD >= 3.0 (strong candidates).
    /// </summary>
    public int StrongCount { get; init; }

    /// <summary>
    /// Number of backtests with AD >= 5.0 (exceptional candidates).
    /// </summary>
    public int ExceptionalCount { get; init; }
}
