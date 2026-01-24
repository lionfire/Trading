using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using LionFire.Trading.Symbols;
using LionFire.Trading.Symbols.Providers;

Console.WriteLine("=== Symbol Provider Integration Test ===\n");

// Test CoinGecko
Console.WriteLine("--- CoinGecko Provider ---");
using var cgHttpClient = new HttpClient();
var cgOptions = Options.Create(new CoinGeckoProviderOptions());
var cgLogger = NullLogger<CoinGeckoSymbolProvider>.Instance;
var coinGecko = new CoinGeckoSymbolProvider(cgHttpClient, cgOptions, cgLogger);

var query = new SymbolCollectionQuery
{
    Exchange = "Binance",
    Area = "futures",
    QuoteCurrency = "USDT",
    SortBy = "volume24h",
    Direction = SortDirection.Descending,
    Limit = 10
};

try
{
    // Note: CoinGecko provider is optimized for marketCap sorting, but still works for volume
    Console.WriteLine($"Query: {query.GetSummary()}");
    var cgResults = await coinGecko.GetTopSymbolsAsync(query);
    Console.WriteLine($"CoinGecko returned {cgResults.Count} symbols:");
    foreach (var s in cgResults)
    {
        Console.WriteLine($"  {s.Symbol}: Vol=${s.Volume24hUsd:N0}, MCap=${s.MarketCapUsd:N0}, Rank#{s.MarketCapRank}");
    }
    if (cgResults.Count == 0)
    {
        Console.WriteLine("  (CoinGecko may be rate-limited or the API response was empty)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"CoinGecko error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine();

// Test Binance
Console.WriteLine("--- Binance Provider ---");
using var binHttpClient = new HttpClient();
var binOptions = Options.Create(new BinanceProviderOptions());
var binLogger = NullLogger<BinanceSymbolProvider>.Instance;
var binance = new BinanceSymbolProvider(binHttpClient, binOptions, binLogger);

try
{
    var binResults = await binance.GetTopSymbolsAsync(query);
    Console.WriteLine($"Binance returned {binResults.Count} symbols:");
    foreach (var s in binResults)
    {
        Console.WriteLine($"  {s.Symbol}: Vol=${s.Volume24hUsd:N0}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Binance error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine();

// Test SymbolCollectionService
Console.WriteLine("--- Collection Service ---");
var providers = new ISymbolDataProvider[] { coinGecko, binance };
var serviceLogger = NullLogger<SymbolCollectionService>.Instance;
var service = new SymbolCollectionService(providers, null!, serviceLogger);

try
{
    var snapshot = await service.CreateSnapshotAsync(query, "Test Collection");
    Console.WriteLine($"Created snapshot: {snapshot.Name}");
    Console.WriteLine($"  Active: {snapshot.ActiveCount}, Pending: {snapshot.PendingCount}");
    Console.WriteLine($"  Symbols: {string.Join(", ", snapshot.ActiveSymbols.Take(5).Select(s => s.Symbol))}...");
}
catch (Exception ex)
{
    Console.WriteLine($"Service error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine("\n=== Test Complete ===");
