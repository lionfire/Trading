using LionFire.Trading.Optimization.Plans;

namespace LionFire.Trading.Automation.Optimization.Prioritization;

/// <summary>
/// Represents a calculated promise score for a pending optimization job.
/// </summary>
public record PromiseScore
{
    /// <summary>
    /// The promise score, ranging from 0.0 (unlikely to succeed) to 1.0 (very likely to succeed).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Confidence in the score, ranging from 0.0 (no data) to 1.0 (abundant data).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// The individual factors that contributed to this score.
    /// </summary>
    public IReadOnlyList<PromiseFactor> Factors { get; init; } = [];

    /// <summary>
    /// Human-readable explanation of the score.
    /// </summary>
    public string Reasoning { get; init; } = "";

    /// <summary>
    /// Creates a neutral score with low confidence (for when no related data exists).
    /// </summary>
    public static PromiseScore Neutral => new()
    {
        Score = 0.5,
        Confidence = 0.1,
        Factors = [],
        Reasoning = "No related jobs available for comparison"
    };
}

/// <summary>
/// Represents a single factor contributing to a promise score.
/// </summary>
public record PromiseFactor
{
    /// <summary>
    /// Name of the factor (e.g., "Symbol Performance", "Timeframe Performance").
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Weight of this factor in the overall calculation (e.g., 0.4 for 40%).
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Raw value of this factor before weighting (0.0 to 1.0 normalized).
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Contribution to the final score (Weight * Value).
    /// </summary>
    public double Contribution { get; init; }

    /// <summary>
    /// Description of how this factor was calculated.
    /// </summary>
    public string Description { get; init; } = "";
}

/// <summary>
/// Configuration for promise score calculation weights.
/// </summary>
public record PrioritizationWeights
{
    /// <summary>
    /// Weight for same-symbol performance factor.
    /// </summary>
    public double SymbolPerformance { get; init; } = 0.4;

    /// <summary>
    /// Weight for same-timeframe performance factor.
    /// </summary>
    public double TimeframePerformance { get; init; } = 0.3;

    /// <summary>
    /// Weight for market characteristics (volatility, trend strength).
    /// </summary>
    public double MarketCharacteristics { get; init; } = 0.2;

    /// <summary>
    /// Weight for recency of related job results.
    /// </summary>
    public double Recency { get; init; } = 0.1;

    /// <summary>
    /// Default weights.
    /// </summary>
    public static PrioritizationWeights Default => new();

    /// <summary>
    /// Create from plan-level prioritization weights.
    /// </summary>
    public static PrioritizationWeights FromPlan(PlanPrioritizationWeights? planWeights)
    {
        if (planWeights == null) return Default;

        return new PrioritizationWeights
        {
            SymbolPerformance = planWeights.SymbolPerformance,
            TimeframePerformance = planWeights.TimeframePerformance,
            MarketCharacteristics = planWeights.MarketCharacteristics,
            Recency = planWeights.Recency
        };
    }
}

/// <summary>
/// Configuration for the prioritization system.
/// </summary>
public record PrioritizationConfig
{
    /// <summary>
    /// Weights for promise score calculation.
    /// </summary>
    public PrioritizationWeights Weights { get; init; } = new();

    /// <summary>
    /// Configuration for follow-up job queuing.
    /// </summary>
    public FollowUpConfig FollowUp { get; init; } = new();

    /// <summary>
    /// Whether prioritization is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Create from plan-level prioritization settings.
    /// </summary>
    public static PrioritizationConfig FromPlan(OptimizationPlanPrioritization? planPrioritization)
    {
        if (planPrioritization == null) return new PrioritizationConfig();

        return new PrioritizationConfig
        {
            Enabled = planPrioritization.Enabled,
            Weights = PrioritizationWeights.FromPlan(planPrioritization.Weights),
            FollowUp = FollowUpConfig.FromPlan(planPrioritization.FollowUpThresholds)
        };
    }
}

/// <summary>
/// Configuration for automatic follow-up job queuing.
/// </summary>
public record FollowUpConfig
{
    /// <summary>
    /// AD threshold for coarse (1000 backtests) to medium (10000 backtests) promotion.
    /// </summary>
    public double CoarseToMediumAdThreshold { get; init; } = 1.5;

    /// <summary>
    /// AD threshold for medium (10000 backtests) to full (50000 backtests) promotion.
    /// </summary>
    public double MediumToFullAdThreshold { get; init; } = 2.0;

    /// <summary>
    /// Maximum backtests for coarse resolution.
    /// </summary>
    public int CoarseMaxBacktests { get; init; } = 1000;

    /// <summary>
    /// Maximum backtests for medium resolution.
    /// </summary>
    public int MediumMaxBacktests { get; init; } = 10000;

    /// <summary>
    /// Maximum backtests for full resolution.
    /// </summary>
    public int FullMaxBacktests { get; init; } = 50000;

    /// <summary>
    /// Create from plan-level follow-up thresholds.
    /// </summary>
    public static FollowUpConfig FromPlan(PlanFollowUpThresholds? planThresholds)
    {
        if (planThresholds == null) return new FollowUpConfig();

        return new FollowUpConfig
        {
            CoarseToMediumAdThreshold = planThresholds.CoarseToMedium,
            MediumToFullAdThreshold = planThresholds.MediumToFull
        };
    }
}
