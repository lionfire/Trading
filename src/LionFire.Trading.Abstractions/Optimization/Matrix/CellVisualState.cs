namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// The visual execution state of a matrix cell, ordered by display priority.
/// </summary>
public enum CellVisualState
{
    /// <summary>Cell has never been executed.</summary>
    NeverRun,
    /// <summary>Cell is disabled by user.</summary>
    Disabled,
    /// <summary>Cell is queued for execution.</summary>
    Queued,
    /// <summary>Cell is currently being executed.</summary>
    Running,
    /// <summary>Cell has completed execution with results.</summary>
    Complete,
    /// <summary>Cell execution failed.</summary>
    Failed
}
