using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Represents a symbol's entry within a collection, including its state and associated metadata.
/// </summary>
public record SymbolEntry
{
    /// <summary>
    /// The trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = "";

    /// <summary>
    /// The current state of this symbol in the collection.
    /// </summary>
    [JsonPropertyName("state")]
    public SymbolState State { get; init; } = SymbolState.Pending;

    /// <summary>
    /// The most recent market data for this symbol.
    /// May be null if market data hasn't been fetched yet.
    /// </summary>
    [JsonPropertyName("marketData")]
    public SymbolMarketData? MarketData { get; init; }

    /// <summary>
    /// When this symbol was first added to the collection.
    /// </summary>
    [JsonPropertyName("addedAt")]
    public DateTime AddedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When this symbol was excluded (if State is Excluded or Hidden).
    /// Null if symbol has never been excluded.
    /// </summary>
    [JsonPropertyName("excludedAt")]
    public DateTime? ExcludedAt { get; init; }

    /// <summary>
    /// User-provided reason for exclusion.
    /// Null if symbol has not been excluded.
    /// </summary>
    [JsonPropertyName("exclusionReason")]
    public string? ExclusionReason { get; init; }

    /// <summary>
    /// When this symbol was delisted from the exchange (if State is Delisted).
    /// Null if symbol has not been delisted.
    /// </summary>
    [JsonPropertyName("delistedAt")]
    public DateTime? DelistedAt { get; init; }
}
