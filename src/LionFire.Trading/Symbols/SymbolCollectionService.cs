using System.Threading;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Service that coordinates symbol collection operations including
/// snapshot creation, updates, and symbol state management.
/// </summary>
public class SymbolCollectionService
{
    private readonly IEnumerable<ISymbolDataProvider> _providers;
    private readonly ISymbolCollectionRepository? _repository;
    private readonly ILogger<SymbolCollectionService> _logger;

    public SymbolCollectionService(
        IEnumerable<ISymbolDataProvider> providers,
        ISymbolCollectionRepository? repository,
        ILogger<SymbolCollectionService> logger)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _repository = repository; // Optional - required only for persistence operations
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new snapshot based on the specified query.
    /// </summary>
    public async Task<SymbolCollectionSnapshot> CreateSnapshotAsync(
        SymbolCollectionQuery query,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(query);
        if (provider == null)
        {
            throw new InvalidOperationException($"No provider available for query: {query.GetSummary()}");
        }

        _logger.LogInformation("Creating snapshot with {Provider} for: {Query}",
            provider.Name, query.GetSummary());

        var marketData = await provider.GetTopSymbolsAsync(query, cancellationToken);

        var symbols = marketData.Select(m => new SymbolEntry
        {
            Symbol = m.Symbol,
            State = SymbolState.Active, // New symbols start as Active in fresh collections
            MarketData = m,
            AddedAt = DateTime.UtcNow
        }).ToList();

        var snapshot = new SymbolCollectionSnapshot
        {
            Name = name,
            Query = query,
            Symbols = symbols,
            ProviderUsed = provider.Name,
            RefreshedAt = DateTimeOffset.UtcNow
        };

        if (_repository != null)
        {
            await _repository.SaveAsync(snapshot, cancellationToken);
        }

        _logger.LogInformation("Created snapshot {Id} with {Count} symbols",
            snapshot.Id, symbols.Count);

        return snapshot;
    }

    /// <summary>
    /// Refreshes an existing snapshot, detecting new and removed symbols.
    /// New symbols are added as Pending, removed symbols are kept for user decision.
    /// </summary>
    public async Task<SymbolCollectionSnapshot> RefreshSnapshotAsync(
        SymbolCollectionSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(snapshot.Query);
        if (provider == null)
        {
            throw new InvalidOperationException($"No provider available for query: {snapshot.Query.GetSummary()}");
        }

        _logger.LogInformation("Refreshing snapshot {Id} with {Provider}",
            snapshot.Id, provider.Name);

        var freshData = await provider.GetTopSymbolsAsync(snapshot.Query, cancellationToken);
        var freshSymbols = freshData.Select(m => m.Symbol).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingSymbols = snapshot.Symbols.ToDictionary(s => s.Symbol, StringComparer.OrdinalIgnoreCase);

        var updatedSymbols = new List<SymbolEntry>();

        // Update existing symbols with fresh market data
        foreach (var entry in snapshot.Symbols)
        {
            var freshEntry = freshData.FirstOrDefault(f =>
                f.Symbol.Equals(entry.Symbol, StringComparison.OrdinalIgnoreCase));

            if (freshEntry != null)
            {
                // Symbol still qualifies - update market data
                updatedSymbols.Add(entry with { MarketData = freshEntry });
            }
            else if (entry.State == SymbolState.Active || entry.State == SymbolState.Pending)
            {
                // Symbol no longer qualifies - keep but mark for attention
                // Don't auto-remove, let user decide
                updatedSymbols.Add(entry);
            }
            else
            {
                // Keep excluded/delisted/hidden symbols as-is
                updatedSymbols.Add(entry);
            }
        }

        // Add new symbols as Pending
        foreach (var data in freshData)
        {
            if (!existingSymbols.ContainsKey(data.Symbol))
            {
                updatedSymbols.Add(new SymbolEntry
                {
                    Symbol = data.Symbol,
                    State = SymbolState.Pending,
                    MarketData = data,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        var refreshedSnapshot = snapshot with
        {
            Symbols = updatedSymbols,
            RefreshedAt = DateTimeOffset.UtcNow,
            ProviderUsed = provider.Name
        };

        if (_repository != null)
        {
            await _repository.SaveAsync(refreshedSnapshot, cancellationToken);
        }

        _logger.LogInformation("Refreshed snapshot {Id}: {Active} active, {Pending} pending",
            refreshedSnapshot.Id, refreshedSnapshot.ActiveCount, refreshedSnapshot.PendingCount);

        return refreshedSnapshot;
    }

    /// <summary>
    /// Activates a pending symbol, adding it to the active collection.
    /// </summary>
    public async Task<SymbolCollectionSnapshot> ActivateSymbolAsync(
        SymbolCollectionSnapshot snapshot,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        return await UpdateSymbolStateAsync(snapshot, symbol, SymbolState.Active, null, cancellationToken);
    }

    /// <summary>
    /// Excludes a symbol from the collection.
    /// </summary>
    public async Task<SymbolCollectionSnapshot> ExcludeSymbolAsync(
        SymbolCollectionSnapshot snapshot,
        string symbol,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        return await UpdateSymbolStateAsync(snapshot, symbol, SymbolState.Excluded, reason, cancellationToken);
    }

    /// <summary>
    /// Hides an excluded symbol from the UI.
    /// </summary>
    public async Task<SymbolCollectionSnapshot> HideSymbolAsync(
        SymbolCollectionSnapshot snapshot,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        return await UpdateSymbolStateAsync(snapshot, symbol, SymbolState.Hidden, null, cancellationToken);
    }

    /// <summary>
    /// Marks a symbol as delisted.
    /// </summary>
    public async Task<SymbolCollectionSnapshot> MarkDelistedAsync(
        SymbolCollectionSnapshot snapshot,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var entry = snapshot.GetSymbol(symbol);
        if (entry == null)
        {
            throw new ArgumentException($"Symbol {symbol} not found in snapshot", nameof(symbol));
        }

        var updatedEntry = entry with
        {
            State = SymbolState.Delisted,
            DelistedAt = DateTime.UtcNow
        };

        var updatedSymbols = snapshot.Symbols
            .Select(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) ? updatedEntry : s)
            .ToList();

        var updatedSnapshot = snapshot with { Symbols = updatedSymbols };
        if (_repository != null)
        {
            await _repository.SaveAsync(updatedSnapshot, cancellationToken);
        }

        _logger.LogInformation("Marked symbol {Symbol} as delisted in snapshot {Id}", symbol, snapshot.Id);

        return updatedSnapshot;
    }

    /// <summary>
    /// Gets a snapshot by ID. Requires repository to be configured.
    /// </summary>
    public Task<SymbolCollectionSnapshot?> GetSnapshotAsync(string id, CancellationToken cancellationToken = default)
    {
        RequireRepository();
        return _repository!.LoadAsync(id, cancellationToken);
    }

    /// <summary>
    /// Lists all snapshots. Requires repository to be configured.
    /// </summary>
    public Task<IReadOnlyList<SymbolCollectionSnapshot>> ListSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        RequireRepository();
        return _repository!.ListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a snapshot. Requires repository to be configured.
    /// </summary>
    public Task<bool> DeleteSnapshotAsync(string id, CancellationToken cancellationToken = default)
    {
        RequireRepository();
        return _repository!.DeleteAsync(id, cancellationToken);
    }

    private void RequireRepository()
    {
        if (_repository == null)
        {
            throw new InvalidOperationException("Repository is required for this operation but was not configured.");
        }
    }

    private async Task<SymbolCollectionSnapshot> UpdateSymbolStateAsync(
        SymbolCollectionSnapshot snapshot,
        string symbol,
        SymbolState newState,
        string? reason,
        CancellationToken cancellationToken)
    {
        var entry = snapshot.GetSymbol(symbol);
        if (entry == null)
        {
            throw new ArgumentException($"Symbol {symbol} not found in snapshot", nameof(symbol));
        }

        var updatedEntry = newState switch
        {
            SymbolState.Excluded => entry with
            {
                State = newState,
                ExcludedAt = DateTime.UtcNow,
                ExclusionReason = reason
            },
            SymbolState.Hidden => entry with
            {
                State = newState,
                ExcludedAt = entry.ExcludedAt ?? DateTime.UtcNow
            },
            _ => entry with { State = newState }
        };

        var updatedSymbols = snapshot.Symbols
            .Select(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) ? updatedEntry : s)
            .ToList();

        var updatedSnapshot = snapshot with { Symbols = updatedSymbols };
        if (_repository != null)
        {
            await _repository.SaveAsync(updatedSnapshot, cancellationToken);
        }

        _logger.LogInformation("Updated symbol {Symbol} to state {State} in snapshot {Id}",
            symbol, newState, snapshot.Id);

        return updatedSnapshot;
    }

    private ISymbolDataProvider? GetProvider(SymbolCollectionQuery query)
    {
        // Find the best provider for this query
        return _providers
            .Where(p => p.CanHandle(query))
            .OrderBy(p => p.Priority)
            .FirstOrDefault();
    }
}
