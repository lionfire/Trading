using System.Threading;
using LionFire.Trading.Symbols;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Provides consistent tiebreaker ordering for optimization jobs.
/// Order: coarser timeframes first, then by symbol rank (volume/marketcap), then alphabetical.
/// </summary>
public class JobOrderingHelper
{
    private readonly ISymbolDataProvider? _symbolDataProvider;
    private Dictionary<string, int>? _symbolRanks;
    private readonly SemaphoreSlim _rankLock = new(1, 1);

    public JobOrderingHelper(ISymbolDataProvider? symbolDataProvider = null)
    {
        _symbolDataProvider = symbolDataProvider;
    }

    /// <summary>
    /// Gets a sort key for timeframe coarseness. Lower = coarser = higher priority.
    /// Negated TimeSpan ticks so that coarser (larger) timeframes sort first.
    /// </summary>
    public static long GetTimeframeSortKey(string timeframe)
    {
        try
        {
            var tf = TimeFrame.Parse(timeframe);
            // Negate so larger timeframes (coarser) come first
            return -tf.TimeSpan.Ticks;
        }
        catch
        {
            return 0; // Unknown timeframes sort neutral
        }
    }

    /// <summary>
    /// Gets a sort key for symbol ranking. Lower = higher volume/marketcap = higher priority.
    /// Returns int.MaxValue for unknown symbols (sorts last, before alphabetical tiebreaker).
    /// </summary>
    public int GetSymbolSortKey(string symbol)
    {
        var ranks = _symbolRanks;
        if (ranks != null && ranks.TryGetValue(symbol, out var rank))
        {
            return rank;
        }
        return int.MaxValue; // Unknown symbols fall through to alphabetical
    }

    /// <summary>
    /// Loads symbol rankings from the data provider. Call this once before using GetSymbolSortKey.
    /// Safe to call multiple times; will only load once.
    /// </summary>
    public async Task EnsureSymbolRanksLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_symbolRanks != null || _symbolDataProvider == null) return;

        await _rankLock.WaitAsync(cancellationToken);
        try
        {
            if (_symbolRanks != null) return; // Double-check after lock

            var query = new SymbolCollectionQuery
            {
                Limit = 200,
                SortBy = "volume24h",
                Direction = SortDirection.Descending
            };

            var symbols = await _symbolDataProvider.GetTopSymbolsAsync(query, cancellationToken);
            var ranks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < symbols.Count; i++)
            {
                var sym = symbols[i].Symbol;
                if (!ranks.ContainsKey(sym))
                {
                    ranks[sym] = i + 1; // 1-based rank
                }
            }

            _symbolRanks = ranks;
        }
        catch
        {
            // If loading fails, use empty ranks (everything falls to alphabetical)
            _symbolRanks = new Dictionary<string, int>();
        }
        finally
        {
            _rankLock.Release();
        }
    }
}
