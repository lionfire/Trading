using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using LionFire.Trading.Symbols;
using LionFire.Trading.Symbols.Providers;

Console.WriteLine("=== Symbol Provider Integration Test ===\n");

// Query for volume-based sorting
var volumeQuery = new SymbolCollectionQuery
{
    Exchange = "Binance",
    Area = "futures",
    QuoteCurrency = "USDT",
    SortBy = "volume24h",
    Direction = SortDirection.Descending,
    Limit = 10
};

// Query for market cap sorting
var marketCapQuery = new SymbolCollectionQuery
{
    Exchange = "Binance",
    Area = "futures",
    QuoteCurrency = "USDT",
    SortBy = "marketCap",
    Direction = SortDirection.Descending,
    Limit = 10
};

// Test CoinLore (recommended - free, no API key, has market cap)
Console.WriteLine("--- CoinLore Provider (Market Cap) ---");
using var clHttpClient = new HttpClient();
var clOptions = Options.Create(new CoinLoreProviderOptions());
var clLogger = NullLogger<CoinLoreSymbolProvider>.Instance;
var coinLore = new CoinLoreSymbolProvider(clHttpClient, clOptions, clLogger);

try
{
    Console.WriteLine($"Query: {marketCapQuery.GetSummary()}");
    var clResults = await coinLore.GetTopSymbolsAsync(marketCapQuery);
    Console.WriteLine($"CoinLore returned {clResults.Count} symbols:");
    foreach (var s in clResults)
    {
        Console.WriteLine($"  {s.Symbol}: MCap=${s.MarketCapUsd:N0}, Vol=${s.Volume24hUsd:N0}, Rank#{s.MarketCapRank}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"CoinLore error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine();

// Test Binance (accurate volume data directly from exchange)
Console.WriteLine("--- Binance Provider (Volume) ---");
using var binHttpClient = new HttpClient();
var binOptions = Options.Create(new BinanceProviderOptions());
var binLogger = NullLogger<BinanceSymbolProvider>.Instance;
var binance = new BinanceSymbolProvider(binHttpClient, binOptions, binLogger);

try
{
    Console.WriteLine($"Query: {volumeQuery.GetSummary()}");
    var binResults = await binance.GetTopSymbolsAsync(volumeQuery);
    Console.WriteLine($"Binance returned {binResults.Count} symbols:");
    foreach (var s in binResults)
    {
        Console.WriteLine($"  {s.Symbol}: Vol=${s.Volume24hUsd:N0} (no market cap from Binance)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Binance error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine();

// Test SymbolCollectionService with CoinLore + Binance
Console.WriteLine("--- Collection Service (using CoinLore for market cap) ---");
var providers = new ISymbolDataProvider[] { coinLore, binance };
var serviceLogger = NullLogger<SymbolCollectionService>.Instance;
var service = new SymbolCollectionService(providers, null, serviceLogger);

try
{
    var snapshot = await service.CreateSnapshotAsync(marketCapQuery, "Top 10 by Market Cap");
    Console.WriteLine($"Created snapshot: {snapshot.Name}");
    Console.WriteLine($"  Active: {snapshot.ActiveCount}, Pending: {snapshot.PendingCount}");
    Console.WriteLine($"  Provider: {snapshot.ProviderUsed}");
    Console.WriteLine($"  Symbols: {string.Join(", ", snapshot.ActiveSymbols.Take(5).Select(s => s.Symbol))}...");
}
catch (Exception ex)
{
    Console.WriteLine($"Service error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine();

// Optional: Test CoinGecko (often rate-limited)
Console.WriteLine("--- CoinGecko Provider (often rate-limited) ---");
using var cgHttpClient = new HttpClient();
var cgOptions = Options.Create(new CoinGeckoProviderOptions());
var cgLogger = NullLogger<CoinGeckoSymbolProvider>.Instance;
var coinGecko = new CoinGeckoSymbolProvider(cgHttpClient, cgOptions, cgLogger);

try
{
    var cgResults = await coinGecko.GetTopSymbolsAsync(marketCapQuery);
    Console.WriteLine($"CoinGecko returned {cgResults.Count} symbols:");
    foreach (var s in cgResults.Take(5))
    {
        Console.WriteLine($"  {s.Symbol}: MCap=${s.MarketCapUsd:N0}, Rank#{s.MarketCapRank}");
    }
    if (cgResults.Count == 0)
    {
        Console.WriteLine("  (CoinGecko may be rate-limited - use CoinLore instead)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"CoinGecko error: {ex.Message}");
}

Console.WriteLine("\n=== Test Complete ===");
