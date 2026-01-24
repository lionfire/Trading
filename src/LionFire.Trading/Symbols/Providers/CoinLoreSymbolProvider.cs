using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Symbols.Providers;

/// <summary>
/// Symbol data provider using the CoinLore API.
/// CoinLore provides free access to market cap and volume data without API keys.
/// API docs: https://www.coinlore.com/cryptocurrency-data-api
/// </summary>
/// <remarks>
/// Key features:
/// - Free, no API key required
/// - Provides market cap rankings
/// - Provides 24h volume
/// - Rate limit: ~1 request per second recommended
/// - Returns top coins by market cap by default
/// </remarks>
public class CoinLoreSymbolProvider : ISymbolDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly CoinLoreProviderOptions _options;
    private readonly ILogger<CoinLoreSymbolProvider> _logger;

    public CoinLoreSymbolProvider(
        HttpClient httpClient,
        IOptions<CoinLoreProviderOptions> options,
        ILogger<CoinLoreSymbolProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new CoinLoreProviderOptions();
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
    public string Name => "CoinLore";

    /// <inheritdoc/>
    public int Priority => 50; // Between CoinGecko (100) and Binance (10)

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExchanges => ["Binance", "Bybit", "OKX", "Kraken", "Coinbase"];

    /// <inheritdoc/>
    public bool CanHandle(SymbolCollectionQuery query)
    {
        // CoinLore is good for market cap rankings (returns data sorted by market cap)
        // Also works for volume since it provides both
        return true;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SymbolMarketData>> GetTopSymbolsAsync(
        SymbolCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<SymbolMarketData>();
            var totalNeeded = query.Limit;
            var start = 0;

            while (results.Count < totalNeeded && !cancellationToken.IsCancellationRequested)
            {
                var limit = Math.Min(_options.MaxPerPage, totalNeeded * 2); // Fetch extra for filtering
                var tickers = await FetchTickersAsync(start, limit, cancellationToken);

                if (tickers == null || tickers.Count == 0)
                    break;

                foreach (var ticker in tickers)
                {
                    var marketData = MapToMarketData(ticker, query.QuoteCurrency);
                    if (marketData != null && MeetsFilterCriteria(marketData, query))
                    {
                        results.Add(marketData);
                        if (results.Count >= totalNeeded)
                            break;
                    }
                }

                start += limit;

                // Safety limit - CoinLore has ~15000 coins
                if (start > 1000)
                {
                    _logger.LogWarning("Reached safety limit for CoinLore pagination");
                    break;
                }

                // Rate limiting delay between pages
                if (results.Count < totalNeeded)
                {
                    await Task.Delay(_options.RequestDelay, cancellationToken);
                }
            }

            // Sort results according to query
            results = SortResults(results, query).ToList();

            _logger.LogInformation(
                "CoinLore returned {Count} symbols for query: {Query}",
                results.Count, query.GetSummary());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching symbols from CoinLore");
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
            // Extract base currency from symbol (e.g., "BTCUSDT" -> "BTC")
            var baseCurrency = ExtractBaseCurrency(symbol);
            if (string.IsNullOrEmpty(baseCurrency))
                return false;

            // Search in top coins
            var tickers = await FetchTickersAsync(0, 100, cancellationToken);
            return tickers?.Any(t =>
                t.Symbol?.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) == true) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking symbol availability on CoinLore: {Symbol}", symbol);
            return false;
        }
    }

    private async Task<List<CoinLoreTicker>?> FetchTickersAsync(
        int start,
        int limit,
        CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl}/api/tickers/?start={start}&limit={limit}";

        try
        {
            _logger.LogDebug("Fetching CoinLore tickers: start={Start}, limit={Limit}", start, limit);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CoinLoreTickersResponse>(
                cancellationToken: cancellationToken);

            _logger.LogDebug("CoinLore returned {Count} tickers", result?.Data?.Count ?? 0);

            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error fetching CoinLore tickers");
            return null;
        }
    }

    private SymbolMarketData? MapToMarketData(CoinLoreTicker ticker, string quoteCurrency)
    {
        if (string.IsNullOrEmpty(ticker.Symbol))
            return null;

        // Convert symbol to exchange format: "BTC" → "BTCUSDT"
        var baseCurrency = ticker.Symbol.ToUpperInvariant();
        var quote = quoteCurrency.ToUpperInvariant();
        var tradingSymbol = baseCurrency + quote;

        // Parse market cap (comes as string)
        decimal marketCap = 0;
        if (!string.IsNullOrEmpty(ticker.MarketCapUsd))
        {
            decimal.TryParse(ticker.MarketCapUsd, out marketCap);
        }

        return new SymbolMarketData
        {
            Symbol = tradingSymbol,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quote,
            MarketCapUsd = marketCap,
            Volume24hUsd = (decimal)ticker.Volume24,
            MarketCapRank = ticker.Rank,
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

    private static IEnumerable<SymbolMarketData> SortResults(
        List<SymbolMarketData> results,
        SymbolCollectionQuery query)
    {
        var sorted = query.SortBy.ToLowerInvariant() switch
        {
            "marketcap" => query.Direction == SortDirection.Descending
                ? results.OrderByDescending(r => r.MarketCapUsd)
                : results.OrderBy(r => r.MarketCapUsd),
            _ => query.Direction == SortDirection.Descending
                ? results.OrderByDescending(r => r.Volume24hUsd)
                : results.OrderBy(r => r.Volume24hUsd)
        };

        return sorted;
    }

    private static string? ExtractBaseCurrency(string symbol)
    {
        // Common quote currencies to strip
        var quoteCurrencies = new[] { "USDT", "USD", "BUSD", "USDC", "BTC", "ETH" };

        foreach (var quote in quoteCurrencies)
        {
            if (symbol.EndsWith(quote, StringComparison.OrdinalIgnoreCase))
            {
                return symbol[..^quote.Length];
            }
        }

        return symbol;
    }
}
