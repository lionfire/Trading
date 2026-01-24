using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols.Providers;

/// <summary>
/// Represents a 24-hour ticker from Binance API.
/// Used for both spot and futures endpoints.
/// </summary>
public class BinanceTicker24hr
{
    /// <summary>
    /// Trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = "";

    /// <summary>
    /// Price change over 24 hours.
    /// </summary>
    [JsonPropertyName("priceChange")]
    public string? PriceChange { get; set; }

    /// <summary>
    /// Price change percentage over 24 hours.
    /// </summary>
    [JsonPropertyName("priceChangePercent")]
    public string? PriceChangePercent { get; set; }

    /// <summary>
    /// Weighted average price over 24 hours.
    /// </summary>
    [JsonPropertyName("weightedAvgPrice")]
    public string? WeightedAvgPrice { get; set; }

    /// <summary>
    /// Previous close price.
    /// </summary>
    [JsonPropertyName("prevClosePrice")]
    public string? PrevClosePrice { get; set; }

    /// <summary>
    /// Last price.
    /// </summary>
    [JsonPropertyName("lastPrice")]
    public string? LastPrice { get; set; }

    /// <summary>
    /// Last quantity.
    /// </summary>
    [JsonPropertyName("lastQty")]
    public string? LastQty { get; set; }

    /// <summary>
    /// Best bid price.
    /// </summary>
    [JsonPropertyName("bidPrice")]
    public string? BidPrice { get; set; }

    /// <summary>
    /// Best ask price.
    /// </summary>
    [JsonPropertyName("askPrice")]
    public string? AskPrice { get; set; }

    /// <summary>
    /// Open price 24 hours ago.
    /// </summary>
    [JsonPropertyName("openPrice")]
    public string? OpenPrice { get; set; }

    /// <summary>
    /// High price over 24 hours.
    /// </summary>
    [JsonPropertyName("highPrice")]
    public string? HighPrice { get; set; }

    /// <summary>
    /// Low price over 24 hours.
    /// </summary>
    [JsonPropertyName("lowPrice")]
    public string? LowPrice { get; set; }

    /// <summary>
    /// Base asset volume (e.g., BTC volume for BTCUSDT).
    /// </summary>
    [JsonPropertyName("volume")]
    public string? Volume { get; set; }

    /// <summary>
    /// Quote asset volume (e.g., USDT volume for BTCUSDT).
    /// This is the actual USD-equivalent volume.
    /// </summary>
    [JsonPropertyName("quoteVolume")]
    public string? QuoteVolume { get; set; }

    /// <summary>
    /// Open time in milliseconds since epoch.
    /// </summary>
    [JsonPropertyName("openTime")]
    public long OpenTime { get; set; }

    /// <summary>
    /// Close time in milliseconds since epoch.
    /// </summary>
    [JsonPropertyName("closeTime")]
    public long CloseTime { get; set; }

    /// <summary>
    /// First trade ID.
    /// </summary>
    [JsonPropertyName("firstId")]
    public long? FirstId { get; set; }

    /// <summary>
    /// Last trade ID.
    /// </summary>
    [JsonPropertyName("lastId")]
    public long? LastId { get; set; }

    /// <summary>
    /// Number of trades.
    /// </summary>
    [JsonPropertyName("count")]
    public long? Count { get; set; }
}

/// <summary>
/// Represents exchange info from Binance API.
/// </summary>
public class BinanceExchangeInfo
{
    [JsonPropertyName("symbols")]
    public List<BinanceSymbolInfo> Symbols { get; set; } = new();
}

/// <summary>
/// Represents a symbol's info from Binance exchange info endpoint.
/// </summary>
public class BinanceSymbolInfo
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("baseAsset")]
    public string BaseAsset { get; set; } = "";

    [JsonPropertyName("quoteAsset")]
    public string QuoteAsset { get; set; } = "";
}
