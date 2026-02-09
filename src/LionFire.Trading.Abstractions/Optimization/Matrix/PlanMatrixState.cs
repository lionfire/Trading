namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Complete priority and enablement state for an optimization plan's symbol/timeframe matrix.
/// </summary>
public record PlanMatrixState
{
    /// <summary>
    /// ID of the plan this state belongs to.
    /// </summary>
    public string PlanId { get; init; } = "";

    /// <summary>
    /// Row states keyed by symbol.
    /// </summary>
    public Dictionary<string, MatrixAxisState> RowStates { get; init; } = new();

    /// <summary>
    /// Column states keyed by timeframe.
    /// </summary>
    public Dictionary<string, MatrixAxisState> ColumnStates { get; init; } = new();

    /// <summary>
    /// Cell states keyed by "symbol|timeframe".
    /// </summary>
    public Dictionary<string, MatrixCellPriority> CellStates { get; init; } = new();

    /// <summary>
    /// Creates a dictionary key for a cell from its symbol and timeframe.
    /// </summary>
    public static string CellKey(string symbol, string timeframe) => $"{symbol}|{timeframe}";

    /// <summary>
    /// Computes the effective priority for a cell, considering row, column, and cell-level state.
    /// Returns int.MaxValue if any level is disabled.
    /// </summary>
    public int GetEffectivePriority(string symbol, string timeframe)
    {
        var cellKey = CellKey(symbol, timeframe);
        var cellPriority = CellStates.TryGetValue(cellKey, out var cell) ? cell.EffectivePriority : 5;
        var rowPriority = RowStates.TryGetValue(symbol, out var row) ? row.EffectivePriority : 5;
        var colPriority = ColumnStates.TryGetValue(timeframe, out var col) ? col.EffectivePriority : 5;

        if (cell is { IsEnabled: false } || row is { IsEnabled: false } || col is { IsEnabled: false })
            return int.MaxValue;

        return Math.Min(cellPriority, Math.Min(rowPriority, colPriority));
    }

    /// <summary>
    /// Checks whether a cell is enabled, considering row, column, and cell-level enablement.
    /// </summary>
    public bool IsCellEnabled(string symbol, string timeframe)
    {
        var cellKey = CellKey(symbol, timeframe);
        var cellEnabled = !CellStates.TryGetValue(cellKey, out var cell) || cell.IsEnabled;
        var rowEnabled = !RowStates.TryGetValue(symbol, out var row) || row.IsEnabled;
        var colEnabled = !ColumnStates.TryGetValue(timeframe, out var col) || col.IsEnabled;
        return cellEnabled && rowEnabled && colEnabled;
    }
}
