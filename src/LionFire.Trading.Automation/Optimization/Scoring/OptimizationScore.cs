namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Represents the calculated score for an optimization run, including histogram distribution and summary statistics.
/// </summary>
/// <remarks>
/// The score is calculated by evaluating a formula against the set of backtest results.
/// The primary metric is AD (Annualized ROI / Drawdown), which measures risk-adjusted returns.
/// </remarks>
public record OptimizationScore
{
    /// <summary>
    /// The calculated score value based on the formula.
    /// </summary>
    /// <example>
    /// For formula "countWhere(ad >= 1.0)", this would be the count of backtests with AD >= 1.0
    /// </example>
    public double Value { get; init; }

    /// <summary>
    /// The formula used to calculate the score.
    /// </summary>
    /// <example>
    /// "countWhere(ad >= 1.0)"
    /// "countWhere(ad >= 1.0) * 0.7 + countWhere(winRate >= 0.5) * 0.3"
    /// </example>
    public string Formula { get; init; } = "countWhere(ad >= 1.0)";

    /// <summary>
    /// Histogram showing the distribution of AD values across all backtests.
    /// </summary>
    public AdHistogram? AdHistogram { get; init; }

    /// <summary>
    /// Summary statistics for the optimization run.
    /// </summary>
    public ScoreSummary? Summary { get; init; }

    /// <summary>
    /// When the score was calculated.
    /// </summary>
    public DateTimeOffset CalculatedAt { get; init; } = DateTimeOffset.UtcNow;
}
