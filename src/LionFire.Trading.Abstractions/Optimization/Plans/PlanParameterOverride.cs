namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Per-parameter override for optimization plans.
/// Only non-null values override the bot's attribute-defined defaults.
/// </summary>
public record PlanParameterOverride
{
    /// <summary>
    /// Parameter path matching IParameterOptimizationOptions.Path (e.g., "Period", "AtrMultiplier").
    /// </summary>
    public string ParameterPath { get; init; } = "";

    /// <summary>
    /// Override minimum value for optimization range.
    /// </summary>
    public double? MinValue { get; init; }

    /// <summary>
    /// Override maximum value for optimization range.
    /// </summary>
    public double? MaxValue { get; init; }

    /// <summary>
    /// Override step size for grid search.
    /// </summary>
    public double? Step { get; init; }

    /// <summary>
    /// Override exponent for non-linear distribution.
    /// </summary>
    public double? Exponent { get; init; }

    /// <summary>
    /// Explicitly enable or disable this parameter for optimization.
    /// null = use priority-based default.
    /// </summary>
    public bool? EnableOptimization { get; init; }
}
