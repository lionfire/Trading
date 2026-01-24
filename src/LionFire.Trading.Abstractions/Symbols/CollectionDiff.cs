using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Represents the difference between a snapshot and fresh provider data.
/// Used to detect staleness and pending changes.
/// </summary>
public record CollectionDiff
{
    /// <summary>
    /// Symbols that now qualify for the collection but aren't in the snapshot.
    /// </summary>
    [JsonPropertyName("newSymbols")]
    public IReadOnlyList<SymbolMarketData> NewSymbols { get; init; } = Array.Empty<SymbolMarketData>();

    /// <summary>
    /// Symbols in the snapshot that no longer qualify based on the query criteria.
    /// </summary>
    [JsonPropertyName("removedSymbols")]
    public IReadOnlyList<SymbolEntry> RemovedSymbols { get; init; } = Array.Empty<SymbolEntry>();

    /// <summary>
    /// Symbols that were active but are no longer available on the exchange.
    /// </summary>
    [JsonPropertyName("delistedSymbols")]
    public IReadOnlyList<SymbolEntry> DelistedSymbols { get; init; } = Array.Empty<SymbolEntry>();

    /// <summary>
    /// When this diff was computed.
    /// </summary>
    [JsonPropertyName("computedAt")]
    public DateTimeOffset ComputedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// True if there are any changes (new, removed, or delisted symbols).
    /// </summary>
    [JsonIgnore]
    public bool IsStale => NewSymbols.Count > 0 || RemovedSymbols.Count > 0 || DelistedSymbols.Count > 0;

    /// <summary>
    /// True if the collection is up to date with no pending changes.
    /// </summary>
    [JsonIgnore]
    public bool IsUpToDate => !IsStale;

    /// <summary>
    /// Gets a human-readable summary of the changes.
    /// </summary>
    [JsonIgnore]
    public string Summary => IsStale
        ? $"+{NewSymbols.Count} new, -{RemovedSymbols.Count} removed, {DelistedSymbols.Count} delisted"
        : "Up to date";

    /// <summary>
    /// Gets the total number of changes.
    /// </summary>
    [JsonIgnore]
    public int TotalChanges => NewSymbols.Count + RemovedSymbols.Count + DelistedSymbols.Count;
}
