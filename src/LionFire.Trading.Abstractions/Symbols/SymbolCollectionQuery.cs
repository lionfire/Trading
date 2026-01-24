using System.Text.Json.Serialization;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Defines criteria for querying and filtering symbols for a collection.
/// </summary>
public record SymbolCollectionQuery
{
    /// <summary>
    /// The exchange to query (e.g., "Binance", "Bybit").
    /// </summary>
    [JsonPropertyName("exchange")]
    public string Exchange { get; init; } = "Binance";

    /// <summary>
    /// The trading area (e.g., "futures", "spot").
    /// </summary>
    [JsonPropertyName("area")]
    public string Area { get; init; } = "futures";

    /// <summary>
    /// The quote currency to filter by (e.g., "USDT", "USD", "BTC").
    /// </summary>
    [JsonPropertyName("quoteCurrency")]
    public string QuoteCurrency { get; init; } = "USDT";

    /// <summary>
    /// The field to sort by ("volume24h", "marketCap").
    /// </summary>
    [JsonPropertyName("sortBy")]
    public string SortBy { get; init; } = "volume24h";

    /// <summary>
    /// The sort direction.
    /// </summary>
    [JsonPropertyName("direction")]
    public SortDirection Direction { get; init; } = SortDirection.Descending;

    /// <summary>
    /// Maximum number of symbols to include.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; init; } = 50;

    /// <summary>
    /// Minimum 24-hour volume in USD (optional filter).
    /// </summary>
    [JsonPropertyName("minVolume24h")]
    public decimal? MinVolume24h { get; init; }

    /// <summary>
    /// Minimum market cap in USD (optional filter).
    /// </summary>
    [JsonPropertyName("minMarketCap")]
    public decimal? MinMarketCap { get; init; }

    /// <summary>
    /// Validates the query configuration.
    /// </summary>
    /// <returns>A list of validation errors, empty if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Exchange))
            errors.Add("Exchange is required.");

        if (string.IsNullOrWhiteSpace(Area))
            errors.Add("Area is required.");

        if (string.IsNullOrWhiteSpace(QuoteCurrency))
            errors.Add("QuoteCurrency is required.");

        if (string.IsNullOrWhiteSpace(SortBy))
            errors.Add("SortBy is required.");

        if (Limit <= 0)
            errors.Add("Limit must be greater than 0.");

        if (Limit > 500)
            errors.Add("Limit cannot exceed 500.");

        if (MinVolume24h.HasValue && MinVolume24h.Value < 0)
            errors.Add("MinVolume24h cannot be negative.");

        if (MinMarketCap.HasValue && MinMarketCap.Value < 0)
            errors.Add("MinMarketCap cannot be negative.");

        var validSortFields = new[] { "volume24h", "marketCap" };
        if (!validSortFields.Contains(SortBy, StringComparer.OrdinalIgnoreCase))
            errors.Add($"SortBy must be one of: {string.Join(", ", validSortFields)}.");

        return errors;
    }

    /// <summary>
    /// Gets a summary string describing this query.
    /// </summary>
    public string GetSummary()
    {
        var sortDesc = Direction == SortDirection.Descending ? "top" : "bottom";
        return $"{sortDesc} {Limit} {QuoteCurrency} {Area} by {SortBy}";
    }

    /// <summary>
    /// Generates a cache key for this query.
    /// </summary>
    public string GetCacheKey()
    {
        return $"symbols:{Exchange}:{Area}:{QuoteCurrency}:{SortBy}:{Direction}:{Limit}:{MinVolume24h}:{MinMarketCap}";
    }

    #region Factory Methods

    /// <summary>
    /// Creates a query for the top 50 USDT futures by 24h volume.
    /// </summary>
    public static SymbolCollectionQuery Top50UsdtFuturesByVolume() => new()
    {
        Exchange = "Binance",
        Area = "futures",
        QuoteCurrency = "USDT",
        SortBy = "volume24h",
        Direction = SortDirection.Descending,
        Limit = 50
    };

    /// <summary>
    /// Creates a query for the top 100 spot symbols by market cap.
    /// </summary>
    public static SymbolCollectionQuery Top100SpotByMarketCap() => new()
    {
        Exchange = "Binance",
        Area = "spot",
        QuoteCurrency = "USDT",
        SortBy = "marketCap",
        Direction = SortDirection.Descending,
        Limit = 100
    };

    /// <summary>
    /// Creates a query for the top 20 USDT futures by market cap.
    /// </summary>
    public static SymbolCollectionQuery Top20UsdtFuturesByMarketCap() => new()
    {
        Exchange = "Binance",
        Area = "futures",
        QuoteCurrency = "USDT",
        SortBy = "marketCap",
        Direction = SortDirection.Descending,
        Limit = 20
    };

    #endregion
}
