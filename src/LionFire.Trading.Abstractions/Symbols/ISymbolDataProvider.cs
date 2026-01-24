using System.Threading;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Interface for providers that supply symbol market data and availability information.
/// </summary>
public interface ISymbolDataProvider
{
    /// <summary>
    /// Gets the name of this provider (e.g., "CoinGecko", "Binance").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the priority of this provider (lower = higher priority).
    /// Used when multiple providers can fulfill a query.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets the supported exchanges for this provider.
    /// </summary>
    IReadOnlyList<string> SupportedExchanges { get; }

    /// <summary>
    /// Retrieves top symbols matching the specified query criteria.
    /// </summary>
    /// <param name="query">The query defining filter and sort criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of symbols with their market data, sorted according to the query.</returns>
    Task<IReadOnlyList<SymbolMarketData>> GetTopSymbolsAsync(
        SymbolCollectionQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a symbol is currently available for trading on the specified exchange.
    /// </summary>
    /// <param name="symbol">The trading symbol (e.g., "BTCUSDT").</param>
    /// <param name="exchange">The exchange name (e.g., "Binance").</param>
    /// <param name="area">The trading area (e.g., "futures", "spot").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the symbol is available for trading, false otherwise.</returns>
    Task<bool> IsSymbolAvailableAsync(
        string symbol,
        string exchange,
        string area,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if this provider can handle the specified query.
    /// </summary>
    /// <param name="query">The query to check.</param>
    /// <returns>True if this provider can fulfill the query, false otherwise.</returns>
    bool CanHandle(SymbolCollectionQuery query);
}
