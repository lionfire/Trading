namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Symbol configuration for an optimization plan.
/// </summary>
public record OptimizationPlanSymbols
{
    /// <summary>
    /// Type of symbol source: "dynamic" or "static".
    /// </summary>
    public string Type { get; init; } = "static";

    /// <summary>
    /// For dynamic type: ID of the symbol collection to use.
    /// </summary>
    public string? CollectionId { get; init; }

    /// <summary>
    /// Resolved symbol snapshot (populated at plan creation for dynamic,
    /// or the full list for static).
    /// </summary>
    public IReadOnlyList<string> Snapshot { get; init; } = [];

    /// <summary>
    /// Symbols explicitly excluded from the plan.
    /// </summary>
    public IReadOnlyList<string> ExcludedSymbols { get; init; } = [];

    /// <summary>
    /// Gets the effective symbols (snapshot minus exclusions).
    /// </summary>
    public IEnumerable<string> EffectiveSymbols =>
        Snapshot.Except(ExcludedSymbols, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether this uses a dynamic collection.
    /// </summary>
    public bool IsDynamic => Type.Equals("dynamic", StringComparison.OrdinalIgnoreCase);
}
