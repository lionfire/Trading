namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Service for managing the priority and enablement state of an optimization plan matrix.
/// </summary>
public interface IPlanMatrixService
{
    /// <summary>
    /// Get the current matrix state for a plan, creating a default if none exists.
    /// </summary>
    Task<PlanMatrixState> GetStateAsync(string planId);

    // Cell-level operations

    /// <summary>
    /// Set a manual priority override for a specific cell.
    /// </summary>
    Task SetCellPriorityAsync(string planId, string symbol, string timeframe, int priority);

    /// <summary>
    /// Enable or disable a specific cell.
    /// </summary>
    Task SetCellEnabledAsync(string planId, string symbol, string timeframe, bool enabled);

    /// <summary>
    /// Set the auto-computed priority for a specific cell.
    /// </summary>
    Task SetCellAutoPriorityAsync(string planId, string symbol, string timeframe, int priority);

    /// <summary>
    /// Remove the manual priority override, returning the cell to autopilot mode.
    /// </summary>
    Task ResetCellToAutopilotAsync(string planId, string symbol, string timeframe);

    // Row-level operations

    /// <summary>
    /// Set a manual priority override for a row (symbol).
    /// </summary>
    Task SetRowPriorityAsync(string planId, string symbol, int priority);

    /// <summary>
    /// Enable or disable a row (symbol).
    /// </summary>
    Task SetRowEnabledAsync(string planId, string symbol, bool enabled);

    /// <summary>
    /// Bulk-disable all cells in a row.
    /// </summary>
    Task DisableRowAsync(string planId, string symbol);

    // Column-level operations

    /// <summary>
    /// Set a manual priority override for a column (timeframe).
    /// </summary>
    Task SetColumnPriorityAsync(string planId, string timeframe, int priority);

    /// <summary>
    /// Enable or disable a column (timeframe).
    /// </summary>
    Task SetColumnEnabledAsync(string planId, string timeframe, bool enabled);

    /// <summary>
    /// Bulk-disable all cells in a column.
    /// </summary>
    Task DisableColumnAsync(string planId, string timeframe);

    /// <summary>
    /// Fired when the matrix state changes.
    /// </summary>
    event EventHandler<MatrixStateChangedEventArgs>? StateChanged;
}

/// <summary>
/// Event args for matrix state changes.
/// </summary>
public record MatrixStateChangedEventArgs(
    string PlanId,
    MatrixStateChangeType ChangeType,
    string? Symbol = null,
    string? Timeframe = null);

/// <summary>
/// Type of matrix state change.
/// </summary>
public enum MatrixStateChangeType
{
    CellPriorityChanged,
    CellEnabledChanged,
    RowPriorityChanged,
    RowEnabledChanged,
    ColumnPriorityChanged,
    ColumnEnabledChanged,
    BulkChange,
    AutoPriorityChanged
}
