using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols.Providers;

/// <summary>
/// Response from CoinLore /api/tickers/ endpoint.
/// </summary>
public class CoinLoreTickersResponse
{
    [JsonPropertyName("data")]
    public List<CoinLoreTicker>? Data { get; set; }

    [JsonPropertyName("info")]
    public CoinLoreInfo? Info { get; set; }
}

/// <summary>
/// Info section from CoinLore response.
/// </summary>
public class CoinLoreInfo
{
    [JsonPropertyName("coins_num")]
    public int CoinsNum { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }
}

/// <summary>
/// Individual ticker from CoinLore API.
/// </summary>
public class CoinLoreTicker
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameid")]
    public string? NameId { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("price_usd")]
    public string? PriceUsd { get; set; }

    [JsonPropertyName("percent_change_24h")]
    public string? PercentChange24h { get; set; }

    [JsonPropertyName("percent_change_1h")]
    public string? PercentChange1h { get; set; }

    [JsonPropertyName("percent_change_7d")]
    public string? PercentChange7d { get; set; }

    [JsonPropertyName("market_cap_usd")]
    public string? MarketCapUsd { get; set; }

    [JsonPropertyName("volume24")]
    public double Volume24 { get; set; }

    [JsonPropertyName("volume24a")]
    public double Volume24a { get; set; }

    [JsonPropertyName("csupply")]
    public string? CirculatingSupply { get; set; }

    [JsonPropertyName("tsupply")]
    public string? TotalSupply { get; set; }

    [JsonPropertyName("msupply")]
    public string? MaxSupply { get; set; }
}

/// <summary>
/// Options for configuring the CoinLore provider.
/// </summary>
public class CoinLoreProviderOptions
{
    /// <summary>
    /// Base URL for CoinLore API. Default: https://api.coinlore.net
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.coinlore.net";

    /// <summary>
    /// Maximum results per API call. CoinLore allows up to 100.
    /// </summary>
    public int MaxPerPage { get; set; } = 100;

    /// <summary>
    /// Request timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Delay between requests to avoid rate limiting.
    /// </summary>
    public TimeSpan RequestDelay { get; set; } = TimeSpan.FromMilliseconds(500);
}
