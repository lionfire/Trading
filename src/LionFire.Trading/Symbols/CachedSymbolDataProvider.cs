using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Configuration options for the cached symbol data provider.
/// </summary>
public class CachedSymbolDataProviderOptions
{
    /// <summary>
    /// Default cache duration for query results.
    /// </summary>
    public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Cache duration for symbol availability checks.
    /// </summary>
    public TimeSpan AvailabilityCacheDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Provider-specific cache durations (key = provider name).
    /// </summary>
    public Dictionary<string, TimeSpan> ProviderCacheDurations { get; set; } = new()
    {
        ["CoinGecko"] = TimeSpan.FromMinutes(5),
        ["Binance"] = TimeSpan.FromMinutes(1)
    };
}

/// <summary>
/// A caching decorator that wraps an <see cref="ISymbolDataProvider"/> to cache results
/// and reduce API calls while respecting rate limits.
/// </summary>
public class CachedSymbolDataProvider : ISymbolDataProvider
{
    private readonly ISymbolDataProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedSymbolDataProvider> _logger;
    private readonly CachedSymbolDataProviderOptions _options;

    public CachedSymbolDataProvider(
        ISymbolDataProvider inner,
        IMemoryCache cache,
        IOptions<CachedSymbolDataProviderOptions> options,
        ILogger<CachedSymbolDataProvider> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? new CachedSymbolDataProviderOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string Name => _inner.Name;

    /// <inheritdoc/>
    public int Priority => _inner.Priority;

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExchanges => _inner.SupportedExchanges;

    /// <inheritdoc/>
    public bool CanHandle(SymbolCollectionQuery query) => _inner.CanHandle(query);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SymbolMarketData>> GetTopSymbolsAsync(
        SymbolCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"provider:{Name}:{query.GetCacheKey()}";

        if (_cache.TryGetValue<IReadOnlyList<SymbolMarketData>>(cacheKey, out var cached))
        {
            _logger.LogDebug("Cache hit for {Provider} query: {CacheKey}", Name, cacheKey);
            return cached!;
        }

        _logger.LogDebug("Cache miss for {Provider} query: {CacheKey}", Name, cacheKey);

        var result = await _inner.GetTopSymbolsAsync(query, cancellationToken);

        var cacheDuration = GetCacheDuration();
        _cache.Set(cacheKey, result, cacheDuration);

        _logger.LogDebug("Cached {Count} symbols from {Provider} for {Duration}",
            result.Count, Name, cacheDuration);

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> IsSymbolAvailableAsync(
        string symbol,
        string exchange,
        string area,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"availability:{Name}:{exchange}:{area}:{symbol}";

        if (_cache.TryGetValue<bool>(cacheKey, out var cached))
        {
            _logger.LogDebug("Cache hit for availability check: {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss for availability check: {CacheKey}", cacheKey);

        var result = await _inner.IsSymbolAvailableAsync(symbol, exchange, area, cancellationToken);

        _cache.Set(cacheKey, result, _options.AvailabilityCacheDuration);

        return result;
    }

    /// <summary>
    /// Forces a refresh of cached data for the specified query.
    /// </summary>
    public async Task<IReadOnlyList<SymbolMarketData>> RefreshAsync(
        SymbolCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"provider:{Name}:{query.GetCacheKey()}";
        _cache.Remove(cacheKey);

        _logger.LogInformation("Force refreshing cache for {Provider} query: {CacheKey}", Name, cacheKey);

        return await GetTopSymbolsAsync(query, cancellationToken);
    }

    /// <summary>
    /// Invalidates all cached data for this provider.
    /// </summary>
    public void InvalidateAll()
    {
        // Note: IMemoryCache doesn't support enumeration or bulk removal
        // In a production scenario, consider using a more sophisticated cache
        // that supports pattern-based invalidation
        _logger.LogInformation("Cache invalidation requested for provider {Provider}", Name);
    }

    private TimeSpan GetCacheDuration()
    {
        if (_options.ProviderCacheDurations.TryGetValue(Name, out var duration))
        {
            return duration;
        }
        return _options.DefaultCacheDuration;
    }
}
