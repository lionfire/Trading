using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Represents market data for a trading symbol from a data provider.
/// </summary>
public record SymbolMarketData
{
    /// <summary>
    /// The trading symbol (e.g., "BTCUSDT", "ETHUSDT").
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = "";

    /// <summary>
    /// The base currency of the trading pair (e.g., "BTC" for BTCUSDT).
    /// </summary>
    [JsonPropertyName("baseCurrency")]
    public string BaseCurrency { get; init; } = "";

    /// <summary>
    /// The quote currency of the trading pair (e.g., "USDT" for BTCUSDT).
    /// </summary>
    [JsonPropertyName("quoteCurrency")]
    public string QuoteCurrency { get; init; } = "";

    /// <summary>
    /// Market capitalization in USD.
    /// </summary>
    [JsonPropertyName("marketCapUsd")]
    public decimal MarketCapUsd { get; init; }

    /// <summary>
    /// 24-hour trading volume in USD.
    /// </summary>
    [JsonPropertyName("volume24hUsd")]
    public decimal Volume24hUsd { get; init; }

    /// <summary>
    /// Market cap ranking (1 = highest market cap).
    /// </summary>
    [JsonPropertyName("marketCapRank")]
    public int MarketCapRank { get; init; }

    /// <summary>
    /// The data provider source (e.g., "CoinGecko", "Binance").
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; init; } = "";

    /// <summary>
    /// Timestamp when this data was retrieved from the provider.
    /// </summary>
    [JsonPropertyName("retrievedAt")]
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
}
