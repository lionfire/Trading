using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Configuration options for staleness detection.
/// </summary>
public class StalenessDetectorOptions
{
    /// <summary>
    /// How long to cache staleness results.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Whether to check for delisted symbols (requires additional API calls).
    /// </summary>
    public bool CheckForDelisted { get; set; } = true;

    /// <summary>
    /// Maximum number of symbols to check for delisting per detection run.
    /// </summary>
    public int MaxDelistingChecks { get; set; } = 20;
}

/// <summary>
/// Detects when a collection snapshot has become stale compared to fresh provider data.
/// </summary>
public class StalenessDetector
{
    private readonly IEnumerable<ISymbolDataProvider> _providers;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StalenessDetector> _logger;
    private readonly StalenessDetectorOptions _options;

    public StalenessDetector(
        IEnumerable<ISymbolDataProvider> providers,
        IMemoryCache cache,
        IOptions<StalenessDetectorOptions> options,
        ILogger<StalenessDetector> logger)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? new StalenessDetectorOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Detects staleness for a snapshot by comparing to fresh provider data.
    /// Results are cached for the configured duration.
    /// </summary>
    /// <param name="snapshot">The snapshot to check.</param>
    /// <param name="forceRefresh">If true, bypasses the cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A diff showing what has changed.</returns>
    public async Task<CollectionDiff> DetectStalenessAsync(
        SymbolCollectionSnapshot snapshot,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"staleness:{snapshot.Id}:{snapshot.RefreshedAt?.Ticks}";

        if (!forceRefresh && _cache.TryGetValue<CollectionDiff>(cacheKey, out var cached))
        {
            _logger.LogDebug("Cache hit for staleness check: {SnapshotId}", snapshot.Id);
            return cached!;
        }

        _logger.LogDebug("Computing staleness for snapshot {SnapshotId}", snapshot.Id);

        var provider = GetProvider(snapshot.Query);
        if (provider == null)
        {
            _logger.LogWarning("No provider available for staleness check: {Query}", snapshot.Query.GetSummary());
            return new CollectionDiff();
        }

        // Fetch fresh data
        var freshData = await provider.GetTopSymbolsAsync(snapshot.Query, cancellationToken);
        var freshSymbols = freshData.ToDictionary(m => m.Symbol, StringComparer.OrdinalIgnoreCase);

        // Get current active symbols from snapshot
        var activeSymbols = snapshot.ActiveSymbols
            .ToDictionary(s => s.Symbol, StringComparer.OrdinalIgnoreCase);

        // Find new symbols (in fresh but not in active)
        var newSymbols = freshData
            .Where(f => !activeSymbols.ContainsKey(f.Symbol))
            .ToList();

        // Find removed symbols (in active but not in fresh)
        var removedSymbols = snapshot.ActiveSymbols
            .Where(s => !freshSymbols.ContainsKey(s.Symbol))
            .ToList();

        // Check for delisted symbols
        var delistedSymbols = new List<SymbolEntry>();
        if (_options.CheckForDelisted)
        {
            delistedSymbols = await CheckForDelistedSymbolsAsync(
                snapshot, provider, cancellationToken);
        }

        var diff = new CollectionDiff
        {
            NewSymbols = newSymbols,
            RemovedSymbols = removedSymbols,
            DelistedSymbols = delistedSymbols,
            ComputedAt = DateTimeOffset.UtcNow
        };

        _cache.Set(cacheKey, diff, _options.CacheDuration);

        _logger.LogInformation(
            "Staleness check for {SnapshotId}: {Summary}",
            snapshot.Id, diff.Summary);

        return diff;
    }

    /// <summary>
    /// Invalidates the cached staleness result for a snapshot.
    /// </summary>
    public void InvalidateCache(SymbolCollectionSnapshot snapshot)
    {
        var cacheKey = $"staleness:{snapshot.Id}:{snapshot.RefreshedAt?.Ticks}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Invalidated staleness cache for {SnapshotId}", snapshot.Id);
    }

    private async Task<List<SymbolEntry>> CheckForDelistedSymbolsAsync(
        SymbolCollectionSnapshot snapshot,
        ISymbolDataProvider provider,
        CancellationToken cancellationToken)
    {
        var delisted = new List<SymbolEntry>();

        // Only check a limited number of symbols to avoid excessive API calls
        var symbolsToCheck = snapshot.ActiveSymbols
            .Take(_options.MaxDelistingChecks)
            .ToList();

        foreach (var entry in symbolsToCheck)
        {
            try
            {
                var isAvailable = await provider.IsSymbolAvailableAsync(
                    entry.Symbol,
                    snapshot.Query.Exchange,
                    snapshot.Query.Area,
                    cancellationToken);

                if (!isAvailable)
                {
                    delisted.Add(entry);
                    _logger.LogInformation("Symbol {Symbol} appears to be delisted", entry.Symbol);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking availability for {Symbol}", entry.Symbol);
            }
        }

        return delisted;
    }

    private ISymbolDataProvider? GetProvider(SymbolCollectionQuery query)
    {
        return _providers
            .Where(p => p.CanHandle(query))
            .OrderBy(p => p.Priority)
            .FirstOrDefault();
    }
}
