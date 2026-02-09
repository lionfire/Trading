namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Prioritization settings for an optimization plan.
/// Controls how jobs are selected and follow-ups are queued.
/// </summary>
public record OptimizationPlanPrioritization
{
    /// <summary>
    /// Whether to use job prioritization based on promise scores.
    /// When enabled, jobs are executed in order of their promise score rather than sequentially.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Whether to auto-queue follow-up jobs at higher resolution for promising results.
    /// </summary>
    public bool AutoQueueFollowUps { get; init; } = true;

    /// <summary>
    /// Weights for promise score calculation.
    /// </summary>
    public PlanPrioritizationWeights Weights { get; init; } = new();

    /// <summary>
    /// Thresholds for follow-up job promotion.
    /// </summary>
    public PlanFollowUpThresholds FollowUpThresholds { get; init; } = new();
}

/// <summary>
/// Weights for promise score factors in plan execution.
/// </summary>
public record PlanPrioritizationWeights
{
    /// <summary>
    /// Weight for same-symbol performance factor (0.0 to 1.0).
    /// </summary>
    public double SymbolPerformance { get; init; } = 0.4;

    /// <summary>
    /// Weight for same-timeframe performance factor (0.0 to 1.0).
    /// </summary>
    public double TimeframePerformance { get; init; } = 0.3;

    /// <summary>
    /// Weight for market characteristics (symbol+timeframe combo) factor (0.0 to 1.0).
    /// </summary>
    public double MarketCharacteristics { get; init; } = 0.2;

    /// <summary>
    /// Weight for recency of related job results (0.0 to 1.0).
    /// </summary>
    public double Recency { get; init; } = 0.1;
}

/// <summary>
/// AD score thresholds for auto-queuing follow-up jobs.
/// </summary>
public record PlanFollowUpThresholds
{
    /// <summary>
    /// AD threshold for coarse (1000 backtests) to medium (10000 backtests) promotion.
    /// </summary>
    public double CoarseToMedium { get; init; } = 1.5;

    /// <summary>
    /// AD threshold for medium (10000 backtests) to full (50000 backtests) promotion.
    /// </summary>
    public double MediumToFull { get; init; } = 2.0;
}
