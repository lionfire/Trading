using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LionFire.Trading.Automation;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Reads and writes step recommendations co-located with optimization results.
/// File: step-recommendations.json in the same directory as backtests.csv.
/// </summary>
public class StepRecommendationPersistence
{
    private const string FileName = "step-recommendations.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private readonly IOptionsMonitor<BacktestOptions> _backtestOptions;
    private readonly ILogger<StepRecommendationPersistence> _logger;

    public StepRecommendationPersistence(
        IOptionsMonitor<BacktestOptions> backtestOptions,
        ILogger<StepRecommendationPersistence> logger)
    {
        _backtestOptions = backtestOptions;
        _logger = logger;
    }

    /// <summary>
    /// Save recommendations to the same directory as the optimization results.
    /// </summary>
    public async Task SaveAsync(string resultsDirectory, StepRecommendationSet recommendations)
    {
        var path = GetFilePath(resultsDirectory);

        try
        {
            var json = JsonSerializer.Serialize(recommendations, JsonOptions);
            await File.WriteAllTextAsync(path, json);
            _logger.LogDebug("Saved step recommendations to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save step recommendations to {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Load recommendations from a results directory, if they exist.
    /// </summary>
    public async Task<StepRecommendationSet?> LoadAsync(string resultsDirectory)
    {
        var path = GetFilePath(resultsDirectory);
        if (!File.Exists(path)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<StepRecommendationSet>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read step recommendations from {Path}", path);
            return null;
        }
    }

    /// <summary>
    /// Check whether recommendations exist for a results directory.
    /// </summary>
    public bool Exists(string resultsDirectory)
    {
        return File.Exists(GetFilePath(resultsDirectory));
    }

    /// <summary>
    /// Load the most recent step recommendations for a given plan ID by scanning result directories.
    /// Returns null if no recommendations are found.
    /// </summary>
    public async Task<StepRecommendationSet?> LoadLatestAsync(string planId)
    {
        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
            return null;

        StepRecommendationSet? latest = null;

        try
        {
            // Find all step-recommendations.json files under the base directory
            var files = Directory.GetFiles(baseDir, FileName, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var recSet = JsonSerializer.Deserialize<StepRecommendationSet>(json, JsonOptions);
                    if (recSet == null) continue;

                    // Match by plan ID
                    if (!string.Equals(recSet.PlanId, planId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Keep the most recent
                    if (latest == null || recSet.AnalyzedAt > latest.AnalyzedAt)
                    {
                        latest = recSet;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Skipping unreadable recommendation file: {Path}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error scanning for recommendations with plan ID {PlanId}", planId);
        }

        return latest;
    }

    private static string GetFilePath(string resultsDirectory)
    {
        return Path.Combine(resultsDirectory, FileName);
    }
}
