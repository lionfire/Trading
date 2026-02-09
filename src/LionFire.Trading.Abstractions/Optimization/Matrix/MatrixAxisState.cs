namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Priority state for a matrix row (symbol) or column (timeframe).
/// </summary>
public record MatrixAxisState
{
    /// <summary>
    /// System-computed priority based on scoring and analysis.
    /// </summary>
    public int? AutoPriority { get; init; }

    /// <summary>
    /// User-specified priority override. Null means autopilot mode.
    /// </summary>
    public int? ManualPriority { get; init; }

    /// <summary>
    /// Whether this axis entry is enabled for optimization.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// True when no manual priority override is set.
    /// </summary>
    public bool IsAutopilot => ManualPriority == null;

    /// <summary>
    /// The priority used for scheduling.
    /// </summary>
    public int EffectivePriority => ManualPriority ?? AutoPriority ?? 5;
}
