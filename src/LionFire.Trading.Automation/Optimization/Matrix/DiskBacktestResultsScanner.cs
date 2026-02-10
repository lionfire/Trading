using LionFire.Trading.Automation.Optimization.Scoring;
using LionFire.Trading.Optimization.Matrix;
using LionFire.Trading.Optimization.Plans;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Scans the disk backtest directory structure to discover completed optimization results
/// for a given plan. Used as a fallback when no execution state exists.
/// </summary>
public class DiskBacktestResultsScanner
{
    private readonly IOptionsMonitor<BacktestOptions> _backtestOptions;
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly ILogger<DiskBacktestResultsScanner> _logger;

    private const double DefaultPassingThreshold = 1.0;

    public DiskBacktestResultsScanner(
        IOptionsMonitor<BacktestOptions> backtestOptions,
        IOptimizationPlanRepository planRepository,
        ILogger<DiskBacktestResultsScanner> logger)
    {
        _backtestOptions = backtestOptions;
        _planRepository = planRepository;
        _logger = logger;
    }

    /// <summary>
    /// Scans the disk for backtest results matching the given plan's bot, symbols, and timeframes.
    /// </summary>
    /// <returns>Dictionary of cell results keyed by "symbol|timeframe".</returns>
    public async Task<Dictionary<string, MatrixCellResult>> ScanForPlanAsync(string planId)
    {
        var results = new Dictionary<string, MatrixCellResult>();

        var plan = await _planRepository.GetAsync(planId);
        if (plan == null)
        {
            _logger.LogWarning("Plan '{PlanId}' not found, cannot scan disk for results", planId);
            return results;
        }

        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir))
        {
            _logger.LogWarning("BacktestOptions.Dir not configured, cannot scan disk for results");
            return results;
        }

        // Plans store PBot names (e.g., "PAtrBot"), disk uses bot names without P prefix (e.g., "AtrBot")
        var diskBotName = NormalizeBotName(plan.Bot);
        _logger.LogDebug("Scanning disk for results: plan={PlanId}, bot={BotName}, baseDir={BaseDir}", planId, diskBotName, baseDir);

        var symbols = plan.Symbols.EffectiveSymbols.ToList();
        var timeframes = plan.Timeframes;

        await Task.Run(() =>
        {
            foreach (var symbol in symbols)
            {
                foreach (var timeframe in timeframes)
                {
                    try
                    {
                        var cellResult = ScanCell(baseDir, diskBotName, symbol, timeframe);
                        if (cellResult != null)
                        {
                            var key = PlanMatrixState.CellKey(symbol, timeframe);
                            results[key] = cellResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error scanning disk for {Symbol}/{Timeframe}", symbol, timeframe);
                    }
                }
            }
        });

        return results;
    }

    private MatrixCellResult? ScanCell(string baseDir, string botName, string symbol, string timeframe)
    {
        var cellDir = Path.Combine(baseDir, botName, symbol, timeframe);
        if (!Directory.Exists(cellDir)) return null;

        var allEntries = new List<BacktestBatchJournalEntry>();
        var dateRangeDirs = Directory.GetDirectories(cellDir);

        foreach (var dateRangeDir in dateRangeDirs)
        {
            var exchangeAreaDirs = Directory.GetDirectories(dateRangeDir);
            foreach (var exchangeAreaDir in exchangeAreaDirs)
            {
                var entries = ReadLatestRun(exchangeAreaDir);
                if (entries.Count > 0)
                {
                    allEntries.AddRange(entries);
                }
            }
        }

        if (allEntries.Count == 0) return null;

        return AggregateEntries(allEntries);
    }

    private List<BacktestBatchJournalEntry> ReadLatestRun(string exchangeAreaDir)
    {
        // Find the highest-numbered run directory or zip file
        var latestRunPath = FindLatestRun(exchangeAreaDir);
        if (latestRunPath == null) return [];

        try
        {
            if (latestRunPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return BacktestResultsReader.ReadFromZip(latestRunPath);
            }
            else
            {
                return BacktestResultsReader.ReadFromDirectory(latestRunPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to read backtest results from {Path}", latestRunPath);
            return [];
        }
    }

    private static string? FindLatestRun(string exchangeAreaDir)
    {
        // Look for numbered directories (e.g., 0000, 0001) and zip files (e.g., 0000.zip, 0001.zip)
        var candidates = new List<(string path, int number)>();

        foreach (var dir in Directory.GetDirectories(exchangeAreaDir))
        {
            var name = Path.GetFileName(dir);
            if (int.TryParse(name, out var num))
            {
                candidates.Add((dir, num));
            }
        }

        foreach (var file in Directory.GetFiles(exchangeAreaDir, "*.zip"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (int.TryParse(name, out var num))
            {
                candidates.Add((file, num));
            }
        }

        if (candidates.Count == 0) return null;

        // Return the highest-numbered run
        return candidates.OrderByDescending(c => c.number).First().path;
    }

    private static MatrixCellResult AggregateEntries(List<BacktestBatchJournalEntry> entries)
    {
        var nonAborted = entries.Where(e => !e.IsAborted).ToList();
        var abortedCount = entries.Count(e => e.IsAborted);

        if (nonAborted.Count == 0)
        {
            return new MatrixCellResult
            {
                BestAd = 0,
                AverageAd = 0,
                TotalBacktests = 0,
                AbortedBacktests = abortedCount,
                PassingCount = 0,
                Grade = OptimizationGrade.Error,
            };
        }

        var adValues = nonAborted.Select(e => e.AD).ToList();
        var bestAd = adValues.Max();
        var averageAd = adValues.Average();
        var passingCount = adValues.Count(ad => ad >= DefaultPassingThreshold);
        var grade = OptimizationGradeComputer.ComputeGrade(bestAd);

        return new MatrixCellResult
        {
            BestAd = bestAd,
            AverageAd = averageAd,
            TotalBacktests = nonAborted.Count,
            AbortedBacktests = abortedCount,
            PassingCount = passingCount,
            Score = passingCount,
            Grade = grade,
        };
    }

    /// <summary>
    /// Converts a plan bot name (e.g., "PAtrBot") to the disk directory name (e.g., "AtrBot").
    /// The "P" prefix is the parameter-type naming convention.
    /// </summary>
    internal static string NormalizeBotName(string planBotName)
    {
        if (planBotName.Length > 1 && planBotName[0] == 'P' && char.IsUpper(planBotName[1]))
        {
            return planBotName[1..];
        }
        return planBotName;
    }
}
