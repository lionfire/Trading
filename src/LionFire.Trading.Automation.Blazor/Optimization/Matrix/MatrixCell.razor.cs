using LionFire.Trading.Automation.Optimization.Matrix;
using LionFire.Trading.Optimization.Matrix;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LionFire.Trading.Automation.Blazor.Optimization.Matrix;

public partial class MatrixCell : ComponentBase
{
    [Parameter]
    public string PlanId { get; set; } = "";

    [Parameter]
    public string Symbol { get; set; } = "";

    [Parameter]
    public string Timeframe { get; set; } = "";

    /// <summary>
    /// The cell-level priority state (without row/column aggregation).
    /// </summary>
    [Parameter]
    public MatrixCellPriority? CellPriority { get; set; }

    /// <summary>
    /// The effective priority considering cell, row, and column states.
    /// </summary>
    [Parameter]
    public int EffectivePriority { get; set; } = 5;

    /// <summary>
    /// Whether this cell is enabled, considering cell, row, and column enablement.
    /// </summary>
    [Parameter]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Aggregated optimization results for this cell (grade, backtest counts, AD scores).
    /// Null if no results are available.
    /// </summary>
    [Parameter]
    public MatrixCellResult? Result { get; set; }

    /// <summary>
    /// The visual execution state of this cell (NeverRun, Disabled, Queued, Running, Complete, Failed).
    /// </summary>
    [Parameter]
    public CellVisualState VisualState { get; set; } = CellVisualState.NeverRun;

    /// <summary>
    /// Execution progress data for this cell (job counts by status).
    /// When provided, Running/Queued states show progress text overlay.
    /// </summary>
    [Parameter]
    public MatrixCellProgress? Progress { get; set; }

    /// <summary>
    /// Fired when the user clicks the cell or the drill-down icon to view details.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellEventArgs> OnCellDrillDown { get; set; }

    /// <summary>
    /// Fired when the user requests a re-run of optimization for this cell.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellEventArgs> OnReRun { get; set; }

    /// <summary>
    /// Fired when the user clicks the play button to run optimization for this cell.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellEventArgs> OnRun { get; set; }

    /// <summary>
    /// Fired when the user requests to view history for this cell.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellEventArgs> OnViewHistory { get; set; }

    /// <summary>
    /// Fired when the user requests to open this cell in the one-shot optimizer page.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellEventArgs> OnOpenInOneShot { get; set; }

    /// <summary>
    /// Whether this cell is currently selected (part of a multi-selection).
    /// </summary>
    [Parameter]
    public bool IsSelected { get; set; }

    /// <summary>
    /// Whether this cell currently has keyboard focus.
    /// </summary>
    [Parameter]
    public bool IsFocused { get; set; }

    /// <summary>
    /// Whether this cell was recently updated by the auto-optimizer.
    /// When true, a brief highlight animation is shown and then cleared.
    /// </summary>
    [Parameter]
    public bool IsAutoUpdated { get; set; }

    /// <summary>
    /// When true, cells with results show the results view (score, AD, backtests).
    /// When false, cells always show the parameters view (priority).
    /// </summary>
    [Parameter]
    public bool ShowResults { get; set; } = true;

    /// <summary>
    /// Fired when the user Ctrl+clicks or otherwise toggles selection on this cell.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellSelectEventArgs> OnSelect { get; set; }

    [Inject]
    private IPlanMatrixService MatrixService { get; set; } = default!;

    private bool _contextMenuOpen;
    private bool _moreMenuOpen;

    private string StateCssClass => CellColorEngine.GetCellCssClass(VisualState, Result?.Grade, IsEnabled);

    private string CellInlineStyle => CellColorEngine.GetCellInlineStyle(VisualState, Result?.Grade, IsEnabled);

    private bool HasResults => Result is not null && Result.Grade != OptimizationGrade.Error;

    private int TotalAll => Result != null ? Result.TotalBacktests + Result.AbortedBacktests : 0;

    private string AbortPercentText
    {
        get
        {
            if (Result is not { AbortedBacktests: > 0 }) return "";
            return TotalAll > 0 ? $"{(int)((double)Result!.AbortedBacktests / TotalAll * 100)}%" : "";
        }
    }

    private string DistributionBarStyle
    {
        get
        {
            if (Result == null || TotalAll == 0) return "background: #444;";

            var passingPct = (double)Result.PassingCount / TotalAll * 100;
            var failingPct = (double)(Result.TotalBacktests - Result.PassingCount) / TotalAll * 100;
            var p1 = passingPct;
            var p2 = passingPct + failingPct;

            // Blue (#1976D2) = passing (B grade+), Red (#C62828) = failing, Black (#1a1a1a) = aborted
            return $"background: linear-gradient(to right, #1976D2 0%, #1976D2 {p1:F1}%, #C62828 {p1:F1}%, #C62828 {p2:F1}%, #1a1a1a {p2:F1}%, #1a1a1a 100%);";
        }
    }

    private string DistributionBarTooltip
    {
        get
        {
            if (Result == null || TotalAll == 0) return "No data";
            var failing = Result.TotalBacktests - Result.PassingCount;
            return $"{Result.PassingCount} passing (AD >= 1.0) | {failing} failing | {Result.AbortedBacktests} aborted — {TotalAll} total";
        }
    }

    private string PriorityChipStyle => EffectivePriority switch
    {
        <= 3 => "color: var(--mud-palette-success); border-color: var(--mud-palette-success);",
        <= 6 => "",
        _ => "color: var(--mud-palette-text-secondary); border-color: var(--mud-palette-text-secondary); opacity: 0.6;"
    };

    private string PriorityColorClass => EffectivePriority switch
    {
        <= 3 => "matrix-cell-priority-high",
        <= 6 => "matrix-cell-priority-medium",
        _ => "matrix-cell-priority-low"
    };

    private string TooltipText
    {
        get
        {
            var parts = new List<string> { $"{Symbol} / {Timeframe}" };

            if (!IsEnabled)
            {
                parts.Add("Disabled");
            }
            else
            {
                parts.Add($"Priority: {EffectivePriority}");
                if (CellPriority is not null)
                {
                    if (CellPriority.ManualPriority.HasValue)
                        parts.Add($"Manual: {CellPriority.ManualPriority}");
                    if (CellPriority.AutoPriority.HasValue)
                        parts.Add($"Auto: {CellPriority.AutoPriority}");
                    parts.Add(CellPriority.IsAutopilot ? "Mode: Autopilot" : "Mode: Manual");
                }
            }

            if (Progress is not null && Progress.TotalJobs > 0)
            {
                parts.Add($"Jobs: {Progress.CompletedJobs}/{Progress.TotalJobs}");
                if (Progress.RunningJobs > 0)
                    parts.Add($"Running: {Progress.RunningJobs}");
                if (Progress.FailedJobs > 0)
                    parts.Add($"Failed: {Progress.FailedJobs}");
                if (Progress.PendingJobs > 0)
                    parts.Add($"Pending: {Progress.PendingJobs}");
            }

            if (Result is not null)
            {
                parts.Add($"Grade: {OptimizationGradeComputer.GradeToString(Result.Grade)} - {OptimizationGradeComputer.GradeDescription(Result.Grade)}");
                if (Result.Grade != OptimizationGrade.Error)
                {
                    parts.Add($"Best AD: {Result.BestAd:F2}");
                    parts.Add($"Avg AD: {Result.AverageAd:F2}");
                    parts.Add($"Passing: {Result.PassingCount}/{Result.TotalBacktests}");
                }
                if (Result.ErrorJobCount > 0)
                    parts.Add($"Errored jobs: {Result.ErrorJobCount}");
                if (!string.IsNullOrEmpty(Result.ErrorMessage))
                    parts.Add($"Error: {Result.ErrorMessage}");
                if (Result.LastRunAt.HasValue)
                    parts.Add($"Last run: {Result.LastRunAt.Value:g}");
            }

            return string.Join(" | ", parts);
        }
    }

    /// <summary>
    /// The cell-level priority (not aggregated). Used for +/- adjustments.
    /// </summary>
    private int CellLevelPriority => CellPriority?.EffectivePriority ?? 5;

    private string GradeText => Result is not null
        ? OptimizationGradeComputer.GradeToString(Result.Grade)
        : "";

    private string GradeColor => Result is not null
        ? OptimizationGradeComputer.GradeToColor(Result.Grade)
        : "";

    private string GradeDescription => Result is not null
        ? OptimizationGradeComputer.GradeDescription(Result.Grade)
        : "";

    private MatrixCellEventArgs CreateEventArgs() => new(Symbol, Timeframe);

    // --- Priority controls ---

    private async Task IncreasePriority()
    {
        var newPriority = Math.Max(1, CellLevelPriority - 1);
        await MatrixService.SetCellPriorityAsync(PlanId, Symbol, Timeframe, newPriority);
    }

    private async Task DecreasePriority()
    {
        var newPriority = Math.Min(9, CellLevelPriority + 1);
        await MatrixService.SetCellPriorityAsync(PlanId, Symbol, Timeframe, newPriority);
    }

    private async Task ToggleEnabled()
    {
        await MatrixService.SetCellEnabledAsync(PlanId, Symbol, Timeframe, !IsEnabled);
    }

    // --- Cell click and drill-down ---

    private async Task OnCellClick(MouseEventArgs e)
    {
        // Ctrl+click toggles selection
        if (e.CtrlKey && OnSelect.HasDelegate)
        {
            await OnSelect.InvokeAsync(new MatrixCellSelectEventArgs(Symbol, Timeframe, e.ShiftKey));
            return;
        }

        // Plain left click fires drill-down
        if (OnCellDrillDown.HasDelegate)
        {
            await OnCellDrillDown.InvokeAsync(CreateEventArgs());
        }
    }

    private async Task DrillDown()
    {
        if (OnCellDrillDown.HasDelegate)
        {
            await OnCellDrillDown.InvokeAsync(CreateEventArgs());
        }
    }

    // --- Context menu ---

    private void OnContextMenu(MouseEventArgs e)
    {
        _contextMenuOpen = true;
    }

    private void CloseContextMenu()
    {
        _contextMenuOpen = false;
    }

    private async Task ViewResults()
    {
        _contextMenuOpen = false;
        if (OnCellDrillDown.HasDelegate)
        {
            await OnCellDrillDown.InvokeAsync(CreateEventArgs());
        }
    }

    private async Task ReRunOptimization()
    {
        _contextMenuOpen = false;
        if (OnReRun.HasDelegate)
        {
            await OnReRun.InvokeAsync(CreateEventArgs());
        }
    }

    private async Task RunCell()
    {
        if (OnRun.HasDelegate)
        {
            await OnRun.InvokeAsync(CreateEventArgs());
        }
    }

    private async Task ResetPriority()
    {
        _contextMenuOpen = false;
        await MatrixService.ResetCellToAutopilotAsync(PlanId, Symbol, Timeframe);
    }

    private async Task ViewHistory()
    {
        _contextMenuOpen = false;
        if (OnViewHistory.HasDelegate)
        {
            await OnViewHistory.InvokeAsync(CreateEventArgs());
        }
    }

    // --- More menu ---

    private void ToggleMoreMenu()
    {
        _moreMenuOpen = !_moreMenuOpen;
    }

    private void CloseMoreMenu()
    {
        _moreMenuOpen = false;
    }

    private async Task OpenInOneShot()
    {
        _moreMenuOpen = false;
        if (OnOpenInOneShot.HasDelegate)
        {
            await OnOpenInOneShot.InvokeAsync(CreateEventArgs());
        }
    }

    // --- Reset to autopilot (for M indicator click) ---

    private async Task ResetToAutopilot()
    {
        await MatrixService.ResetCellToAutopilotAsync(PlanId, Symbol, Timeframe);
    }
}

/// <summary>
/// Event args for cell-level interactions (drill-down, re-run, history).
/// </summary>
public record MatrixCellEventArgs(string Symbol, string Timeframe, string? DateRangeName = null);

/// <summary>
/// Event args for cell selection (Ctrl+click or Shift+Ctrl+click).
/// </summary>
public record MatrixCellSelectEventArgs(string Symbol, string Timeframe, bool IsShiftHeld);
