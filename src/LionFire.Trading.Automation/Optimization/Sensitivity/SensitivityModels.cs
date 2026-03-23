namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Metric to use for sensitivity analysis.
/// </summary>
public enum SensitivityMetric
{
    /// <summary>
    /// Annualized ROI / Drawdown (risk-adjusted, default).
    /// Already available in backtest CSV journal.
    /// </summary>
    AD,

    /// <summary>
    /// Raw net profit (secondary, less stable due to outlier sensitivity).
    /// Not currently in CSV journal — reserved for future use.
    /// </summary>
    NetPnL,

    /// <summary>
    /// Fitness score (computed during optimization).
    /// </summary>
    Fitness,
}

/// <summary>
/// Sensitivity analysis result for a single parameter.
/// </summary>
public record ParameterSensitivity
{
    /// <summary>
    /// Parameter path (e.g., "P.StopLossATRMultiplier").
    /// </summary>
    public required string ParameterName { get; init; }

    /// <summary>
    /// Mean absolute gradient magnitude across all grid points for this parameter.
    /// Higher = more sensitive = needs finer step.
    /// </summary>
    public double MeanGradientMagnitude { get; init; }

    /// <summary>
    /// Maximum absolute gradient magnitude observed.
    /// </summary>
    public double MaxGradientMagnitude { get; init; }

    /// <summary>
    /// Variance of gradient magnitudes. High variance indicates localized sensitivity.
    /// </summary>
    public double GradientVariance { get; init; }

    /// <summary>
    /// Classification derived from gradient statistics.
    /// </summary>
    public SensitivityLevel Level { get; init; }

    /// <summary>
    /// Number of valid gradient samples computed for this parameter.
    /// </summary>
    public int SampleCount { get; init; }

    /// <summary>
    /// Effective sample size after confidence weighting.
    /// </summary>
    public double EffectiveSampleSize { get; init; }

    /// <summary>
    /// Whether gradient variance is high relative to mean, indicating localized sensitivity
    /// that may benefit from non-uniform sampling.
    /// </summary>
    public bool HasLocalizedSensitivity { get; init; }
}

public enum SensitivityLevel
{
    /// <summary>Low sensitivity — parameter has little effect on results. Can use coarse step.</summary>
    Low,
    /// <summary>Medium sensitivity — moderate effect on results.</summary>
    Medium,
    /// <summary>High sensitivity — parameter strongly affects results. Needs fine step.</summary>
    High,
}

/// <summary>
/// Step recommendation for a single parameter, derived from sensitivity analysis.
/// </summary>
public record StepRecommendation
{
    /// <summary>
    /// Parameter path (e.g., "P.StopLossATRMultiplier").
    /// </summary>
    public required string ParameterName { get; init; }

    /// <summary>
    /// Recommended step size for optimization grid.
    /// </summary>
    public double RecommendedStep { get; init; }

    /// <summary>
    /// Recommended step exponent (1.0 = linear, >1 = denser at low end, <1 = denser at high end).
    /// </summary>
    public double RecommendedStepExponent { get; init; } = 1.0;

    /// <summary>
    /// Confidence in this recommendation (0.0 to 1.0).
    /// Based on sample size, consistency of gradients, and trade count reliability.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// The sensitivity analysis that produced this recommendation.
    /// </summary>
    public required ParameterSensitivity Sensitivity { get; init; }

    /// <summary>
    /// Current step value (if known), for comparison.
    /// </summary>
    public double? CurrentStep { get; init; }

    /// <summary>
    /// Current step exponent (if known), for comparison.
    /// </summary>
    public double? CurrentStepExponent { get; init; }
}

/// <summary>
/// Complete set of step recommendations for a plan/optimization run.
/// </summary>
public record StepRecommendationSet
{
    /// <summary>
    /// Plan ID this analysis was performed on.
    /// </summary>
    public string? PlanId { get; init; }

    /// <summary>
    /// Bot type name.
    /// </summary>
    public string? BotType { get; init; }

    /// <summary>
    /// Symbol analyzed.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// TimeFrame analyzed.
    /// </summary>
    public string? TimeFrame { get; init; }

    /// <summary>
    /// Metric used for sensitivity analysis.
    /// </summary>
    public SensitivityMetric Metric { get; init; }

    /// <summary>
    /// When the analysis was performed.
    /// </summary>
    public DateTimeOffset AnalyzedAt { get; init; }

    /// <summary>
    /// Total backtest entries analyzed (before filtering).
    /// </summary>
    public int TotalEntries { get; init; }

    /// <summary>
    /// Entries that passed the minimum trade count filter.
    /// </summary>
    public int QualifyingEntries { get; init; }

    /// <summary>
    /// Minimum trade count threshold used.
    /// </summary>
    public int MinTradeCount { get; init; }

    /// <summary>
    /// Full confidence trade count threshold used.
    /// </summary>
    public int FullConfidenceTradeCount { get; init; }

    /// <summary>
    /// Per-parameter recommendations.
    /// </summary>
    public required List<StepRecommendation> Recommendations { get; init; }
}
