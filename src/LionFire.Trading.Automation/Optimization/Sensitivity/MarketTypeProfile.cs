namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Aggregated step recommendations for a specific market type and bot type combination.
/// Built by averaging confidence-weighted recommendations from multiple symbol-specific analyses.
/// </summary>
public record MarketTypeProfile
{
    public required string BotType { get; init; }
    public MarketType MarketType { get; init; }
    public DateTimeOffset LastUpdated { get; init; }

    /// <summary>Number of individual recommendation sets that contributed to this profile.</summary>
    public int ContributingAnalyses { get; init; }

    /// <summary>Per-parameter aggregated recommendations.</summary>
    public required List<AggregatedStepRecommendation> Recommendations { get; init; }
}

/// <summary>
/// Confidence-weighted average of step recommendations from multiple analyses.
/// </summary>
public record AggregatedStepRecommendation
{
    public required string ParameterName { get; init; }
    public double WeightedAverageStep { get; init; }
    public double WeightedAverageExponent { get; init; }
    public double AverageConfidence { get; init; }
    public int SampleCount { get; init; }
    public SensitivityLevel DominantSensitivityLevel { get; init; }
}
