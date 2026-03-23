using System.Text.Json;
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

        var runSummaries = new List<RunSummary>();
        var dateRangeDirs = Directory.GetDirectories(cellDir);

        foreach (var dateRangeDir in dateRangeDirs)
        {
            var exchangeAreaDirs = Directory.GetDirectories(dateRangeDir);
            foreach (var exchangeAreaDir in exchangeAreaDirs)
            {
                var summary = GetRunSummary(exchangeAreaDir);
                if (summary != null)
                {
                    runSummaries.Add(summary);
                }
            }
        }

        if (runSummaries.Count == 0) return null;

        return AggregateSummaries(runSummaries);
    }

    private RunSummary? GetRunSummary(string exchangeAreaDir)
    {
        var latestRunPath = FindLatestRun(exchangeAreaDir);
        if (latestRunPath == null) return null;

        var summaryPath = GetSummaryPath(latestRunPath);

        // Try to read cached summary
        try
        {
            if (File.Exists(summaryPath))
            {
                var sourceLastWrite = latestRunPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                    ? File.GetLastWriteTimeUtc(latestRunPath)
                    : Directory.GetLastWriteTimeUtc(latestRunPath);

                if (File.GetLastWriteTimeUtc(summaryPath) >= sourceLastWrite)
                {
                    var json = File.ReadAllText(summaryPath);
                    var cached = JsonSerializer.Deserialize<RunSummary>(json);
                    if (cached is { Version: 3 })
                    {
                        return cached;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to read cached summary from {Path}, will regenerate", summaryPath);
        }

        // Cache miss — read from source and write summary
        try
        {
            List<BacktestBatchJournalEntry> entries;
            if (latestRunPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                entries = BacktestResultsReader.ReadFromZip(latestRunPath);
            }
            else
            {
                entries = BacktestResultsReader.ReadFromDirectory(latestRunPath);
            }

            if (entries.Count == 0) return null;

            var summary = ComputeRunSummary(entries);
            WriteSummary(summaryPath, summary);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to read backtest results from {Path}", latestRunPath);
            return null;
        }
    }

    private static string GetSummaryPath(string runPath)
    {
        // For "0001.zip" -> "0001.summary.json", for directory "0001" -> "0001.summary.json"
        var basePath = runPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            ? runPath[..^4]
            : runPath;
        return basePath + ".summary.json";
    }

    private static RunSummary ComputeRunSummary(List<BacktestBatchJournalEntry> entries)
    {
        var nonAborted = entries.Where(e => !e.IsAborted).ToList();
        var abortedCount = entries.Count(e => e.IsAborted);

        if (nonAborted.Count == 0)
        {
            return new RunSummary
            {
                BestAd = 0,
                AverageAd = 0,
                AdSum = 0,
                TotalBacktests = 0,
                AbortedBacktests = abortedCount,
                PassingCount = 0,
            };
        }

        var adValues = nonAborted.Select(e => e.AD).ToList();
        var adSum = adValues.Sum();

        return new RunSummary
        {
            BestAd = adValues.Max(),
            AverageAd = adSum / adValues.Count,
            AdSum = adSum,
            TotalBacktests = nonAborted.Count,
            AbortedBacktests = abortedCount,
            PassingCount = adValues.Count(ad => ad >= DefaultPassingThreshold),
        };
    }

    private void WriteSummary(string summaryPath, RunSummary summary)
    {
        try
        {
            var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(summaryPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to write summary cache to {Path}", summaryPath);
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

    private static MatrixCellResult AggregateSummaries(List<RunSummary> summaries)
    {
        var totalBacktests = summaries.Sum(s => s.TotalBacktests);
        var abortedBacktests = summaries.Sum(s => s.AbortedBacktests);

        if (totalBacktests == 0)
        {
            return new MatrixCellResult
            {
                BestAd = 0,
                AverageAd = 0,
                TotalBacktests = 0,
                AbortedBacktests = abortedBacktests,
                PassingCount = 0,
                Grade = OptimizationGrade.Error,
                AchievedLevelOfDetail = 0,
                BacktestsAtLevel = 0,
            };
        }

        var bestAd = summaries.Max(s => s.BestAd);
        var adSum = summaries.Sum(s => s.AdSum);
        var averageAd = adSum / totalBacktests;
        var passingCount = summaries.Sum(s => s.PassingCount);
        var grade = OptimizationGradeComputer.ComputeGrade(bestAd);

        return new MatrixCellResult
        {
            BestAd = bestAd,
            AverageAd = averageAd,
            TotalBacktests = totalBacktests,
            AbortedBacktests = abortedBacktests,
            PassingCount = passingCount,
            Score = passingCount,
            Grade = grade,
            AchievedLevelOfDetail = 0, // Disk-scanned results predate LoD system; treat as comprehensive (L0)
            BacktestsAtLevel = totalBacktests,
        };
    }

    /// <summary>
    /// Scans the disk for per-date-range results for a specific cell (symbol × timeframe) within a plan.
    /// Used as fallback when execution state is unavailable (e.g., after Silo restart).
    /// </summary>
    public async Task<List<DiskCellJobInfo>> ScanCellDetailAsync(string planId, string symbol, string timeframe)
    {
        var results = new List<DiskCellJobInfo>();

        var plan = await _planRepository.GetAsync(planId);
        if (plan == null)
        {
            _logger.LogWarning("Plan '{PlanId}' not found, cannot scan cell detail", planId);
            return results;
        }

        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir))
        {
            _logger.LogWarning("BacktestOptions.Dir not configured, cannot scan cell detail");
            return results;
        }

        var diskBotName = NormalizeBotName(plan.Bot);
        var cellDir = Path.Combine(baseDir, diskBotName, symbol, timeframe);

        if (!Directory.Exists(cellDir)) return results;

        await Task.Run(() =>
        {
            foreach (var dateRangeDir in Directory.GetDirectories(cellDir))
            {
                var dateRangeName = Path.GetFileName(dateRangeDir);
                var exchangeAreaDirs = Directory.GetDirectories(dateRangeDir);

                foreach (var exchangeAreaDir in exchangeAreaDirs)
                {
                    try
                    {
                        var latestRunPath = FindLatestRun(exchangeAreaDir);
                        if (latestRunPath == null) continue;

                        var summary = GetRunSummary(exchangeAreaDir);
                        if (summary == null) continue;

                        var lastModified = latestRunPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                            ? File.GetLastWriteTimeUtc(latestRunPath)
                            : Directory.GetLastWriteTimeUtc(latestRunPath);

                        var exchangeArea = Path.GetFileName(exchangeAreaDir);

                        // Store path relative to baseDir to avoid absolute paths in URLs
                        var relativePath = Path.GetRelativePath(baseDir, latestRunPath);

                        results.Add(new DiskCellJobInfo
                        {
                            DateRangeName = dateRangeName,
                            ExchangeArea = exchangeArea,
                            BestAd = summary.BestAd,
                            AverageAd = summary.AverageAd,
                            TotalBacktests = summary.TotalBacktests,
                            AbortedBacktests = summary.AbortedBacktests,
                            PassingCount = summary.PassingCount,
                            ResultPath = relativePath,
                            LastModified = lastModified,
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error scanning cell detail for {DateRangeDir}/{ExchangeAreaDir}",
                            dateRangeName, Path.GetFileName(exchangeAreaDir));
                    }
                }
            }
        });

        return results;
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

/// <summary>
/// Per-date-range result info discovered from disk scanning.
/// </summary>
public record DiskCellJobInfo
{
    public string DateRangeName { get; init; } = "";
    public string ExchangeArea { get; init; } = "";
    public double BestAd { get; init; }
    public double AverageAd { get; init; }
    public int TotalBacktests { get; init; }
    public int AbortedBacktests { get; init; }
    public int PassingCount { get; init; }
    public string ResultPath { get; init; } = "";
    public DateTime LastModified { get; init; }
}
