namespace LionFire.Trading.HistoricalData;

/// <summary>
/// Provides exchange and symbol listing for UI components.
/// Implemented in the Silo project; may not be registered in all hosts.
/// </summary>
public interface IExchangeSymbolsService
{
    IReadOnlyList<string> ListExchanges();
    IReadOnlyList<string> ListExchangeAreas(string exchange);
    IReadOnlyList<string> ListSymbols(string exchange, string area);
    Task<Dictionary<string, decimal>> GetVolumeDataAsync(string exchange, string area);
    Task<Dictionary<string, decimal>> GetMarketCapDataAsync(string exchange, string area);
}
