namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Priority state for a single cell in the optimization matrix (symbol + timeframe intersection).
/// </summary>
public record MatrixCellPriority
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
    /// Whether this cell is enabled for optimization.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// True when no manual priority override is set.
    /// </summary>
    public bool IsAutopilot => ManualPriority == null;

    /// <summary>
    /// The priority used for scheduling. Disabled cells return int.MaxValue.
    /// </summary>
    public int EffectivePriority => IsEnabled ? (ManualPriority ?? AutoPriority ?? 5) : int.MaxValue;
}
