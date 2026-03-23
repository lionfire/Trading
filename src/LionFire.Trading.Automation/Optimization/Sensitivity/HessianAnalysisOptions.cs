namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Configuration options for Hessian (cross-parameter interaction) analysis.
/// </summary>
public class HessianAnalysisOptions
{
    public const string ConfigurationLocation = "Trading:Optimization:Sensitivity:Hessian";

    /// <summary>
    /// Maximum number of parameter pairs to analyze (N choose 2 can grow fast).
    /// Pairs are selected by descending product of individual sensitivities.
    /// Default: 10 pairs.
    /// </summary>
    public int MaxPairs { get; set; } = 10;

    /// <summary>
    /// Only analyze parameters with sensitivity level at or above this threshold.
    /// Default: Medium (analyze Medium and High sensitivity parameters).
    /// </summary>
    public SensitivityLevel MinSensitivityLevel { get; set; } = SensitivityLevel.Medium;

    /// <summary>
    /// Minimum number of valid 2x2 grid cells needed for a reliable interaction estimate.
    /// Pairs with fewer cells are still reported but with reduced confidence.
    /// Default: 5.
    /// </summary>
    public int MinSampleCount { get; set; } = 5;

    /// <summary>
    /// Threshold for Strong interaction classification.
    /// Default: 0.5.
    /// </summary>
    public double StrongInteractionThreshold { get; set; } = 0.5;

    /// <summary>
    /// Threshold for Weak interaction classification (below this = Independent).
    /// Default: 0.1.
    /// </summary>
    public double WeakInteractionThreshold { get; set; } = 0.1;
}
