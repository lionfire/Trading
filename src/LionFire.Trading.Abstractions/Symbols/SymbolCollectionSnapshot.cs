using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Represents a snapshot of a symbol collection at a point in time.
/// The snapshot captures the query criteria and the resulting symbols with their states.
/// </summary>
public record SymbolCollectionSnapshot
{
    /// <summary>
    /// Unique identifier for this snapshot.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Optional user-friendly name for this collection.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The query criteria that defines this collection.
    /// </summary>
    [JsonPropertyName("query")]
    public SymbolCollectionQuery Query { get; init; } = new();

    /// <summary>
    /// When this snapshot was first created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When this snapshot was last refreshed from providers.
    /// </summary>
    [JsonPropertyName("refreshedAt")]
    public DateTimeOffset? RefreshedAt { get; init; }

    /// <summary>
    /// The symbols included in this collection with their states.
    /// </summary>
    [JsonPropertyName("symbols")]
    public List<SymbolEntry> Symbols { get; init; } = new();

    /// <summary>
    /// The provider that was used to create/refresh this snapshot.
    /// </summary>
    [JsonPropertyName("providerUsed")]
    public string ProviderUsed { get; init; } = "";

    /// <summary>
    /// Optional notes about this collection.
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    #region Computed Properties

    /// <summary>
    /// Gets all symbols in the Active state.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<SymbolEntry> ActiveSymbols =>
        Symbols.Where(s => s.State == SymbolState.Active);

    /// <summary>
    /// Gets all symbols in the Pending state (awaiting user action).
    /// </summary>
    [JsonIgnore]
    public IEnumerable<SymbolEntry> PendingSymbols =>
        Symbols.Where(s => s.State == SymbolState.Pending);

    /// <summary>
    /// Gets all symbols in the Excluded state.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<SymbolEntry> ExcludedSymbols =>
        Symbols.Where(s => s.State == SymbolState.Excluded);

    /// <summary>
    /// Gets all symbols in the Delisted state.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<SymbolEntry> DelistedSymbols =>
        Symbols.Where(s => s.State == SymbolState.Delisted);

    /// <summary>
    /// Gets the count of active symbols.
    /// </summary>
    [JsonIgnore]
    public int ActiveCount => Symbols.Count(s => s.State == SymbolState.Active);

    /// <summary>
    /// Gets the count of pending symbols.
    /// </summary>
    [JsonIgnore]
    public int PendingCount => Symbols.Count(s => s.State == SymbolState.Pending);

    #endregion

    #region Methods

    /// <summary>
    /// Gets a symbol entry by symbol name.
    /// </summary>
    public SymbolEntry? GetSymbol(string symbol)
        => Symbols.FirstOrDefault(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if a symbol is in the collection (any state except Hidden).
    /// </summary>
    public bool ContainsSymbol(string symbol)
        => Symbols.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)
                           && s.State != SymbolState.Hidden);

    /// <summary>
    /// Gets a human-readable summary of this collection.
    /// </summary>
    public string GetSummary()
    {
        var name = Name ?? Query.GetSummary();
        return $"{name} ({ActiveCount} active, {PendingCount} pending)";
    }

    #endregion
}
