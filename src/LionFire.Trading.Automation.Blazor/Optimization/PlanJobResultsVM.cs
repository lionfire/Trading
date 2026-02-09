using DynamicData;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using LionFire.Trading.Optimization;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LionFire.Trading.Automation.Blazor.Optimization;

/// <summary>
/// Lightweight IOptimizationResultsVM that wraps a live OptimizationTask from LocalJobRunner.
/// Used for viewing running/completed plan jobs with the same dashboard widgets as one-shot optimize.
/// </summary>
public class PlanJobResultsVM : ReactiveObject, IOptimizationResultsVM, IDisposable
{
    private readonly OptimizationTask _task;
    private readonly CompositeDisposable _disposables = new();
    private readonly Subject<Unit> _changes = new();
    private readonly Subject<Unit> _debouncedChanges;
    private readonly Subject<Unit> _chartVisibilityChanged = new();

    public PlanJobResultsVM(OptimizationTask task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));

        // Connect to the task's Journal ObservableCache when available
        if (_task.MultiSimContext?.Journal?.ObservableCache != null)
        {
            Backtests = _task.MultiSimContext.Journal.ObservableCache;
        }

        // Set up debounced changes
        _debouncedChanges = new Subject<Unit>();
        _changes.Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(u => _debouncedChanges.OnNext(u))
            .DisposeWith(_disposables);

        // Poll progress periodically while running
        Observable.Interval(TimeSpan.FromMilliseconds(500))
            .TakeWhile(_ => !IsCompleted)
            .Subscribe(_ =>
            {
                UpdateFromTask();
                _changes.OnNext(Unit.Default);
            })
            .DisposeWith(_disposables);
    }

    private void UpdateFromTask()
    {
        // Update backtests reference if it became available
        if (Backtests == null && _task.MultiSimContext?.Journal?.ObservableCache != null)
        {
            Backtests = _task.MultiSimContext.Journal.ObservableCache;
        }

        // Update running/completed state
        var runTask = _task.RunTask;
        if (runTask != null)
        {
            IsRunning = !runTask.IsCompleted;
            IsCompleted = runTask.IsCompleted;
        }
    }

    // --- Progress ---

    public bool IsRunning { get; private set; } = true;
    public bool IsCompleted { get; private set; }

    public OptimizationProgress Progress =>
        _task.OptimizationStrategy?.Progress ?? OptimizationProgress.NoProgress;

    // --- Results ---

    public IObservableCache<BacktestBatchJournalEntry, (int, long)>? Backtests { get; private set; }
    public MultiSimContext? MultiSimContext => _task.MultiSimContext;
    public OptimizationRunInfo? OptimizationRunInfo => _task.MultiSimContext?.OptimizationRunInfo;

    // --- Filtering ---

    public ResultsFilterState FilterState { get; } = new();

    public bool MatchesFilter(BacktestBatchJournalEntry entry)
    {
        if (!FilterState.IncludeAborted && entry.IsAborted) return false;
        if (FilterState.MinAD.HasValue && entry.AD < FilterState.MinAD.Value) return false;
        if (FilterState.MinAMWT.HasValue && entry.AMWT < FilterState.MinAMWT.Value) return false;
        if (FilterState.MinFitness.HasValue && entry.Fitness < FilterState.MinFitness.Value) return false;
        if (FilterState.MinTrades.HasValue && entry.TotalTrades < FilterState.MinTrades.Value) return false;
        if (FilterState.MinWinRate.HasValue && entry.WinRate < FilterState.MinWinRate.Value) return false;
        if (FilterState.MaxDrawdownPercent.HasValue && (entry.MaxBalanceDrawdownPerunum * 100) > FilterState.MaxDrawdownPercent.Value) return false;
        return true;
    }

    public IEnumerable<BacktestBatchJournalEntry> GetFilteredBacktests()
    {
        if (Backtests == null) return Enumerable.Empty<BacktestBatchJournalEntry>();
        return Backtests.Items.Where(MatchesFilter);
    }

    public int FilteredCount => Backtests?.Items.Count(MatchesFilter) ?? 0;
    public int TotalCount => Backtests?.Count ?? 0;

    // --- Chart Visibility ---

    public HashSet<string> HiddenFromChart { get; } = new();

    public bool IsVisibleInChart(BacktestBatchJournalEntry entry)
        => MatchesFilter(entry) && !HiddenFromChart.Contains(entry.StringId);

    public void ToggleChartVisibility(BacktestBatchJournalEntry entry)
    {
        if (HiddenFromChart.Contains(entry.StringId))
            HiddenFromChart.Remove(entry.StringId);
        else
            HiddenFromChart.Add(entry.StringId);
        _chartVisibilityChanged.OnNext(Unit.Default);
        _changes.OnNext(Unit.Default);
    }

    public void SetChartVisibility(BacktestBatchJournalEntry entry, bool visible)
    {
        if (visible)
            HiddenFromChart.Remove(entry.StringId);
        else
            HiddenFromChart.Add(entry.StringId);
        _chartVisibilityChanged.OnNext(Unit.Default);
        _changes.OnNext(Unit.Default);
    }

    public IEnumerable<BacktestBatchJournalEntry> GetChartVisibleBacktests()
    {
        if (Backtests == null) return Enumerable.Empty<BacktestBatchJournalEntry>();
        return Backtests.Items.Where(IsVisibleInChart);
    }

    public IObservable<Unit> ChartVisibilityChanged => _chartVisibilityChanged.AsObservable();

    // --- Selection ---

    public BacktestBatchJournalEntry? SelectedBacktest { get; set; }

    // --- Reactive Notifications ---

    public IObservable<Unit> DebouncedChanges => _debouncedChanges.AsObservable();
    public IObservable<Unit> Changes => _changes.AsObservable();

    public void Dispose()
    {
        _disposables.Dispose();
        _changes.Dispose();
        _debouncedChanges.Dispose();
        _chartVisibilityChanged.Dispose();
    }
}
