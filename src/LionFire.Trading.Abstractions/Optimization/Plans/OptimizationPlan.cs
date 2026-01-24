namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Defines a reusable optimization plan configuration.
/// </summary>
public record OptimizationPlan
{
    /// <summary>
    /// Unique identifier (human-friendly slug).
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name for the plan.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Optional description of the plan's purpose.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// When the plan was created.
    /// </summary>
    public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the plan was last modified.
    /// </summary>
    public DateTimeOffset Modified { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Plan version, auto-incremented on save.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Bot type name to optimize.
    /// </summary>
    public string Bot { get; init; } = "";

    /// <summary>
    /// Symbol configuration (dynamic or static).
    /// </summary>
    public OptimizationPlanSymbols Symbols { get; init; } = new();

    /// <summary>
    /// Timeframes to test (e.g., "m1", "h1", "d1").
    /// </summary>
    public IReadOnlyList<string> Timeframes { get; init; } = [];

    /// <summary>
    /// Date ranges to test.
    /// </summary>
    public IReadOnlyList<OptimizationDateRange> DateRanges { get; init; } = [];

    /// <summary>
    /// Resolution settings for optimization granularity.
    /// </summary>
    public OptimizationResolution Resolution { get; init; } = new();

    /// <summary>
    /// Optional scoring configuration.
    /// </summary>
    public OptimizationScoring? Scoring { get; init; }

    /// <summary>
    /// Tags for categorization and filtering.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];
}
