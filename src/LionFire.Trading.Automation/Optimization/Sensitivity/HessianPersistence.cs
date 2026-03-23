using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Reads and writes Hessian analysis results co-located with optimization results.
/// File: hessian-analysis.json in the same directory as backtests.csv.
/// </summary>
public class HessianPersistence
{
    private const string FileName = "hessian-analysis.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private readonly ILogger<HessianPersistence> _logger;
    private readonly IOptionsMonitor<BacktestOptions>? _backtestOptions;

    public HessianPersistence(ILogger<HessianPersistence> logger)
    {
        _logger = logger;
        _backtestOptions = null;
    }

    public HessianPersistence(
        IOptionsMonitor<BacktestOptions> backtestOptions,
        ILogger<HessianPersistence> logger)
    {
        _backtestOptions = backtestOptions;
        _logger = logger;
    }

    /// <summary>
    /// Save Hessian analysis results to the same directory as the optimization results.
    /// </summary>
    public async Task SaveAsync(string resultsDirectory, HessianAnalysisResult result)
    {
        var path = GetFilePath(resultsDirectory);

        try
        {
            var json = JsonSerializer.Serialize(result, JsonOptions);
            await File.WriteAllTextAsync(path, json);
            _logger.LogDebug("Saved Hessian analysis to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save Hessian analysis to {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Load Hessian analysis results from a results directory, if they exist.
    /// </summary>
    public async Task<HessianAnalysisResult?> LoadAsync(string resultsDirectory)
    {
        var path = GetFilePath(resultsDirectory);
        if (!File.Exists(path)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<HessianAnalysisResult>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Hessian analysis from {Path}", path);
            return null;
        }
    }

    /// <summary>
    /// Check whether Hessian analysis results exist for a results directory.
    /// </summary>
    public bool Exists(string resultsDirectory)
    {
        return File.Exists(GetFilePath(resultsDirectory));
    }

    /// <summary>
    /// Load the most recent Hessian analysis for a given plan ID by scanning result directories.
    /// Returns null if no analysis is found.
    /// </summary>
    public async Task<HessianAnalysisResult?> LoadLatestAsync(string planId)
    {
        if (_backtestOptions == null) return null;

        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
            return null;

        HessianAnalysisResult? latest = null;

        try
        {
            // Find all hessian-analysis.json files under the base directory
            var files = Directory.GetFiles(baseDir, FileName, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var result = JsonSerializer.Deserialize<HessianAnalysisResult>(json, JsonOptions);
                    if (result == null) continue;

                    // Match by plan ID
                    if (!string.Equals(result.PlanId, planId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Keep the most recent
                    if (latest == null || result.AnalyzedAt > latest.AnalyzedAt)
                    {
                        latest = result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Skipping unreadable Hessian analysis file: {Path}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error scanning for Hessian analysis with plan ID {PlanId}", planId);
        }

        return latest;
    }

    private static string GetFilePath(string resultsDirectory)
    {
        return Path.Combine(resultsDirectory, FileName);
    }
}
