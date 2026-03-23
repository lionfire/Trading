using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LionFire.Trading.Automation;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Aggregates symbol-specific step recommendations into market-type-level profiles.
/// Scans all step-recommendations.json files, groups by (BotType, MarketType),
/// and computes confidence-weighted averages per parameter.
/// </summary>
public class MarketTypeProfileAggregator
{
    private const string ProfilesSubdirectory = "profiles";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private readonly IOptionsMonitor<BacktestOptions> _backtestOptions;
    private readonly StepRecommendationPersistence _persistence;
    private readonly ILogger<MarketTypeProfileAggregator> _logger;

    public MarketTypeProfileAggregator(
        IOptionsMonitor<BacktestOptions> backtestOptions,
        StepRecommendationPersistence persistence,
        ILogger<MarketTypeProfileAggregator> logger)
    {
        _backtestOptions = backtestOptions;
        _persistence = persistence;
        _logger = logger;
    }

    /// <summary>
    /// Rebuild all market-type profiles by scanning all recommendation files.
    /// </summary>
    public async Task RebuildAllProfilesAsync(CancellationToken cancellationToken = default)
    {
        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
        {
            _logger.LogWarning("BacktestOptions.Dir not configured or doesn't exist: {Dir}", baseDir);
            return;
        }

        var allRecommendations = await ScanAllRecommendationsAsync(baseDir, cancellationToken);

        _logger.LogInformation("Found {Count} recommendation sets for profile aggregation", allRecommendations.Count);

        // Group by (BotType, MarketType)
        var groups = allRecommendations
            .Where(r => !string.IsNullOrEmpty(r.BotType) && !string.IsNullOrEmpty(r.Symbol))
            .GroupBy(r => (BotType: r.BotType!, MarketType: MarketTypeClassification.Classify(r.Symbol!)));

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var profile = AggregateGroup(group.Key.BotType, group.Key.MarketType, group.ToList());
            await SaveProfileAsync(baseDir, profile);

            _logger.LogInformation(
                "Built profile for {BotType}/{MarketType}: {ParamCount} parameters from {AnalysisCount} analyses",
                profile.BotType, profile.MarketType, profile.Recommendations.Count, profile.ContributingAnalyses);
        }
    }

    /// <summary>
    /// Get the aggregated profile for a symbol's market type.
    /// Classifies the symbol and loads the corresponding profile.
    /// </summary>
    public async Task<MarketTypeProfile?> GetProfileAsync(string botType, string symbol)
    {
        var marketType = MarketTypeClassification.Classify(symbol);
        if (marketType == MarketType.Unknown) return null;

        return await LoadProfileAsync(botType, marketType);
    }

    /// <summary>
    /// Load a specific market-type profile from disk.
    /// </summary>
    public async Task<MarketTypeProfile?> LoadProfileAsync(string botType, MarketType marketType)
    {
        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir)) return null;

        var path = GetProfilePath(baseDir, botType, marketType);
        if (!File.Exists(path)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<MarketTypeProfile>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read market type profile from {Path}", path);
            return null;
        }
    }

    private async Task<List<StepRecommendationSet>> ScanAllRecommendationsAsync(
        string baseDir, CancellationToken cancellationToken)
    {
        var results = new List<StepRecommendationSet>();
        var files = await Task.Run(
            () => Directory.GetFiles(baseDir, "step-recommendations.json", SearchOption.AllDirectories),
            cancellationToken);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var recSet = JsonSerializer.Deserialize<StepRecommendationSet>(json, JsonOptions);
                if (recSet != null)
                {
                    results.Add(recSet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Skipping unreadable recommendation file: {Path}", file);
            }
        }

        return results;
    }

    private static MarketTypeProfile AggregateGroup(
        string botType, MarketType marketType, List<StepRecommendationSet> sets)
    {
        // Flatten all individual recommendations and group by parameter name
        var paramGroups = sets
            .SelectMany(s => s.Recommendations)
            .GroupBy(r => r.ParameterName);

        var aggregated = new List<AggregatedStepRecommendation>();

        foreach (var paramGroup in paramGroups)
        {
            var recs = paramGroup.ToList();
            var totalWeight = recs.Sum(r => r.Confidence);

            double weightedStep;
            double weightedExponent;
            double avgConfidence;

            if (totalWeight > 0)
            {
                weightedStep = recs.Sum(r => r.RecommendedStep * r.Confidence) / totalWeight;
                weightedExponent = recs.Sum(r => r.RecommendedStepExponent * r.Confidence) / totalWeight;
                avgConfidence = totalWeight / recs.Count;
            }
            else
            {
                // All zero confidence - fall back to simple average
                weightedStep = recs.Average(r => r.RecommendedStep);
                weightedExponent = recs.Average(r => r.RecommendedStepExponent);
                avgConfidence = 0;
            }

            // Determine dominant sensitivity level by vote count
            var dominantLevel = recs
                .GroupBy(r => r.Sensitivity.Level)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            aggregated.Add(new AggregatedStepRecommendation
            {
                ParameterName = paramGroup.Key,
                WeightedAverageStep = weightedStep,
                WeightedAverageExponent = weightedExponent,
                AverageConfidence = avgConfidence,
                SampleCount = recs.Count,
                DominantSensitivityLevel = dominantLevel,
            });
        }

        return new MarketTypeProfile
        {
            BotType = botType,
            MarketType = marketType,
            LastUpdated = DateTimeOffset.UtcNow,
            ContributingAnalyses = sets.Count,
            Recommendations = aggregated,
        };
    }

    private async Task SaveProfileAsync(string baseDir, MarketTypeProfile profile)
    {
        var path = GetProfilePath(baseDir, profile.BotType, profile.MarketType);
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(profile, JsonOptions);
        await File.WriteAllTextAsync(path, json);

        _logger.LogDebug("Saved market type profile to {Path}", path);
    }

    private static string GetProfilePath(string baseDir, string botType, MarketType marketType)
    {
        return Path.Combine(baseDir, ProfilesSubdirectory, botType, $"{marketType}.json");
    }
}
