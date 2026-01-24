using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols.Providers;

/// <summary>
/// Represents a market item from the CoinGecko /coins/markets endpoint.
/// </summary>
public class CoinGeckoMarketItem
{
    /// <summary>
    /// CoinGecko's unique identifier for the coin (e.g., "bitcoin", "ethereum").
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// The coin's ticker symbol (e.g., "btc", "eth").
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = "";

    /// <summary>
    /// The full name of the coin (e.g., "Bitcoin", "Ethereum").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Current price in the requested vs_currency.
    /// </summary>
    [JsonPropertyName("current_price")]
    public decimal? CurrentPrice { get; set; }

    /// <summary>
    /// Market capitalization in the requested vs_currency.
    /// </summary>
    [JsonPropertyName("market_cap")]
    public decimal? MarketCap { get; set; }

    /// <summary>
    /// Market cap ranking (1 = highest market cap).
    /// </summary>
    [JsonPropertyName("market_cap_rank")]
    public int? MarketCapRank { get; set; }

    /// <summary>
    /// 24-hour trading volume in the requested vs_currency.
    /// </summary>
    [JsonPropertyName("total_volume")]
    public decimal? TotalVolume { get; set; }

    /// <summary>
    /// 24-hour high price.
    /// </summary>
    [JsonPropertyName("high_24h")]
    public decimal? High24h { get; set; }

    /// <summary>
    /// 24-hour low price.
    /// </summary>
    [JsonPropertyName("low_24h")]
    public decimal? Low24h { get; set; }

    /// <summary>
    /// Price change in 24 hours.
    /// </summary>
    [JsonPropertyName("price_change_24h")]
    public decimal? PriceChange24h { get; set; }

    /// <summary>
    /// Price change percentage in 24 hours.
    /// </summary>
    [JsonPropertyName("price_change_percentage_24h")]
    public decimal? PriceChangePercentage24h { get; set; }

    /// <summary>
    /// Circulating supply.
    /// </summary>
    [JsonPropertyName("circulating_supply")]
    public decimal? CirculatingSupply { get; set; }

    /// <summary>
    /// Total supply (null if infinite).
    /// </summary>
    [JsonPropertyName("total_supply")]
    public decimal? TotalSupply { get; set; }

    /// <summary>
    /// Timestamp of last update.
    /// </summary>
    [JsonPropertyName("last_updated")]
    public DateTime? LastUpdated { get; set; }
}
