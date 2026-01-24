using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Symbols.Providers;

/// <summary>
/// Configuration options for the CoinGecko provider.
/// </summary>
public class CoinGeckoProviderOptions
{
    /// <summary>
    /// Base URL for the CoinGecko API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.coingecko.com/api/v3";

    /// <summary>
    /// Maximum results per page (CoinGecko max is 250).
    /// </summary>
    public int MaxPerPage { get; set; } = 250;

    /// <summary>
    /// Timeout for HTTP requests.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Delay between retry attempts after rate limiting.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum retry attempts on failure.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Symbol data provider that fetches market data from CoinGecko API.
/// Best for market cap rankings and broad cryptocurrency coverage.
/// </summary>
public class CoinGeckoSymbolProvider : ISymbolDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoinGeckoSymbolProvider> _logger;
    private readonly CoinGeckoProviderOptions _options;

    // Mapping from CoinGecko symbols to Binance quote currencies
    private static readonly Dictionary<string, string> QuoteCurrencyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["usd"] = "USDT",  // CoinGecko uses USD, Binance uses USDT
        ["usdt"] = "USDT",
        ["btc"] = "BTC",
        ["eth"] = "ETH",
        ["busd"] = "BUSD"
    };

    public CoinGeckoSymbolProvider(
        HttpClient httpClient,
        IOptions<CoinGeckoProviderOptions> options,
        ILogger<CoinGeckoSymbolProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new CoinGeckoProviderOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            _httpClient.Timeout = _options.RequestTimeout;
        }
        catch (InvalidOperationException)
        {
            // HttpClient already started - timeout cannot be changed
        }
    }

    /// <inheritdoc/>
    public string Name => "CoinGecko";

    /// <inheritdoc/>
    public int Priority => 100; // Lower priority than direct exchange providers

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExchanges => ["Binance", "Bybit", "OKX", "Kraken"];

    /// <inheritdoc/>
    public bool CanHandle(SymbolCollectionQuery query)
    {
        // CoinGecko is best for market cap rankings
        return query.SortBy.Equals("marketCap", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SymbolMarketData>> GetTopSymbolsAsync(
        SymbolCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<SymbolMarketData>();
            var vsCurrency = GetVsCurrency(query.QuoteCurrency);
            var order = GetOrderParameter(query);
            var totalNeeded = query.Limit;
            var page = 1;

            while (results.Count < totalNeeded && !cancellationToken.IsCancellationRequested)
            {
                var perPage = Math.Min(_options.MaxPerPage, totalNeeded - results.Count);
                var items = await FetchPageAsync(vsCurrency, order, page, perPage, cancellationToken);

                if (items == null || items.Count == 0)
                    break;

                foreach (var item in items)
                {
                    var marketData = MapToMarketData(item, query.QuoteCurrency);
                    if (marketData != null && MeetsFilterCriteria(marketData, query))
                    {
                        results.Add(marketData);
                        if (results.Count >= totalNeeded)
                            break;
                    }
                }

                page++;

                // Safety limit to prevent infinite loops
                if (page > 10)
                {
                    _logger.LogWarning("Reached maximum page limit for CoinGecko query");
                    break;
                }
            }

            _logger.LogInformation(
                "CoinGecko returned {Count} symbols for query: {Query}",
                results.Count, query.GetSummary());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching symbols from CoinGecko");
            return Array.Empty<SymbolMarketData>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsSymbolAvailableAsync(
        string symbol,
        string exchange,
        string area,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // CoinGecko doesn't have exchange-specific availability
            // This is a basic check if the coin exists
            var coinId = GetCoinIdFromSymbol(symbol);
            var url = $"{_options.BaseUrl}/coins/{coinId}?localization=false&tickers=false&market_data=false&community_data=false&developer_data=false";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking symbol availability for {Symbol}", symbol);
            return false;
        }
    }

    private async Task<List<CoinGeckoMarketItem>?> FetchPageAsync(
        string vsCurrency,
        string order,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl}/coins/markets" +
                  $"?vs_currency={vsCurrency}" +
                  $"&order={order}" +
                  $"&per_page={perPage}" +
                  $"&page={page}" +
                  "&sparkline=false";

        for (var attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("CoinGecko rate limit hit, waiting {Delay}", _options.RetryDelay);
                    await Task.Delay(_options.RetryDelay * (attempt + 1), cancellationToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<CoinGeckoMarketItem>>(
                    cancellationToken: cancellationToken);

                return items;
            }
            catch (HttpRequestException ex) when (attempt < _options.MaxRetries)
            {
                _logger.LogWarning(ex, "HTTP error fetching CoinGecko page {Page}, attempt {Attempt}", page, attempt + 1);
                await Task.Delay(_options.RetryDelay * (attempt + 1), cancellationToken);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (attempt < _options.MaxRetries)
            {
                _logger.LogWarning(ex, "Timeout fetching CoinGecko page {Page}, attempt {Attempt}", page, attempt + 1);
                await Task.Delay(_options.RetryDelay * (attempt + 1), cancellationToken);
            }
        }

        return null;
    }

    private SymbolMarketData? MapToMarketData(CoinGeckoMarketItem item, string quoteCurrency)
    {
        if (string.IsNullOrEmpty(item.Symbol))
            return null;

        // Convert symbol to exchange format: "btc" → "BTCUSDT"
        var baseCurrency = item.Symbol.ToUpperInvariant();
        var quote = quoteCurrency.ToUpperInvariant();
        var tradingSymbol = baseCurrency + quote;

        return new SymbolMarketData
        {
            Symbol = tradingSymbol,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quote,
            MarketCapUsd = item.MarketCap ?? 0,
            Volume24hUsd = item.TotalVolume ?? 0,
            MarketCapRank = item.MarketCapRank ?? 0,
            Source = Name,
            RetrievedAt = DateTime.UtcNow
        };
    }

    private static bool MeetsFilterCriteria(SymbolMarketData data, SymbolCollectionQuery query)
    {
        if (query.MinVolume24h.HasValue && data.Volume24hUsd < query.MinVolume24h.Value)
            return false;

        if (query.MinMarketCap.HasValue && data.MarketCapUsd < query.MinMarketCap.Value)
            return false;

        return true;
    }

    private static string GetVsCurrency(string quoteCurrency)
    {
        // CoinGecko uses lowercase currency codes
        return quoteCurrency.ToLowerInvariant() switch
        {
            "usdt" => "usd", // CoinGecko doesn't have USDT, use USD
            "busd" => "usd",
            _ => quoteCurrency.ToLowerInvariant()
        };
    }

    private static string GetOrderParameter(SymbolCollectionQuery query)
    {
        var field = query.SortBy.ToLowerInvariant() switch
        {
            "marketcap" => "market_cap",
            "volume24h" => "volume",
            _ => "market_cap"
        };

        var direction = query.Direction == SortDirection.Descending ? "desc" : "asc";
        return $"{field}_{direction}";
    }

    private static string GetCoinIdFromSymbol(string tradingSymbol)
    {
        // Extract base currency from trading symbol (e.g., "BTCUSDT" → "bitcoin")
        // This is a simplified mapping - in production, you'd use a proper lookup table
        var symbol = tradingSymbol.ToUpperInvariant();

        // Remove common quote currencies
        foreach (var quote in new[] { "USDT", "USD", "BUSD", "BTC", "ETH" })
        {
            if (symbol.EndsWith(quote))
            {
                symbol = symbol[..^quote.Length];
                break;
            }
        }

        // Map common symbols to CoinGecko IDs
        return symbol.ToLowerInvariant() switch
        {
            "btc" => "bitcoin",
            "eth" => "ethereum",
            "bnb" => "binancecoin",
            "xrp" => "ripple",
            "ada" => "cardano",
            "doge" => "dogecoin",
            "sol" => "solana",
            "dot" => "polkadot",
            "matic" => "matic-network",
            "shib" => "shiba-inu",
            "ltc" => "litecoin",
            "avax" => "avalanche-2",
            "link" => "chainlink",
            "uni" => "uniswap",
            "atom" => "cosmos",
            "xlm" => "stellar",
            "etc" => "ethereum-classic",
            "bch" => "bitcoin-cash",
            _ => symbol.ToLowerInvariant() // Fallback to symbol as ID
        };
    }
}
