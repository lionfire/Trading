using LionFire.Trading.Optimization.Matrix;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Determines composite visual styling for matrix cells based on execution state,
/// grade, and enabled/disabled status.
/// </summary>
public static class CellColorEngine
{
    /// <summary>
    /// Returns CSS class name(s) for the cell based on composite state.
    /// Disabled state overrides all others, then Running > Failed > Complete > Queued > NeverRun.
    /// </summary>
    public static string GetCellCssClass(CellVisualState state, OptimizationGrade? grade, bool isEnabled)
    {
        if (!isEnabled)
            return "matrix-cell-state-disabled";

        return state switch
        {
            CellVisualState.Running => "matrix-cell-state-running",
            CellVisualState.Failed => "matrix-cell-state-failed",
            CellVisualState.Complete => "matrix-cell-state-complete",
            CellVisualState.Queued => "matrix-cell-state-queued",
            CellVisualState.NeverRun => "matrix-cell-state-never-run",
            CellVisualState.Disabled => "matrix-cell-state-disabled",
            _ => "matrix-cell-state-never-run"
        };
    }

    /// <summary>
    /// Returns inline background-color style for grade-based coloring.
    /// For Complete cells, the background uses the grade color at reduced opacity
    /// so that text remains readable.
    /// </summary>
    public static string GetCellBackgroundStyle(CellVisualState state, OptimizationGrade? grade)
    {
        if (state == CellVisualState.Complete && grade.HasValue)
        {
            var hex = OptimizationGradeComputer.GradeToColor(grade.Value);
            // Use grade color at 25% opacity for the cell background
            var (r, g, b) = HexToRgb(hex);
            return $"background-color: rgba({r},{g},{b},0.25);";
        }

        // Other states use CSS classes for background; no inline style needed
        return "";
    }

    /// <summary>
    /// Returns a CSS color value for text that is readable against the cell background.
    /// </summary>
    public static string GetTextColor(CellVisualState state, OptimizationGrade? grade)
    {
        return state switch
        {
            CellVisualState.Running => "#FFFFFF",
            CellVisualState.Disabled => "#9E9E9E",
            // For Complete, Queued, NeverRun, Failed: use default text color (handled by CSS variable)
            _ => ""
        };
    }

    /// <summary>
    /// Returns inline style combining background and text color for a cell.
    /// </summary>
    public static string GetCellInlineStyle(CellVisualState state, OptimizationGrade? grade, bool isEnabled)
    {
        if (!isEnabled)
            return "";

        var bgStyle = GetCellBackgroundStyle(state, grade);
        var textColor = GetTextColor(state, grade);

        if (!string.IsNullOrEmpty(textColor))
        {
            bgStyle += $" color: {textColor};";
        }

        return bgStyle.Trim();
    }

    private static (int R, int G, int B) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6)
            return (128, 128, 128);

        return (
            Convert.ToInt32(hex[..2], 16),
            Convert.ToInt32(hex[2..4], 16),
            Convert.ToInt32(hex[4..6], 16)
        );
    }
}
