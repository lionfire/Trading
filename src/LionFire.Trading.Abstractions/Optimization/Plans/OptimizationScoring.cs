namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Scoring configuration for optimization results.
/// </summary>
public record OptimizationScoring
{
    /// <summary>
    /// Scoring formula reference or expression.
    /// </summary>
    public string Formula { get; init; } = "";

    /// <summary>
    /// Optional custom scoring parameters.
    /// </summary>
    public IDictionary<string, object>? Parameters { get; init; }
}
