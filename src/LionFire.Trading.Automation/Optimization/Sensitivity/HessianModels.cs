namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Interaction analysis result for a pair of parameters, computed from the mixed partial derivative
/// of the objective metric with respect to both parameters.
/// </summary>
public record ParameterInteraction
{
    public required string ParameterA { get; init; }
    public required string ParameterB { get; init; }

    /// <summary>
    /// Mean absolute magnitude of the mixed partial derivative (d²f/dxdy).
    /// </summary>
    public double InteractionMagnitude { get; init; }

    /// <summary>
    /// Interaction score: magnitude of mixed partial relative to product of individual partial magnitudes.
    /// Values greater than 1 indicate strong interaction. Range: [0, infinity).
    /// </summary>
    public double InteractionScore { get; init; }

    /// <summary>
    /// Classification based on InteractionScore thresholds.
    /// </summary>
    public InteractionStrength Strength { get; init; }

    /// <summary>
    /// Whether the interaction is synergistic (both parameters amplify each other's effect)
    /// or antagonistic (one dampens the other's effect).
    /// </summary>
    public InteractionDirection Direction { get; init; }

    /// <summary>
    /// Number of valid 2x2 grid cells used for computation.
    /// </summary>
    public int SampleCount { get; init; }

    /// <summary>
    /// Confidence in this interaction measurement (0-1), derived from sample count
    /// and confidence weights of the underlying backtest results.
    /// </summary>
    public double Confidence { get; init; }
}

public enum InteractionStrength
{
    /// <summary>Score below weak threshold — parameters are effectively independent.</summary>
    Independent,

    /// <summary>Score between weak and strong thresholds — mild coupling.</summary>
    Weak,

    /// <summary>Score above strong threshold — significant cross-parameter coupling.</summary>
    Strong,
}

public enum InteractionDirection
{
    /// <summary>Positive mixed partial — both parameters amplify each other's effect.</summary>
    Synergistic,

    /// <summary>Negative mixed partial — one parameter dampens the other's effect.</summary>
    Antagonistic,

    /// <summary>Sign of mixed partial varies across the grid — direction is inconsistent.</summary>
    Mixed,
}

/// <summary>
/// Complete Hessian analysis result for an optimization run, containing pairwise
/// interaction measurements for all analyzed parameter pairs.
/// </summary>
public record HessianAnalysisResult
{
    public string? PlanId { get; init; }
    public string? BotType { get; init; }
    public string? Symbol { get; init; }
    public DateTimeOffset AnalyzedAt { get; init; }

    /// <summary>Parameters that were selected for pairwise analysis.</summary>
    public required List<string> AnalyzedParameters { get; init; }

    /// <summary>All computed parameter pair interactions.</summary>
    public required List<ParameterInteraction> Interactions { get; init; }

    /// <summary>Only interactions classified as Weak or Strong.</summary>
    public IEnumerable<ParameterInteraction> SignificantInteractions
        => Interactions.Where(i => i.Strength != InteractionStrength.Independent);
}
