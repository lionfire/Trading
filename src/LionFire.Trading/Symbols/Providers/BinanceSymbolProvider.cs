using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Symbols.Providers;

/// <summary>
/// Configuration options for the Binance provider.
/// </summary>
public class BinanceProviderOptions
{
    /// <summary>
    /// Base URL for Binance Spot API.
    /// </summary>
    public string SpotBaseUrl { get; set; } = "https://api.binance.com";

    /// <summary>
    /// Base URL for Binance Futures API.
    /// </summary>
    public string FuturesBaseUrl { get; set; } = "https://fapi.binance.com";

    /// <summary>
    /// Timeout for HTTP requests.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum retry attempts on failure.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}

/// <summary>
/// Symbol data provider that fetches data directly from Binance exchange.
/// Best for accurate volume data and futures symbol availability.
/// </summary>
public class BinanceSymbolProvider : ISymbolDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BinanceSymbolProvider> _logger;
    private readonly BinanceProviderOptions _options;

    // Common quote currencies on Binance
    private static readonly string[] QuoteCurrencies = ["USDT", "BUSD", "USD", "BTC", "ETH", "BNB"];

    public BinanceSymbolProvider(
        HttpClient httpClient,
        IOptions<BinanceProviderOptions> options,
        ILogger<BinanceSymbolProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new BinanceProviderOptions();
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
    public string Name => "Binance";

    /// <inheritdoc/>
    public int Priority => 10; // High priority - direct exchange data

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExchanges => ["Binance"];

    /// <inheritdoc/>
    public bool CanHandle(SymbolCollectionQuery query)
    {
        return query.Exchange.Equals("Binance", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SymbolMarketData>> GetTopSymbolsAsync(
        SymbolCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = GetBaseUrl(query.Area);
            var endpoint = GetTickerEndpoint(query.Area);
            var url = $"{baseUrl}{endpoint}";

            var tickers = await FetchTickersAsync(url, cancellationToken);
            if (tickers == null || tickers.Count == 0)
            {
                _logger.LogWarning("No tickers returned from Binance {Area}", query.Area);
                return Array.Empty<SymbolMarketData>();
            }

            var results = tickers
                .Where(t => MatchesQuoteCurrency(t.Symbol, query.QuoteCurrency))
                .Select(t => MapToMarketData(t, query.QuoteCurrency))
                .Where(m => m != null && MeetsFilterCriteria(m, query))
                .Cast<SymbolMarketData>()
                .ToList();

            // Sort by the requested field
            results = SortResults(results, query);

            // Apply limit
            if (query.Limit > 0 && results.Count > query.Limit)
            {
                results = results.Take(query.Limit).ToList();
            }

            _logger.LogInformation(
                "Binance returned {Count} symbols for query: {Query}",
                results.Count, query.GetSummary());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching symbols from Binance");
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
            var baseUrl = GetBaseUrl(area);
            var endpoint = GetExchangeInfoEndpoint(area);
            var url = $"{baseUrl}{endpoint}";

            var exchangeInfo = await FetchExchangeInfoAsync(url, cancellationToken);
            if (exchangeInfo?.Symbols == null)
                return false;

            var symbolInfo = exchangeInfo.Symbols
                .FirstOrDefault(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

            return symbolInfo != null && symbolInfo.Status.Equals("TRADING", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking symbol availability for {Symbol}", symbol);
            return false;
        }
    }

    private async Task<List<BinanceTicker24hr>?> FetchTickersAsync(
        string url,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    (int)response.StatusCode == 418 || // Binance IP ban
                    (int)response.StatusCode == 429)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? _options.RetryDelay;
                    _logger.LogWarning("Binance rate limit hit, waiting {Delay}", retryAfter);
                    await Task.Delay(retryAfter, cancellationToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var tickers = await response.Content.ReadFromJsonAsync<List<BinanceTicker24hr>>(
                    cancellationToken: cancellationToken);

                return tickers;
            }
            catch (HttpRequestException ex) when (attempt < _options.MaxRetries)
            {
                _logger.LogWarning(ex, "HTTP error fetching Binance tickers, attempt {Attempt}", attempt + 1);
                await Task.Delay(_options.RetryDelay * (attempt + 1), cancellationToken);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (attempt < _options.MaxRetries)
            {
                _logger.LogWarning(ex, "Timeout fetching Binance tickers, attempt {Attempt}", attempt + 1);
                await Task.Delay(_options.RetryDelay * (attempt + 1), cancellationToken);
            }
        }

        return null;
    }

    private async Task<BinanceExchangeInfo?> FetchExchangeInfoAsync(
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BinanceExchangeInfo>(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching Binance exchange info");
            return null;
        }
    }

    private SymbolMarketData? MapToMarketData(BinanceTicker24hr ticker, string quoteCurrency)
    {
        if (string.IsNullOrEmpty(ticker.Symbol))
            return null;

        // Parse quote volume as the USD volume
        if (!decimal.TryParse(ticker.QuoteVolume, out var volume))
            volume = 0;

        // Extract base currency from symbol
        var baseCurrency = ExtractBaseCurrency(ticker.Symbol, quoteCurrency);

        return new SymbolMarketData
        {
            Symbol = ticker.Symbol,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency.ToUpperInvariant(),
            MarketCapUsd = 0, // Binance doesn't provide market cap
            Volume24hUsd = volume,
            MarketCapRank = 0, // Binance doesn't provide ranking
            Source = Name,
            RetrievedAt = DateTime.UtcNow
        };
    }

    private static string ExtractBaseCurrency(string symbol, string quoteCurrency)
    {
        var quote = quoteCurrency.ToUpperInvariant();
        if (symbol.EndsWith(quote, StringComparison.OrdinalIgnoreCase))
        {
            return symbol[..^quote.Length];
        }
        return symbol;
    }

    private static bool MatchesQuoteCurrency(string symbol, string quoteCurrency)
    {
        return symbol.EndsWith(quoteCurrency, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MeetsFilterCriteria(SymbolMarketData data, SymbolCollectionQuery query)
    {
        if (query.MinVolume24h.HasValue && data.Volume24hUsd < query.MinVolume24h.Value)
            return false;

        if (query.MinMarketCap.HasValue && data.MarketCapUsd < query.MinMarketCap.Value)
            return false;

        return true;
    }

    private static List<SymbolMarketData> SortResults(List<SymbolMarketData> results, SymbolCollectionQuery query)
    {
        var sorted = query.SortBy.ToLowerInvariant() switch
        {
            "volume24h" => query.Direction == SortDirection.Descending
                ? results.OrderByDescending(r => r.Volume24hUsd)
                : results.OrderBy(r => r.Volume24hUsd),
            "marketcap" => query.Direction == SortDirection.Descending
                ? results.OrderByDescending(r => r.MarketCapUsd)
                : results.OrderBy(r => r.MarketCapUsd),
            _ => results.OrderByDescending(r => r.Volume24hUsd)
        };

        return sorted.ToList();
    }

    private string GetBaseUrl(string area)
    {
        return area.ToLowerInvariant() switch
        {
            "futures" => _options.FuturesBaseUrl,
            "spot" => _options.SpotBaseUrl,
            _ => _options.SpotBaseUrl
        };
    }

    private static string GetTickerEndpoint(string area)
    {
        return area.ToLowerInvariant() switch
        {
            "futures" => "/fapi/v1/ticker/24hr",
            "spot" => "/api/v3/ticker/24hr",
            _ => "/api/v3/ticker/24hr"
        };
    }

    private static string GetExchangeInfoEndpoint(string area)
    {
        return area.ToLowerInvariant() switch
        {
            "futures" => "/fapi/v1/exchangeInfo",
            "spot" => "/api/v3/exchangeInfo",
            _ => "/api/v3/exchangeInfo"
        };
    }
}
