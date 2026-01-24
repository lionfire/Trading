namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Resolution settings controlling optimization granularity.
/// </summary>
public record OptimizationResolution
{
    /// <summary>
    /// Maximum number of backtests to run. Lower for coarse scans.
    /// </summary>
    public int MaxBacktests { get; init; } = 1000;

    /// <summary>
    /// Minimum parameter priority to include (0 = all, higher = fewer params).
    /// </summary>
    public int MinParameterPriority { get; init; } = 0;
}
