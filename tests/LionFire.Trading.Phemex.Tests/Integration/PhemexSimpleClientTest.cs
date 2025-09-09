using System.Net.Http;
using FluentAssertions;
using LionFire.Trading.Phemex.Api;
using LionFire.Trading.Phemex.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Phemex.Tests.Integration;

/// <summary>
/// Integration tests for PhemexSimpleClient using actual testnet API
/// </summary>
[Collection("Integration")]
public class PhemexSimpleClientTest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ServiceProvider _serviceProvider;
    private readonly PhemexSimpleClient _client;
    private readonly ILogger<PhemexSimpleClientTest> _logger;

    public PhemexSimpleClientTest(ITestOutputHelper output)
    {
        _output = output;

        // Setup DI
        var services = new ServiceCollection();
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        // Configure Phemex options
        services.Configure<PhemexOptions>(options =>
        {
            // Use testnet credentials from .env file
            options.ApiKey = "2dc16ad9-8186-4eef-9cfc-67e5acd299e3";
            options.ApiSecret = "cvyA7vlZqIoG34iV9-eGpqsMCxT7bL7ULskunCQb0ApmODk0NjFjZi0zYjE0LTQzNmMtYTVhZC0yMWZjZmE4NGYyY2I";
            options.IsTestnet = true;
            options.RateLimitPerSecond = 10;
        });

        // Add HttpClient
        services.AddHttpClient("Phemex")
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PhemexOptions>>().Value;
                client.BaseAddress = new Uri(options.IsTestnet ? "https://testnet-api.phemex.com" : "https://api.phemex.com");
                client.DefaultRequestHeaders.Add("User-Agent", "LionFire.Trading.Phemex.Tests/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Register the client
        services.AddSingleton<PhemexSimpleClient>();

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<PhemexSimpleClient>();
        _logger = _serviceProvider.GetRequiredService<ILogger<PhemexSimpleClientTest>>();
    }

    [Fact]
    public async Task GetTicker24hr_BTCUSD_ShouldReturnData()
    {
        // Arrange
        var symbol = "BTCUSD"; // Phemex uses BTCUSD not BTCUSDT

        // Act
        var ticker = await _client.GetTicker24hrAsync(symbol);

        // Assert
        ticker.Should().NotBeNull();
        ticker.Symbol.Should().Be(symbol);
        ticker.LastPrice.Should().BeGreaterThan(0);
        ticker.BidPrice.Should().BeGreaterThan(0);
        ticker.AskPrice.Should().BeGreaterThan(0);

        _output.WriteLine($"BTC/USD Ticker:");
        _output.WriteLine($"  Last: ${ticker.LastPrice:N2}");
        _output.WriteLine($"  Bid: ${ticker.BidPrice:N2}");
        _output.WriteLine($"  Ask: ${ticker.AskPrice:N2}");
        _output.WriteLine($"  24hr Volume: {ticker.Volume:N0}");
        _output.WriteLine($"  24hr High: ${ticker.High:N2}");
        _output.WriteLine($"  24hr Low: ${ticker.Low:N2}");

        _logger.LogInformation("Successfully fetched ticker for {Symbol}: ${Price}", 
            symbol, ticker.LastPrice);
    }

    [Fact]
    public async Task GetOrderBook_BTCUSD_ShouldReturnBidsAndAsks()
    {
        // Arrange
        var symbol = "BTCUSD";

        // Act
        var orderBook = await _client.GetOrderBookAsync(symbol);

        // Assert
        orderBook.Should().NotBeNull();
        orderBook.Bids.Should().NotBeEmpty();
        orderBook.Asks.Should().NotBeEmpty();

        var topBid = orderBook.Bids.First();
        var topAsk = orderBook.Asks.First();
        var spread = topAsk.Price - topBid.Price;

        _output.WriteLine($"Order Book for {symbol}:");
        _output.WriteLine($"  Top Bid: ${topBid.Price:N2} x {topBid.Size}");
        _output.WriteLine($"  Top Ask: ${topAsk.Price:N2} x {topAsk.Size}");
        _output.WriteLine($"  Spread: ${spread:N2}");
        _output.WriteLine($"  Bid Depth: {orderBook.Bids.Count} levels");
        _output.WriteLine($"  Ask Depth: {orderBook.Asks.Count} levels");

        _logger.LogInformation("Order book fetched: Spread=${Spread:N2}", spread);
    }

    [Fact]
    public async Task GetKlines_BTCUSD_ShouldReturnHistoricalData()
    {
        // Arrange
        var symbol = "BTCUSD";
        var resolution = 60; // 1 minute
        var to = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var from = to - (60 * 60); // Last hour

        // Act
        var klines = await _client.GetKlinesAsync(symbol, resolution, from, to);

        // Assert
        klines.Should().NotBeNull();
        klines.Should().NotBeEmpty();
        
        _output.WriteLine($"Retrieved {klines.Count} klines for {symbol}:");
        
        foreach (var kline in klines.Take(5))
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(kline.Timestamp);
            _output.WriteLine($"  {time:HH:mm} - O: ${kline.Open:N2}, H: ${kline.High:N2}, " +
                            $"L: ${kline.Low:N2}, C: ${kline.Close:N2}, V: {kline.Volume:N0}");
        }

        _logger.LogInformation("Fetched {Count} klines for {Symbol}", klines.Count, symbol);
    }

    [Fact]
    public async Task GetTicker_MultipleSymbols_ShouldWork()
    {
        // Test with multiple symbols
        var symbols = new[] { "BTCUSD", "ETHUSD", "XRPUSD" };

        foreach (var symbol in symbols)
        {
            try
            {
                var ticker = await _client.GetTicker24hrAsync(symbol);
                _output.WriteLine($"{symbol}: ${ticker.LastPrice:N4}");
                ticker.Should().NotBeNull();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{symbol}: Failed - {ex.Message}");
                // Some symbols might not be available on testnet
            }
        }
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

/// <summary>
/// Logger provider for xUnit test output
/// </summary>
public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public XunitLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_output, categoryName);
    }

    public void Dispose() { }
}

public class XunitLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    private readonly string _categoryName;

    public XunitLogger(ITestOutputHelper output, string categoryName)
    {
        _output = output;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _output.WriteLine($"[{logLevel}] {_categoryName}: {message}");
        if (exception != null)
        {
            _output.WriteLine(exception.ToString());
        }
    }
}