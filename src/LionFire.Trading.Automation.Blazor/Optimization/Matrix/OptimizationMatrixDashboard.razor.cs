using LionFire.Trading.Automation.Optimization.Matrix;
using LionFire.Trading.Optimization.Matrix;
using LionFire.Trading.Optimization.Plans;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace LionFire.Trading.Automation.Blazor.Optimization.Matrix;

public partial class OptimizationMatrixDashboard : ComponentBase, IDisposable
{
    /// <summary>
    /// The optimization plan to display. If provided, takes precedence over PlanId.
    /// </summary>
    [Parameter]
    public OptimizationPlan? Plan { get; set; }

    /// <summary>
    /// Plan ID to load the matrix state for. Used when Plan is not directly provided.
    /// </summary>
    [Parameter]
    public string? PlanId { get; set; }

    /// <summary>
    /// Fired when the user clicks the play button on a cell to run optimization.
    /// Includes the symbol, timeframe, and selected date range name.
    /// </summary>
    [Parameter]
    public EventCallback<(string Symbol, string Timeframe, string? DateRangeName)> OnRunCell { get; set; }

    /// <summary>
    /// Fired when the user requests to open a cell in the one-shot optimizer.
    /// </summary>
    [Parameter]
    public EventCallback<(string Symbol, string Timeframe)> OnOpenCellInOneShot { get; set; }

    /// <summary>
    /// Fired when a user drills down into a cell (click or Enter).
    /// If this callback has a delegate, the parent handles navigation.
    /// Otherwise, the dashboard uses its default navigation behavior.
    /// </summary>
    [Parameter]
    public EventCallback<MatrixCellEventArgs> OnCellDrillDown { get; set; }

    /// <summary>
    /// Fired when the plan is modified (e.g., timeframe added/removed) so the parent can update its state.
    /// </summary>
    [Parameter]
    public EventCallback<OptimizationPlan> OnPlanChanged { get; set; }

    [Inject]
    private IPlanMatrixService MatrixService { get; set; } = default!;

    [Inject]
    private IMatrixResultsProvider ResultsProvider { get; set; } = default!;

    [Inject]
    private ILogger<OptimizationMatrixDashboard> Logger { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IOptimizationPlanRepository PlanRepository { get; set; } = default!;

    private PlanMatrixState? _state;
    private Dictionary<string, MatrixCellResult> _cellResults = new();
    private Dictionary<string, MatrixCellProgress> _cellProgress = new();
    private List<string> _symbols = new();
    private List<string> _timeframes = new();
    private bool _isLoading = true;
    private string? _error;
    private bool _disposed;

    /// <summary>
    /// Timer for auto-refreshing progress when cells are active.
    /// </summary>
    private System.Timers.Timer? _progressTimer;

    /// <summary>
    /// Interval in milliseconds for polling progress updates.
    /// </summary>
    private const int ProgressPollIntervalMs = 5000;

    private string EffectivePlanId => Plan?.Id ?? PlanId ?? "";

    // --- Cell selection state ---

    private readonly HashSet<string> _selectedCells = new();

    // --- Keyboard focus state ---

    private int _focusedRow = -1;
    private int _focusedCol = -1;

    // --- Filtering state ---

    private string _gradeFilter = "";
    private string _statusFilter = "";
    private bool _showResults = true;

    // --- Auto-priority highlight tracking ---

    /// <summary>
    /// Set of cell keys that were recently updated by auto-priority changes.
    /// Cells in this set show a brief highlight animation, then get removed after timeout.
    /// </summary>
    private readonly HashSet<string> _autoUpdatedCells = new();
    private CancellationTokenSource? _autoUpdateHighlightCts;

    // --- Date range selection ---

    private int _selectedDateRangeIndex = -1;

    /// <summary>
    /// Standard timeframes available for adding to the matrix.
    /// </summary>
    private static readonly string[] AllStandardTimeframes =
        ["m1", "m5", "m15", "m30", "h1", "h2", "h4", "h6", "h8", "h12", "d1", "w1", "mn1"];

    // --- Aggregate progress properties ---

    private int AggregateCompletedCells => _cellProgress.Values.Count(p => p.VisualState == CellVisualState.Complete);
    private int AggregateRunningCells => _cellProgress.Values.Count(p => p.VisualState == CellVisualState.Running);
    private int AggregateQueuedCells => _cellProgress.Values.Count(p => p.VisualState == CellVisualState.Queued);
    private int AggregateFailedCells => _cellProgress.Values.Count(p => p.VisualState == CellVisualState.Failed);
    private int AggregateTotalCellsWithJobs => _cellProgress.Count;
    private bool HasActiveProgress => _cellProgress.Values.Any(p => p.IsActive);

    private double AggregateProgressPercent
    {
        get
        {
            var totalJobs = _cellProgress.Values.Sum(p => p.TotalJobs);
            var completedJobs = _cellProgress.Values.Sum(p => p.CompletedJobs);
            return totalJobs > 0 ? (double)completedJobs / totalJobs * 100 : 0;
        }
    }

    private int AggregateTotalJobs => _cellProgress.Values.Sum(p => p.TotalJobs);
    private int AggregateCompletedJobs => _cellProgress.Values.Sum(p => p.CompletedJobs);

    // --- Effective lists ---

    private List<string> EffectiveTimeframes => _timeframes;

    // --- Filtered lists ---

    private List<string> FilteredSymbols
    {
        get
        {
            if (string.IsNullOrEmpty(_gradeFilter) && string.IsNullOrEmpty(_statusFilter))
                return _symbols;

            return _symbols.Where(s => EffectiveTimeframes.Any(tf => CellPassesFilter(s, tf))).ToList();
        }
    }

    private List<string> FilteredTimeframes
    {
        get
        {
            var effective = EffectiveTimeframes;
            if (string.IsNullOrEmpty(_gradeFilter) && string.IsNullOrEmpty(_statusFilter))
                return effective;

            return effective.Where(tf => _symbols.Any(s => CellPassesFilter(s, tf))).ToList();
        }
    }

    private static void SortTimeframes(List<string> timeframes)
    {
        timeframes.Sort((a, b) => Array.IndexOf(AllStandardTimeframes, a).CompareTo(Array.IndexOf(AllStandardTimeframes, b)));
    }

    /// <summary>
    /// Available timeframes that are not yet in the matrix (for the Add Timeframe dropdown).
    /// </summary>
    private IEnumerable<string> AvailableNewTimeframes =>
        AllStandardTimeframes.Where(tf => !_timeframes.Contains(tf));

    /// <summary>
    /// Date ranges from the plan.
    /// </summary>
    private IReadOnlyList<OptimizationDateRange> EffectiveDateRanges => Plan?.DateRanges ?? [];

    /// <summary>
    /// Currently selected date range, or null if none selected.
    /// </summary>
    private OptimizationDateRange? SelectedDateRange =>
        _selectedDateRangeIndex >= 0 && _selectedDateRangeIndex < EffectiveDateRanges.Count
            ? EffectiveDateRanges[_selectedDateRangeIndex]
            : null;

    protected override async Task OnInitializedAsync()
    {
        MatrixService.StateChanged += OnMatrixStateChanged;
        await LoadAsync();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Reload if the plan or plan ID changed
        var newSymbols = Plan?.Symbols.EffectiveSymbols.ToList() ?? new();
        var newTimeframes = Plan?.Timeframes.ToList() ?? new();

        if (!_symbols.SequenceEqual(newSymbols) || !_timeframes.SequenceEqual(newTimeframes))
        {
            _symbols = newSymbols;
            _timeframes = newTimeframes;
            SortTimeframes(_timeframes);
            await LoadStateAsync();
            await LoadResultsAsync();
            await LoadProgressAsync();
        }

        await base.OnParametersSetAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _isLoading = true;
            _error = null;

            if (Plan is not null)
            {
                _symbols = Plan.Symbols.EffectiveSymbols.ToList();
                _timeframes = Plan.Timeframes.ToList();
                SortTimeframes(_timeframes);
            }

            await LoadStateAsync();
            await LoadResultsAsync();
            await LoadProgressAsync();
            ManageProgressTimer();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load optimization matrix for plan '{PlanId}'", EffectivePlanId);
            _error = $"Failed to load matrix: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadStateAsync()
    {
        if (string.IsNullOrEmpty(EffectivePlanId))
        {
            _state = new PlanMatrixState();
            return;
        }

        try
        {
            _state = await MatrixService.GetStateAsync(EffectivePlanId);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load matrix state for plan '{PlanId}', using defaults", EffectivePlanId);
            _state = new PlanMatrixState { PlanId = EffectivePlanId };
        }
    }

    private async Task LoadResultsAsync()
    {
        if (string.IsNullOrEmpty(EffectivePlanId))
        {
            _cellResults = new();
            return;
        }

        try
        {
            _cellResults = await ResultsProvider.GetAllResultsAsync(EffectivePlanId);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load matrix results for plan '{PlanId}'", EffectivePlanId);
            _cellResults = new();
        }
    }

    private async Task LoadProgressAsync()
    {
        if (string.IsNullOrEmpty(EffectivePlanId))
        {
            _cellProgress = new();
            return;
        }

        try
        {
            _cellProgress = await ResultsProvider.GetProgressAsync(EffectivePlanId);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load progress for plan '{PlanId}'", EffectivePlanId);
            _cellProgress = new();
        }
    }

    private MatrixCellResult? GetCellResult(string symbol, string timeframe)
    {
        var key = PlanMatrixState.CellKey(symbol, timeframe);
        return _cellResults.GetValueOrDefault(key);
    }

    private MatrixCellProgress? GetCellProgress(string symbol, string timeframe)
    {
        var key = PlanMatrixState.CellKey(symbol, timeframe);
        return _cellProgress.GetValueOrDefault(key);
    }

    private MatrixAxisState GetRowState(string symbol)
    {
        if (_state?.RowStates.TryGetValue(symbol, out var state) == true)
            return state;
        return new MatrixAxisState();
    }

    private MatrixAxisState GetColumnState(string timeframe)
    {
        if (_state?.ColumnStates.TryGetValue(timeframe, out var state) == true)
            return state;
        return new MatrixAxisState();
    }

    private MatrixCellPriority GetCellPriority(string cellKey)
    {
        if (_state?.CellStates.TryGetValue(cellKey, out var priority) == true)
            return priority;
        return new MatrixCellPriority();
    }

    /// <summary>
    /// Determines the visual state for a cell based on progress data, enabled status, and results.
    /// Progress data takes precedence for Running/Queued/Failed states.
    /// </summary>
    private CellVisualState GetCellVisualState(string symbol, string timeframe, bool isEnabled)
    {
        if (!isEnabled)
            return CellVisualState.Disabled;

        // Use progress data for execution states when available
        var progress = GetCellProgress(symbol, timeframe);
        if (progress is not null && progress.TotalJobs > 0)
        {
            return progress.VisualState;
        }

        // Fall back to result-based detection
        var result = GetCellResult(symbol, timeframe);
        if (result is not null)
        {
            return CellVisualState.Complete;
        }

        return CellVisualState.NeverRun;
    }

    private int CountEnabledCells()
    {
        int count = 0;
        foreach (var symbol in _symbols)
        {
            foreach (var tf in EffectiveTimeframes)
            {
                if (_state?.IsCellEnabled(symbol, tf) != false)
                    count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Counts complete cells for a specific row (symbol).
    /// </summary>
    private string GetRowProgressText(string symbol)
    {
        int complete = 0;
        int total = 0;
        foreach (var tf in EffectiveTimeframes)
        {
            if (_state?.IsCellEnabled(symbol, tf) == false) continue;
            total++;
            var key = PlanMatrixState.CellKey(symbol, tf);
            var progress = _cellProgress.GetValueOrDefault(key);
            if (progress is { VisualState: CellVisualState.Complete })
                complete++;
            else if (progress is null && _cellResults.ContainsKey(key))
                complete++;
        }
        return total > 0 ? $"{complete}/{total}" : "";
    }

    /// <summary>
    /// Counts complete cells for a specific column (timeframe).
    /// </summary>
    private string GetColumnProgressText(string timeframe)
    {
        int complete = 0;
        int total = 0;
        foreach (var symbol in _symbols)
        {
            if (_state?.IsCellEnabled(symbol, timeframe) == false) continue;
            total++;
            var key = PlanMatrixState.CellKey(symbol, timeframe);
            var progress = _cellProgress.GetValueOrDefault(key);
            if (progress is { VisualState: CellVisualState.Complete })
                complete++;
            else if (progress is null && _cellResults.ContainsKey(key))
                complete++;
        }
        return total > 0 ? $"{complete}/{total}" : "";
    }

    private static string FormatSymbol(string symbol)
    {
        // Trim common suffixes for compact display if the symbol is long
        if (symbol.Length > 10)
        {
            // Try to show a shortened version, keeping the base pair
            return symbol[..10] + "...";
        }
        return symbol;
    }

    // --- Filtering ---

    /// <summary>
    /// Tests whether a cell passes the current grade and status filters.
    /// </summary>
    private bool CellPassesFilter(string symbol, string timeframe)
    {
        var isEnabled = _state?.IsCellEnabled(symbol, timeframe) ?? true;
        var visualState = GetCellVisualState(symbol, timeframe, isEnabled);

        // Status filter
        if (!string.IsNullOrEmpty(_statusFilter))
        {
            var passes = _statusFilter switch
            {
                "running" => visualState == CellVisualState.Running,
                "complete" => visualState == CellVisualState.Complete,
                "failed" => visualState == CellVisualState.Failed,
                "never-run" => visualState == CellVisualState.NeverRun,
                _ => true
            };
            if (!passes) return false;
        }

        // Grade filter
        if (!string.IsNullOrEmpty(_gradeFilter))
        {
            var result = GetCellResult(symbol, timeframe);
            if (result is null) return false;

            var passes = _gradeFilter switch
            {
                "A" => result.Grade is OptimizationGrade.APlus or OptimizationGrade.A or OptimizationGrade.AMinus,
                "B" => result.Grade is OptimizationGrade.BPlus or OptimizationGrade.B or OptimizationGrade.BMinus,
                "C" => result.Grade is OptimizationGrade.CPlus or OptimizationGrade.C or OptimizationGrade.CMinus,
                "DF" => result.Grade is OptimizationGrade.D or OptimizationGrade.F or OptimizationGrade.Error,
                _ => true
            };
            if (!passes) return false;
        }

        return true;
    }

    private void OnGradeFilterChanged(string value)
    {
        _gradeFilter = value;
    }

    private void OnStatusFilterChanged(string value)
    {
        _statusFilter = value;
    }

    private void OnDateRangeIndexChanged(int value)
    {
        _selectedDateRangeIndex = value;
    }

    private void ToggleShowResults()
    {
        _showResults = !_showResults;
    }

    // --- Cell selection ---

    private bool IsCellSelected(string symbol, string timeframe) =>
        _selectedCells.Contains(PlanMatrixState.CellKey(symbol, timeframe));

    private bool IsCellFocused(string symbol, string timeframe)
    {
        var filteredSymbols = FilteredSymbols;
        var filteredTimeframes = FilteredTimeframes;
        if (_focusedRow < 0 || _focusedCol < 0) return false;
        if (_focusedRow >= filteredSymbols.Count || _focusedCol >= filteredTimeframes.Count) return false;
        return filteredSymbols[_focusedRow] == symbol && filteredTimeframes[_focusedCol] == timeframe;
    }

    private bool IsCellAutoUpdated(string symbol, string timeframe) =>
        _autoUpdatedCells.Contains(PlanMatrixState.CellKey(symbol, timeframe));

    private void HandleCellSelect(MatrixCellSelectEventArgs args)
    {
        var key = PlanMatrixState.CellKey(args.Symbol, args.Timeframe);
        if (_selectedCells.Contains(key))
        {
            _selectedCells.Remove(key);
        }
        else
        {
            _selectedCells.Add(key);
        }

        // Update focus to the clicked cell
        var filteredSymbols = FilteredSymbols;
        var filteredTimeframes = FilteredTimeframes;
        _focusedRow = filteredSymbols.IndexOf(args.Symbol);
        _focusedCol = filteredTimeframes.IndexOf(args.Timeframe);
    }

    private void SelectRow(string symbol)
    {
        foreach (var tf in FilteredTimeframes)
        {
            _selectedCells.Add(PlanMatrixState.CellKey(symbol, tf));
        }
    }

    private void SelectColumn(string timeframe)
    {
        foreach (var s in FilteredSymbols)
        {
            _selectedCells.Add(PlanMatrixState.CellKey(s, timeframe));
        }
    }

    private void ClearSelection()
    {
        _selectedCells.Clear();
    }

    // --- Batch operations ---

    private async Task SetPriorityForSelected(int priority)
    {
        foreach (var key in _selectedCells)
        {
            var parts = key.Split('|', 2);
            if (parts.Length == 2)
            {
                await MatrixService.SetCellPriorityAsync(EffectivePlanId, parts[0], parts[1], priority);
            }
        }
        Snackbar.Add($"Set priority {priority} for {_selectedCells.Count} cells", Severity.Success);
    }

    private async Task EnableSelected()
    {
        foreach (var key in _selectedCells)
        {
            var parts = key.Split('|', 2);
            if (parts.Length == 2)
            {
                await MatrixService.SetCellEnabledAsync(EffectivePlanId, parts[0], parts[1], true);
            }
        }
        Snackbar.Add($"Enabled {_selectedCells.Count} cells", Severity.Success);
    }

    private async Task DisableSelected()
    {
        foreach (var key in _selectedCells)
        {
            var parts = key.Split('|', 2);
            if (parts.Length == 2)
            {
                await MatrixService.SetCellEnabledAsync(EffectivePlanId, parts[0], parts[1], false);
            }
        }
        Snackbar.Add($"Disabled {_selectedCells.Count} cells", Severity.Success);
    }

    private async Task ResetSelectedToAutopilot()
    {
        foreach (var key in _selectedCells)
        {
            var parts = key.Split('|', 2);
            if (parts.Length == 2)
            {
                await MatrixService.ResetCellToAutopilotAsync(EffectivePlanId, parts[0], parts[1]);
            }
        }
        Snackbar.Add($"Reset {_selectedCells.Count} cells to autopilot", Severity.Success);
    }

    // --- Add / Remove Timeframe ---

    private async Task AddTimeframe(string tf)
    {
        if (Plan == null || _timeframes.Contains(tf)) return;
        var newTimeframes = Plan.Timeframes.Append(tf).ToList();
        newTimeframes.Sort((a, b) => Array.IndexOf(AllStandardTimeframes, a).CompareTo(Array.IndexOf(AllStandardTimeframes, b)));
        var updatedPlan = Plan with { Timeframes = newTimeframes };
        var saved = await PlanRepository.SaveAsync(updatedPlan);
        await OnPlanChanged.InvokeAsync(saved);
        Snackbar.Add($"Added timeframe {tf}", Severity.Success);
    }

    private async Task RemoveTimeframe(string tf)
    {
        if (Plan == null || !_timeframes.Contains(tf)) return;
        var updatedPlan = Plan with { Timeframes = Plan.Timeframes.Where(t => t != tf).ToList() };
        var saved = await PlanRepository.SaveAsync(updatedPlan);
        foreach (var s in _symbols)
        {
            _selectedCells.Remove(PlanMatrixState.CellKey(s, tf));
        }
        await OnPlanChanged.InvokeAsync(saved);
        Snackbar.Add($"Removed timeframe {tf}", Severity.Info);
    }

    // --- Keyboard navigation ---

    private void OnKeyDown(KeyboardEventArgs e)
    {
        var filteredSymbols = FilteredSymbols;
        var filteredTimeframes = FilteredTimeframes;
        if (filteredSymbols.Count == 0 || filteredTimeframes.Count == 0) return;

        switch (e.Key)
        {
            case "ArrowUp":
                _focusedRow = Math.Max(0, _focusedRow - 1);
                if (_focusedCol < 0) _focusedCol = 0;
                break;
            case "ArrowDown":
                _focusedRow = Math.Min(filteredSymbols.Count - 1, _focusedRow + 1);
                if (_focusedCol < 0) _focusedCol = 0;
                break;
            case "ArrowLeft":
                _focusedCol = Math.Max(0, _focusedCol - 1);
                if (_focusedRow < 0) _focusedRow = 0;
                break;
            case "ArrowRight":
                _focusedCol = Math.Min(filteredTimeframes.Count - 1, _focusedCol + 1);
                if (_focusedRow < 0) _focusedRow = 0;
                break;
            case "Enter":
                if (_focusedRow >= 0 && _focusedCol >= 0
                    && _focusedRow < filteredSymbols.Count && _focusedCol < filteredTimeframes.Count)
                {
                    HandleCellDrillDown(new MatrixCellEventArgs(
                        filteredSymbols[_focusedRow], filteredTimeframes[_focusedCol]));
                }
                break;
            case " ":
                ToggleFocusedCellEnabled();
                break;
            case "+" or "=":
                AdjustFocusedCellPriority(-1);
                break;
            case "-":
                AdjustFocusedCellPriority(+1);
                break;
            case "Escape":
                ClearSelection();
                _focusedRow = -1;
                _focusedCol = -1;
                break;
        }
    }

    private async void ToggleFocusedCellEnabled()
    {
        var filteredSymbols = FilteredSymbols;
        var filteredTimeframes = FilteredTimeframes;
        if (_focusedRow < 0 || _focusedCol < 0
            || _focusedRow >= filteredSymbols.Count || _focusedCol >= filteredTimeframes.Count) return;

        var symbol = filteredSymbols[_focusedRow];
        var tf = filteredTimeframes[_focusedCol];
        var isEnabled = _state?.IsCellEnabled(symbol, tf) ?? true;
        await MatrixService.SetCellEnabledAsync(EffectivePlanId, symbol, tf, !isEnabled);
    }

    private async void AdjustFocusedCellPriority(int delta)
    {
        var filteredSymbols = FilteredSymbols;
        var filteredTimeframes = FilteredTimeframes;
        if (_focusedRow < 0 || _focusedCol < 0
            || _focusedRow >= filteredSymbols.Count || _focusedCol >= filteredTimeframes.Count) return;

        var symbol = filteredSymbols[_focusedRow];
        var tf = filteredTimeframes[_focusedCol];
        var cellKey = PlanMatrixState.CellKey(symbol, tf);
        var currentPriority = GetCellPriority(cellKey).EffectivePriority;
        var newPriority = Math.Clamp(currentPriority + delta, 1, 9);
        await MatrixService.SetCellPriorityAsync(EffectivePlanId, symbol, tf, newPriority);
    }

    // --- Auto-refresh progress polling ---

    private void ManageProgressTimer()
    {
        if (HasActiveProgress)
        {
            StartProgressTimer();
        }
        else
        {
            StopProgressTimer();
        }
    }

    private void StartProgressTimer()
    {
        if (_progressTimer is not null) return;

        _progressTimer = new System.Timers.Timer(ProgressPollIntervalMs);
        _progressTimer.Elapsed += OnProgressTimerElapsed;
        _progressTimer.AutoReset = true;
        _progressTimer.Start();
    }

    private void StopProgressTimer()
    {
        if (_progressTimer is null) return;

        _progressTimer.Stop();
        _progressTimer.Elapsed -= OnProgressTimerElapsed;
        _progressTimer.Dispose();
        _progressTimer = null;
    }

    private async void OnProgressTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_disposed) return;

        try
        {
            await InvokeAsync(async () =>
            {
                await LoadProgressAsync();
                await LoadResultsAsync();
                ManageProgressTimer();
                StateHasChanged();
            });
        }
        catch (ObjectDisposedException)
        {
            // Component was disposed during async callback
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error during progress poll for plan '{PlanId}'", EffectivePlanId);
        }
    }

    // Row header priority controls

    private async Task IncreaseRowPriority(string symbol)
    {
        var state = GetRowState(symbol);
        var newPriority = Math.Max(1, state.EffectivePriority - 1);
        await MatrixService.SetRowPriorityAsync(EffectivePlanId, symbol, newPriority);
    }

    private async Task DecreaseRowPriority(string symbol)
    {
        var state = GetRowState(symbol);
        var newPriority = Math.Min(9, state.EffectivePriority + 1);
        await MatrixService.SetRowPriorityAsync(EffectivePlanId, symbol, newPriority);
    }

    private async Task ToggleRowEnabled(string symbol)
    {
        var state = GetRowState(symbol);
        await MatrixService.SetRowEnabledAsync(EffectivePlanId, symbol, !state.IsEnabled);
    }

    // Column header priority controls

    private async Task IncreaseColumnPriority(string timeframe)
    {
        var state = GetColumnState(timeframe);
        var newPriority = Math.Max(1, state.EffectivePriority - 1);
        await MatrixService.SetColumnPriorityAsync(EffectivePlanId, timeframe, newPriority);
    }

    private async Task DecreaseColumnPriority(string timeframe)
    {
        var state = GetColumnState(timeframe);
        var newPriority = Math.Min(9, state.EffectivePriority + 1);
        await MatrixService.SetColumnPriorityAsync(EffectivePlanId, timeframe, newPriority);
    }

    private async Task ToggleColumnEnabled(string timeframe)
    {
        var state = GetColumnState(timeframe);
        await MatrixService.SetColumnEnabledAsync(EffectivePlanId, timeframe, !state.IsEnabled);
    }

    // --- Cell interaction handlers ---

    private async void HandleCellDrillDown(MatrixCellEventArgs args)
    {
        // Enrich with date range info
        var enrichedArgs = args with { DateRangeName = SelectedDateRange?.Name };

        // If parent handles drill-down, delegate to it
        if (OnCellDrillDown.HasDelegate)
        {
            await OnCellDrillDown.InvokeAsync(enrichedArgs);
            return;
        }

        // Default behavior
        var result = GetCellResult(args.Symbol, args.Timeframe);
        if (result is not null)
        {
            // Navigate to results view for this cell
            NavigationManager.NavigateTo(
                $"/optimization/plans/{EffectivePlanId}/results?symbol={Uri.EscapeDataString(args.Symbol)}&timeframe={Uri.EscapeDataString(args.Timeframe)}");
        }
        else
        {
            // No results yet - check progress state for more detail
            var progress = GetCellProgress(args.Symbol, args.Timeframe);
            if (progress is { IsActive: true })
            {
                Snackbar.Add($"{args.Symbol} {args.Timeframe} is currently running ({progress.CompletedJobs}/{progress.TotalJobs} jobs)", Severity.Info);
            }
            else if (progress is { FailedJobs: > 0 })
            {
                Snackbar.Add(
                    $"{args.Symbol} {args.Timeframe}: {progress.FailedJobs} job(s) failed. Check Silo logs for details.",
                    Severity.Error);
            }
            else
            {
                Snackbar.Add($"No results for {args.Symbol} {args.Timeframe} yet. Start execution from the Overview tab.", Severity.Info);
            }
        }
    }

    private async Task HandleCellRun(MatrixCellEventArgs args)
    {
        if (OnRunCell.HasDelegate)
        {
            var dateRangeName = SelectedDateRange?.Name;
            await OnRunCell.InvokeAsync((args.Symbol, args.Timeframe, dateRangeName));
        }
        else
        {
            Snackbar.Add($"Run requested for {args.Symbol} {args.Timeframe} - connect OnRunCell to enable", Severity.Warning);
        }
    }

    private void HandleCellReRun(MatrixCellEventArgs args)
    {
        Snackbar.Add($"Re-run requested for {args.Symbol} {args.Timeframe} (not yet implemented)", Severity.Warning);
        // TODO: Wire to optimization job submission
    }

    private void HandleCellViewHistory(MatrixCellEventArgs args)
    {
        Snackbar.Add($"View history for {args.Symbol} {args.Timeframe} (not yet implemented)", Severity.Warning);
        // TODO: Navigate to or open history view
    }

    private async Task HandleCellOpenInOneShot(MatrixCellEventArgs args)
    {
        if (OnOpenCellInOneShot.HasDelegate)
        {
            await OnOpenCellInOneShot.InvokeAsync((args.Symbol, args.Timeframe));
        }
        else
        {
            // Default behavior: navigate to one-shot page
            NavigationManager.NavigateTo(
                $"/optimize/one-shot?symbol={Uri.EscapeDataString(args.Symbol)}&timeframe={Uri.EscapeDataString(args.Timeframe)}");
        }
    }

    private async void OnMatrixStateChanged(object? sender, MatrixStateChangedEventArgs args)
    {
        if (args.PlanId != EffectivePlanId) return;

        try
        {
            await InvokeAsync(async () =>
            {
                // Track auto-priority changes for highlight animation
                if (args.ChangeType == MatrixStateChangeType.AutoPriorityChanged
                    && args.Symbol is not null && args.Timeframe is not null)
                {
                    var cellKey = PlanMatrixState.CellKey(args.Symbol, args.Timeframe);
                    _autoUpdatedCells.Add(cellKey);
                    ScheduleAutoUpdateHighlightClear();

                    Snackbar.Add($"Auto-optimizer updated priority for {args.Symbol} / {args.Timeframe}",
                        Severity.Info, options => options.VisibleStateDuration = 3000);
                }

                await LoadStateAsync();
                await LoadResultsAsync();
                await LoadProgressAsync();
                ManageProgressTimer();
                StateHasChanged();
            });
        }
        catch (ObjectDisposedException)
        {
            // Component was disposed during async callback
        }
    }

    /// <summary>
    /// Schedules removal of auto-updated cell highlights after 2 seconds.
    /// Resets the timer if new auto-updates arrive before the previous clear.
    /// </summary>
    private void ScheduleAutoUpdateHighlightClear()
    {
        _autoUpdateHighlightCts?.Cancel();
        _autoUpdateHighlightCts = new CancellationTokenSource();
        var token = _autoUpdateHighlightCts.Token;

        _ = Task.Delay(2000, token).ContinueWith(async _ =>
        {
            if (token.IsCancellationRequested || _disposed) return;
            try
            {
                await InvokeAsync(() =>
                {
                    _autoUpdatedCells.Clear();
                    StateHasChanged();
                });
            }
            catch (ObjectDisposedException) { }
        }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        MatrixService.StateChanged -= OnMatrixStateChanged;
        StopProgressTimer();
        _autoUpdateHighlightCts?.Cancel();
        _autoUpdateHighlightCts?.Dispose();
    }
}
