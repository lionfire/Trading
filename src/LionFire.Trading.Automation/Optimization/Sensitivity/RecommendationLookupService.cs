using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Source of step recommendations, indicating the lookup tier that produced the result.
/// </summary>
public enum RecommendationSource
{
    /// <summary>Recommendations from a symbol-specific sensitivity analysis.</summary>
    SymbolSpecific,

    /// <summary>Recommendations aggregated from a market-type profile (e.g., CryptoMajor).</summary>
    MarketTypeProfile,

    /// <summary>No recommendations found; caller should use bot defaults.</summary>
    BotDefault,
}

/// <summary>
/// Result of looking up step recommendations with source provenance.
/// </summary>
public record RecommendationLookupResult
{
    /// <summary>
    /// The step recommendations, keyed by parameter name.
    /// Null when Source is BotDefault.
    /// </summary>
    public IReadOnlyDictionary<string, StepRecommendation>? Recommendations { get; init; }

    /// <summary>
    /// The aggregated recommendations from a market-type profile.
    /// Null when Source is not MarketTypeProfile.
    /// </summary>
    public IReadOnlyDictionary<string, AggregatedStepRecommendation>? AggregatedRecommendations { get; init; }

    /// <summary>
    /// Which tier produced the result.
    /// </summary>
    public RecommendationSource Source { get; init; }
}

/// <summary>
/// Tiered lookup for step recommendations:
/// 1. Symbol-specific (from existing StepRecommendationPersistence)
/// 2. Market-type profile (from MarketTypeProfileAggregator)
/// 3. Null (use bot defaults)
/// </summary>
public class RecommendationLookupService
{
    private readonly StepRecommendationPersistence _persistence;
    private readonly MarketTypeProfileAggregator _profileAggregator;
    private readonly ILogger<RecommendationLookupService> _logger;

    public RecommendationLookupService(
        StepRecommendationPersistence persistence,
        MarketTypeProfileAggregator profileAggregator,
        ILogger<RecommendationLookupService> logger)
    {
        _persistence = persistence;
        _profileAggregator = profileAggregator;
        _logger = logger;
    }

    /// <summary>
    /// Look up step recommendations for a plan, falling back through tiers.
    /// </summary>
    /// <param name="planId">Plan ID for symbol-specific lookup.</param>
    /// <param name="botType">Bot type name for profile lookup.</param>
    /// <param name="symbol">Trading symbol for market-type classification.</param>
    public async Task<RecommendationLookupResult> LookupAsync(string planId, string botType, string symbol)
    {
        // Tier 1: Symbol-specific recommendations
        var symbolSpecific = await _persistence.LoadLatestAsync(planId);
        if (symbolSpecific != null && symbolSpecific.Recommendations.Count > 0)
        {
            _logger.LogDebug(
                "Found symbol-specific recommendations for plan {PlanId} ({Count} parameters)",
                planId, symbolSpecific.Recommendations.Count);

            return new RecommendationLookupResult
            {
                Recommendations = symbolSpecific.Recommendations
                    .ToDictionary(r => r.ParameterName, r => r),
                Source = RecommendationSource.SymbolSpecific,
            };
        }

        // Tier 2: Market-type profile
        if (!string.IsNullOrEmpty(botType) && !string.IsNullOrEmpty(symbol))
        {
            var profile = await _profileAggregator.GetProfileAsync(botType, symbol);
            if (profile != null && profile.Recommendations.Count > 0)
            {
                var marketType = MarketTypeClassification.Classify(symbol);
                _logger.LogDebug(
                    "Using {MarketType} profile for {BotType}/{Symbol} ({Count} parameters from {Analyses} analyses)",
                    marketType, botType, symbol, profile.Recommendations.Count, profile.ContributingAnalyses);

                return new RecommendationLookupResult
                {
                    AggregatedRecommendations = profile.Recommendations
                        .ToDictionary(r => r.ParameterName, r => r),
                    Source = RecommendationSource.MarketTypeProfile,
                };
            }
        }

        // Tier 3: No recommendations found
        _logger.LogDebug("No recommendations found for plan {PlanId} ({BotType}/{Symbol}), using bot defaults",
            planId, botType, symbol);

        return new RecommendationLookupResult
        {
            Source = RecommendationSource.BotDefault,
        };
    }
}
