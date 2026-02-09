using DynamicData;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Automation.Optimization.Scoring;
using LionFire.Trading.Journal;
using LionFire.Trading.Optimization;
using ReactiveUI;
using System.IO.Compression;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

namespace LionFire.Trading.Automation.Blazor.Optimization;

/// <summary>
/// IOptimizationResultsVM implementation for viewing saved (completed) optimization results from disk.
/// Loads backtests.csv and OptimizationRunInfo.hjson from the result directory (or zip archive).
/// </summary>
public class SavedResultsVM : ReactiveObject, IOptimizationResultsVM, IDisposable
{
    private readonly SourceCache<BacktestBatchJournalEntry, (int, long)> _sourceCache;
    private readonly Subject<Unit> _changes = new();
    private readonly Subject<Unit> _chartVisibilityChanged = new();

    private SavedResultsVM(List<BacktestBatchJournalEntry> entries, OptimizationRunInfo? runInfo)
    {
        _sourceCache = new SourceCache<BacktestBatchJournalEntry, (int, long)>(e => (e.BatchId, e.Id));
        _sourceCache.AddOrUpdate(entries);
        Backtests = _sourceCache.AsObservableCache();
        OptimizationRunInfo = runInfo;
    }

    /// <summary>
    /// Loads saved results from a result directory or its zip archive.
    /// </summary>
    /// <returns>A SavedResultsVM if results were found, null otherwise.</returns>
    public static SavedResultsVM? LoadFromDirectory(string resultPath)
    {
        List<BacktestBatchJournalEntry> entries;
        OptimizationRunInfo? runInfo = null;

        if (Directory.Exists(resultPath))
        {
            entries = BacktestResultsReader.ReadFromDirectory(resultPath);
            runInfo = LoadRunInfo(resultPath);
        }
        else if (File.Exists(resultPath + ".zip"))
        {
            var zipPath = resultPath + ".zip";
            entries = BacktestResultsReader.ReadFromZip(zipPath);
            runInfo = LoadRunInfoFromZip(zipPath);
        }
        else
        {
            return null;
        }

        if (entries.Count == 0) return null;

        return new SavedResultsVM(entries, runInfo);
    }

    private static OptimizationRunInfo? LoadRunInfo(string directory)
    {
        var path = Path.Combine(directory, "OptimizationRunInfo.hjson");
        if (!File.Exists(path)) return null;

        try
        {
            var hjsonText = File.ReadAllText(path);
            var json = global::Hjson.HjsonValue.Parse(hjsonText).ToString();
            return JsonSerializer.Deserialize<OptimizationRunInfo>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    private static OptimizationRunInfo? LoadRunInfoFromZip(string zipPath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.GetEntry("OptimizationRunInfo.hjson");
            if (entry == null) return null;

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            var hjsonText = reader.ReadToEnd();
            var json = global::Hjson.HjsonValue.Parse(hjsonText).ToString();
            return JsonSerializer.Deserialize<OptimizationRunInfo>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    // --- Progress ---

    public bool IsRunning => false;
    public bool IsCompleted => true;

    public OptimizationProgress Progress
    {
        get
        {
            var count = _sourceCache.Count;
            return new OptimizationProgress
            {
                Queued = count,
                Completed = count,
            };
        }
    }

    // --- Results ---

    public IObservableCache<BacktestBatchJournalEntry, (int, long)>? Backtests { get; }
    public MultiSimContext? MultiSimContext => null;
    public OptimizationRunInfo? OptimizationRunInfo { get; }

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

    public IObservable<Unit> DebouncedChanges => _changes.Throttle(TimeSpan.FromMilliseconds(500));
    public IObservable<Unit> Changes => _changes.AsObservable();

    public void Dispose()
    {
        _sourceCache.Dispose();
        _changes.Dispose();
        _chartVisibilityChanged.Dispose();
    }
}
