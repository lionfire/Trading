using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LionFire.Trading.Automation.Optimization.Scoring;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Result of scanning for a single optimization run's analysis status.
/// </summary>
public record AnalysisScanResult
{
    /// <summary>
    /// Full path to the optimization run directory or zip file.
    /// </summary>
    public required string ResultPath { get; init; }

    /// <summary>
    /// Bot type name (derived from directory structure).
    /// </summary>
    public string? BotType { get; init; }

    /// <summary>
    /// Symbol (derived from directory structure).
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// TimeFrame (derived from directory structure).
    /// </summary>
    public string? TimeFrame { get; init; }

    /// <summary>
    /// Whether step recommendations already exist for this run.
    /// </summary>
    public bool HasRecommendations { get; init; }

    /// <summary>
    /// Last modified time of the results.
    /// </summary>
    public DateTime LastModified { get; init; }
}

/// <summary>
/// Scans optimization result directories for runs that haven't been analyzed yet.
/// Detection: absence of step-recommendations.json in the run directory.
/// </summary>
public class PendingAnalysisScanner
{
    private readonly IOptionsMonitor<BacktestOptions> _backtestOptions;
    private readonly StepRecommendationPersistence _persistence;
    private readonly ILogger<PendingAnalysisScanner> _logger;

    public PendingAnalysisScanner(
        IOptionsMonitor<BacktestOptions> backtestOptions,
        StepRecommendationPersistence persistence,
        ILogger<PendingAnalysisScanner> logger)
    {
        _backtestOptions = backtestOptions;
        _persistence = persistence;
        _logger = logger;
    }

    /// <summary>
    /// Scan for all optimization runs, returning their analysis status.
    /// </summary>
    /// <param name="botTypeFilter">Optional: only scan for this bot type.</param>
    /// <param name="symbolFilter">Optional: only scan for this symbol.</param>
    /// <param name="unanalyzedOnly">If true, only return runs without recommendations.</param>
    public async Task<List<AnalysisScanResult>> ScanAsync(
        string? botTypeFilter = null,
        string? symbolFilter = null,
        bool unanalyzedOnly = true)
    {
        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
        {
            _logger.LogWarning("BacktestOptions.Dir not configured or doesn't exist: {Dir}", baseDir);
            return [];
        }

        var results = new List<AnalysisScanResult>();

        await Task.Run(() =>
        {
            // Structure: {baseDir}/{BotType}/{Symbol}/{TimeFrame}/{DateRange}/{Exchange.Area}/{RunNumber}/
            foreach (var botDir in Directory.GetDirectories(baseDir))
            {
                var botType = Path.GetFileName(botDir);
                if (botTypeFilter != null && !string.Equals(botType, botTypeFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var symbolDir in Directory.GetDirectories(botDir))
                {
                    var symbol = Path.GetFileName(symbolDir);
                    if (symbolFilter != null && !string.Equals(symbol, symbolFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (var tfDir in Directory.GetDirectories(symbolDir))
                    {
                        var timeFrame = Path.GetFileName(tfDir);

                        // Navigate into date range and exchange area directories to find runs
                        ScanRunsRecursive(tfDir, botType, symbol, timeFrame, results, unanalyzedOnly, depth: 0);
                    }
                }
            }
        });

        _logger.LogInformation("Scanned {Total} optimization runs, {Pending} pending analysis",
            results.Count + (unanalyzedOnly ? 0 : results.Count(r => r.HasRecommendations)),
            results.Count(r => !r.HasRecommendations));

        return results;
    }

    /// <summary>
    /// Recursively scan for run directories (numbered directories or zip files) under date range / exchange area.
    /// </summary>
    private void ScanRunsRecursive(string directory, string botType, string symbol, string timeFrame,
        List<AnalysisScanResult> results, bool unanalyzedOnly, int depth)
    {
        if (depth > 3) return; // Safety: don't recurse too deep

        // Check for numbered run directories and zip files at this level
        foreach (var entry in Directory.GetFileSystemEntries(directory))
        {
            var name = Path.GetFileName(entry);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(entry);

            bool isRunDir = Directory.Exists(entry) && int.TryParse(name, out _);
            bool isRunZip = entry.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && int.TryParse(nameWithoutExt, out _);

            if (isRunDir || isRunZip)
            {
                // This is a run — check if it has results and whether it's been analyzed
                bool hasResults;
                if (isRunZip)
                {
                    hasResults = true; // Zip files contain backtests.csv
                }
                else
                {
                    hasResults = File.Exists(Path.Combine(entry, "backtests.csv"));
                }

                if (!hasResults) continue;

                // Check for existing recommendations
                var runDir = isRunZip ? Path.GetDirectoryName(entry)! : entry;
                var hasRecommendations = _persistence.Exists(runDir);

                if (unanalyzedOnly && hasRecommendations) continue;

                var lastModified = isRunZip
                    ? File.GetLastWriteTimeUtc(entry)
                    : Directory.GetLastWriteTimeUtc(entry);

                results.Add(new AnalysisScanResult
                {
                    ResultPath = entry,
                    BotType = botType,
                    Symbol = symbol,
                    TimeFrame = timeFrame,
                    HasRecommendations = hasRecommendations,
                    LastModified = lastModified,
                });
            }
            else if (Directory.Exists(entry) && !name.StartsWith('.'))
            {
                // Not a run directory — recurse (date range or exchange area subdirectory)
                ScanRunsRecursive(entry, botType, symbol, timeFrame, results, unanalyzedOnly, depth + 1);
            }
        }
    }
}
