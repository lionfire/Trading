namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Configuration options for sensitivity analysis.
/// </summary>
public class SensitivityAnalysisOptions
{
    public const string ConfigurationLocation = "Trading:Optimization:Sensitivity";

    /// <summary>
    /// Minimum number of trades for a backtest to be included in analysis.
    /// Below this threshold, the backtest is discarded entirely.
    /// Default: 30 (Central Limit Theorem floor).
    /// </summary>
    public int MinTradeCount { get; set; } = 30;

    /// <summary>
    /// Trade count at which confidence reaches 1.0.
    /// Between MinTradeCount and this value, confidence ramps linearly.
    /// Default: 100.
    /// </summary>
    public int FullConfidenceTradeCount { get; set; } = 100;

    /// <summary>
    /// Primary metric to use for gradient computation.
    /// Default: AD (Annualized ROI / Drawdown — risk-adjusted).
    /// </summary>
    public SensitivityMetric Metric { get; set; } = SensitivityMetric.AD;

    /// <summary>
    /// Threshold for classifying a parameter as high sensitivity.
    /// Expressed as a fraction of the metric range: if mean gradient > range * threshold, it's high.
    /// Default: 0.1 (10% of metric range per parameter step).
    /// </summary>
    public double HighSensitivityThreshold { get; set; } = 0.1;

    /// <summary>
    /// Threshold for classifying a parameter as medium sensitivity.
    /// Default: 0.02 (2% of metric range per parameter step).
    /// </summary>
    public double MediumSensitivityThreshold { get; set; } = 0.02;

    /// <summary>
    /// Coefficient of variation threshold for flagging localized sensitivity.
    /// If gradient CV (stddev/mean) exceeds this, the parameter has localized sensitivity.
    /// Default: 1.5.
    /// </summary>
    public double LocalizedSensitivityCvThreshold { get; set; } = 1.5;

    /// <summary>
    /// Target number of meaningful sample points per parameter for the recommended step.
    /// Higher = finer resolution. Default: 20.
    /// </summary>
    public int TargetSamplePoints { get; set; } = 20;

    /// <summary>
    /// Minimum sample points per parameter (even for low-sensitivity parameters).
    /// Default: 5.
    /// </summary>
    public int MinSamplePoints { get; set; } = 5;

    /// <summary>
    /// Maximum sample points per parameter (cap to prevent explosion).
    /// Default: 100.
    /// </summary>
    public int MaxSamplePoints { get; set; } = 100;
}
