using DynamicData;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using LionFire.Trading.Optimization;
using System.Reactive;

namespace LionFire.Trading.Automation.Blazor.Optimization;

/// <summary>
/// Narrow interface for optimization results viewing. Consumed by dashboard widgets
/// and reusable components to decouple them from OneShotOptimizeVM.
/// </summary>
public interface IOptimizationResultsVM
{
    // --- Progress ---

    bool IsRunning { get; }
    bool IsCompleted { get; }
    OptimizationProgress Progress { get; }

    // --- Results ---

    IObservableCache<BacktestBatchJournalEntry, (int, long)>? Backtests { get; }
    MultiSimContext? MultiSimContext { get; }
    OptimizationRunInfo? OptimizationRunInfo { get; }

    // --- Filtering ---

    ResultsFilterState FilterState { get; }
    bool MatchesFilter(BacktestBatchJournalEntry entry);
    IEnumerable<BacktestBatchJournalEntry> GetFilteredBacktests();
    int FilteredCount { get; }
    int TotalCount { get; }

    // --- Chart Visibility ---

    HashSet<string> HiddenFromChart { get; }
    bool IsVisibleInChart(BacktestBatchJournalEntry entry);
    void ToggleChartVisibility(BacktestBatchJournalEntry entry);
    void SetChartVisibility(BacktestBatchJournalEntry entry, bool visible);
    IEnumerable<BacktestBatchJournalEntry> GetChartVisibleBacktests();
    IObservable<Unit> ChartVisibilityChanged { get; }

    // --- Selection ---

    BacktestBatchJournalEntry? SelectedBacktest { get; set; }

    // --- Reactive Notifications ---

    IObservable<Unit> DebouncedChanges { get; }
    IObservable<Unit> Changes { get; }
}
