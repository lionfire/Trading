using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Result of a batch sensitivity analysis run.
/// </summary>
public record BatchAnalysisResult
{
    public int TotalScanned { get; init; }
    public int Analyzed { get; init; }
    public int Failed { get; init; }
    public int Skipped { get; init; }
    public List<string> Errors { get; init; } = [];
}

/// <summary>
/// Processes multiple unanalyzed optimization results in batch.
/// Combines PendingAnalysisScanner (discovery) with SensitivityAnalyzer (analysis)
/// and StepRecommendationPersistence (storage).
/// </summary>
public class BatchSensitivityAnalyzer
{
    private readonly PendingAnalysisScanner _scanner;
    private readonly SensitivityAnalyzer _analyzer;
    private readonly StepRecommendationPersistence _persistence;
    private readonly ILogger<BatchSensitivityAnalyzer> _logger;

    public BatchSensitivityAnalyzer(
        PendingAnalysisScanner scanner,
        SensitivityAnalyzer analyzer,
        StepRecommendationPersistence persistence,
        ILogger<BatchSensitivityAnalyzer> logger)
    {
        _scanner = scanner;
        _analyzer = analyzer;
        _persistence = persistence;
        _logger = logger;
    }

    /// <summary>
    /// Scan for unanalyzed results and process them.
    /// </summary>
    public async Task<BatchAnalysisResult> AnalyzePendingAsync(
        string? botTypeFilter = null,
        string? symbolFilter = null,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var pending = await _scanner.ScanAsync(botTypeFilter, symbolFilter, unanalyzedOnly: true);

        if (pending.Count == 0)
        {
            _logger.LogInformation("No unanalyzed optimization results found");
            return new BatchAnalysisResult { TotalScanned = 0 };
        }

        _logger.LogInformation("Found {Count} unanalyzed optimization runs", pending.Count);

        int analyzed = 0, failed = 0, skipped = 0;
        var errors = new List<string>();

        foreach (var item in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                progress?.Report($"Analyzing {item.BotType}/{item.Symbol}/{item.TimeFrame} ({analyzed + failed + skipped + 1}/{pending.Count})");

                var recommendations = _analyzer.Analyze(
                    item.ResultPath,
                    botType: item.BotType,
                    symbol: item.Symbol,
                    timeFrame: item.TimeFrame);

                if (recommendations.Recommendations.Count == 0)
                {
                    _logger.LogDebug("No recommendations generated for {Path} (insufficient data)", item.ResultPath);
                    skipped++;
                    continue;
                }

                // Save to the directory containing the run (not the run dir itself for zips)
                var saveDir = Directory.Exists(item.ResultPath)
                    ? item.ResultPath
                    : Path.GetDirectoryName(item.ResultPath)!;

                await _persistence.SaveAsync(saveDir, recommendations);

                analyzed++;
                _logger.LogDebug("Analyzed {BotType}/{Symbol}/{TimeFrame}: {Count} parameter recommendations",
                    item.BotType, item.Symbol, item.TimeFrame, recommendations.Recommendations.Count);
            }
            catch (Exception ex)
            {
                failed++;
                var error = $"Failed to analyze {item.ResultPath}: {ex.Message}";
                errors.Add(error);
                _logger.LogWarning(ex, "Failed to analyze {Path}", item.ResultPath);
            }
        }

        _logger.LogInformation("Batch analysis complete: {Analyzed} analyzed, {Skipped} skipped, {Failed} failed",
            analyzed, skipped, failed);

        return new BatchAnalysisResult
        {
            TotalScanned = pending.Count,
            Analyzed = analyzed,
            Failed = failed,
            Skipped = skipped,
            Errors = errors,
        };
    }
}
