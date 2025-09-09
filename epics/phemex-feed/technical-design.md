# Phemex & MEXC Integration - Technical Design (Using Existing Architecture)

## Architecture Overview - Integration Points

```
                    Existing Architecture                           New Components
    ┌──────────────────────────────────────────────┐    ┌────────────────────────┐
    │         LionFire.Trading.Exchanges           │    │    New Implementations │
    │                                              │    │                        │
    │  IExchangeClient                             │◄───┤  PhemexExchangeClient  │
    │  IExchangeWebSocketClient                    │◄───┤  MexcExchangeClient    │
    │  IExchangeRestClient                         │    └────────────────────────┘
    │                                              │
    │  ExchangeClientFactory                       │
    │  ├─ CreateClient("binance") ✓                │
    │  ├─ CreateClient("bybit") ✓                  │
    │  ├─ CreateClient("phemex") [NEW]             │◄─── Register new exchanges
    │  └─ CreateClient("mexc") [NEW]               │
    └──────────────────────────────────────────────┘
    
    ┌──────────────────────────────────────────────┐    ┌────────────────────────┐
    │          LionFire.Trading.Feeds              │    │    New Collectors      │
    │                                              │    │                        │
    │  FeedCollectorBase                           │◄───┤  PhemexFeedCollector   │
    │  ├─ ProcessTrades()                          │    │  MexcFeedCollector     │
    │  ├─ ProcessOrderBook()                       │    └────────────────────────┘
    │  ├─ CalculateCVD()                           │
    │  └─ CalculateDepths()                        │
    │                                              │
    │  Existing Models:                            │
    │  ├─ MarketDataSnapshot                       │◄─── Reuse existing models
    │  ├─ ExchangeTrade                            │
    │  ├─ ExchangeOrderBook                        │
    │  └─ ExchangeTicker                           │
    └──────────────────────────────────────────────┘
```

## Implementation Plan

### Phase 1: Phemex Exchange Client

#### 1.1 Create PhemexExchangeClient
```csharp
// Location: /LionFire.Trading.Exchanges/Phemex/PhemexExchangeClient.cs

public class PhemexExchangeClient : IExchangeWebSocketClient, IExchangeRestClient
{
    private readonly CcxtClient ccxtClient;
    private readonly ILogger<PhemexExchangeClient> logger;
    private dynamic phemexExchange;
    
    public PhemexExchangeClient(
        IOptions<PhemexExchangeOptions> options,
        ILogger<PhemexExchangeClient> logger)
    {
        this.logger = logger;
        // Initialize CCXT Phemex instance
        ccxtClient = new CcxtClient();
        phemexExchange = ccxtClient.CreateExchange("phemex", new Dictionary<string, object>
        {
            ["apiKey"] = options.Value.ApiKey,
            ["secret"] = options.Value.ApiSecret,
            ["enableRateLimit"] = true
        });
    }
    
    // Implement IExchangeWebSocketClient methods
    public async Task<IExchangeSubscription> SubscribeToTradesAsync(
        string symbol, 
        Action<ExchangeTrade> onData)
    {
        // Use CCXT Pro watchTrades
        var subscription = new PhemexSubscription();
        _ = Task.Run(async () =>
        {
            while (!subscription.IsCancelled)
            {
                var trades = await ccxtClient.WatchTrades(phemexExchange, symbol);
                foreach (var trade in trades)
                {
                    onData(ConvertToExchangeTrade(trade));
                }
            }
        });
        return subscription;
    }
    
    public async Task<IExchangeSubscription> SubscribeToOrderBookAsync(
        string symbol,
        int depth,
        Action<ExchangeOrderBook> onData)
    {
        // Use CCXT Pro watchOrderBook
        // Similar pattern as trades
    }
    
    // Convert CCXT format to existing models
    private ExchangeTrade ConvertToExchangeTrade(dynamic ccxtTrade)
    {
        return new ExchangeTrade
        {
            Symbol = ccxtTrade.symbol,
            Price = Convert.ToDecimal(ccxtTrade.price),
            Quantity = Convert.ToDecimal(ccxtTrade.amount),
            Side = ccxtTrade.side == "buy" ? TradeSide.Buy : TradeSide.Sell,
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ccxtTrade.timestamp),
            TradeId = ccxtTrade.id?.ToString()
        };
    }
}
```

#### 1.2 Register in ExchangeClientFactory
```csharp
// Location: /LionFire.Trading.Exchanges/Services/ExchangeClientFactory.cs

public class ExchangeClientFactory : IExchangeClientFactory
{
    public IExchangeClient CreateClient(string exchange)
    {
        return exchange.ToLowerInvariant() switch
        {
            "binance" => serviceProvider.GetRequiredService<BinanceExchangeClient>(),
            "bybit" => serviceProvider.GetRequiredService<BybitExchangeClient>(),
            "phemex" => serviceProvider.GetRequiredService<PhemexExchangeClient>(),  // NEW
            "mexc" => serviceProvider.GetRequiredService<MexcExchangeClient>(),      // NEW
            _ => throw new NotSupportedException($"Exchange {exchange} not supported")
        };
    }
}
```

### Phase 2: Phemex Feed Collector

#### 2.1 Create PhemexFeedCollector
```csharp
// Location: /LionFire.Trading.Feeds.Phemex/PhemexFeedCollector.cs

public class PhemexFeedCollector : FeedCollectorBase
{
    private readonly IExchangeWebSocketClient exchangeClient;
    private readonly Dictionary<string, IExchangeSubscription> subscriptions;
    
    public PhemexFeedCollector(
        IExchangeClientFactory clientFactory,
        IOptions<PhemexFeedOptions> options,
        ILogger<PhemexFeedCollector> logger) 
        : base(logger, options.Value)
    {
        exchangeClient = clientFactory.CreateClient("phemex") as IExchangeWebSocketClient;
        subscriptions = new Dictionary<string, IExchangeSubscription>();
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        
        foreach (var symbol in Options.Symbols)
        {
            // Subscribe to trades
            var tradeSub = await exchangeClient.SubscribeToTradesAsync(
                symbol,
                trade => ProcessTrade(trade));  // Use base class method
            
            // Subscribe to order book
            var bookSub = await exchangeClient.SubscribeToOrderBookAsync(
                symbol,
                Options.OrderBookDepth,
                book => ProcessOrderBook(book));  // Use base class method
                
            subscriptions[$"{symbol}_trades"] = tradeSub;
            subscriptions[$"{symbol}_book"] = bookSub;
        }
    }
    
    protected override void ProcessTrade(ExchangeTrade trade)
    {
        // Base class handles CVD calculation
        base.ProcessTrade(trade);
        
        // Any Phemex-specific processing
        if (trade.Symbol.EndsWith("PERP"))
        {
            // Handle perpetual-specific logic
        }
    }
    
    protected override MarketDataSnapshot CreateSnapshot(string symbol)
    {
        var snapshot = base.CreateSnapshot(symbol);
        
        // Base class already calculates depths (0.1%, 0.25%, etc.)
        // Add any Phemex-specific fields if needed
        
        return snapshot;
    }
}
```

### Phase 3: Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs

services.Configure<PhemexExchangeOptions>(configuration.GetSection("Exchanges:Phemex"));
services.Configure<MexcExchangeOptions>(configuration.GetSection("Exchanges:Mexc"));

// Register exchange clients
services.AddSingleton<PhemexExchangeClient>();
services.AddSingleton<MexcExchangeClient>();

// Register feed collectors
services.AddHostedService<PhemexFeedCollector>();
services.AddHostedService<MexcFeedCollector>();

// Factory already registered
services.AddSingleton<IExchangeClientFactory, ExchangeClientFactory>();
```

### Phase 4: Configuration

```json
{
  "Exchanges": {
    "Phemex": {
      "ApiKey": "{{PHEMEX_API_KEY}}",
      "ApiSecret": "{{PHEMEX_API_SECRET}}",
      "TestMode": true,
      "Symbols": ["BTC/USDT", "ETH/USDT"],
      "OrderBookDepth": 20,
      "EnableCVD": true,
      "CalculateDepths": [0.001, 0.0025, 0.005, 0.01]
    },
    "Mexc": {
      "ApiKey": "{{MEXC_API_KEY}}",
      "ApiSecret": "{{MEXC_API_SECRET}}",
      "TestMode": true,
      "Symbols": ["BTC/USDT", "ETH/USDT"],
      "OrderBookDepth": 20
    }
  }
}
```

## Integration with Existing Components

### Using Existing Models

```csharp
// All these models are already defined and will be reused:

public class ExchangeTrade
{
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public TradeSide Side { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string TradeId { get; set; }
}

public class ExchangeOrderBook
{
    public string Symbol { get; set; }
    public List<PriceLevel> Bids { get; set; }
    public List<PriceLevel> Asks { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class MarketDataSnapshot
{
    public string Exchange { get; set; }
    public string Symbol { get; set; }
    public decimal LastPrice { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal CVD { get; set; }  // Calculated by base class
    public OrderBookDepth Depths { get; set; }  // Calculated by base class
    // ... other existing fields
}
```

### Leveraging Base Class Functionality

The `FeedCollectorBase` provides:
- **CVD Calculation**: Automatic cumulative volume delta tracking
- **Order Book Depth**: Multi-level depth calculations (0.1%, 0.25%, etc.)
- **Snapshot Management**: Periodic snapshot creation
- **Event Publishing**: Built-in event system for data distribution
- **Metrics**: Performance and data quality metrics

### NATS Integration (Optional - Proprietary Pattern)

If using the proprietary pattern with NATS:

```csharp
public class PhemexNatsFeeder : PhemexFeedCollector
{
    private readonly INatsConnection natsConnection;
    
    protected override async Task PublishSnapshot(MarketDataSnapshot snapshot)
    {
        await base.PublishSnapshot(snapshot);
        
        // Publish to NATS
        var subject = $"market.{snapshot.Exchange}.{snapshot.Symbol}";
        await natsConnection.PublishAsync(subject, snapshot);
    }
}
```

## Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class PhemexExchangeClientTests
{
    [TestMethod]
    public async Task SubscribeToTrades_Should_ConvertCcxtFormat()
    {
        // Arrange
        var mockCcxt = new Mock<ICcxtClient>();
        var client = new PhemexExchangeClient(mockCcxt.Object);
        
        // Act
        var trades = new List<ExchangeTrade>();
        await client.SubscribeToTradesAsync("BTC/USDT", t => trades.Add(t));
        
        // Assert
        Assert.IsTrue(trades.All(t => t.Symbol == "BTC/USDT"));
    }
}
```

### Integration Tests
```csharp
[TestClass]
[TestCategory("Integration")]
public class PhemexFeedCollectorIntegrationTests
{
    [TestMethod]
    public async Task Collector_Should_InheritBaseCalculations()
    {
        // Test that CVD and depth calculations work
        var collector = new PhemexFeedCollector(/* dependencies */);
        await collector.StartAsync(CancellationToken.None);
        
        // Wait for data
        await Task.Delay(5000);
        
        var snapshot = collector.GetLatestSnapshot("BTC/USDT");
        
        // Verify base class calculations
        Assert.IsNotNull(snapshot.CVD);
        Assert.IsNotNull(snapshot.Depths);
        Assert.IsTrue(snapshot.Depths.Depth_0_1_Percent > 0);
    }
}
```

## Migration Path

1. **No Breaking Changes**: Existing Binance/Bybit implementations continue working
2. **Gradual Rollout**: Deploy Phemex first, then MEXC
3. **Feature Flags**: Use configuration to enable/disable exchanges
4. **Monitoring**: Use existing metrics infrastructure

## Benefits of This Approach

1. **Reuse Existing Code**: Leverage proven abstractions and base classes
2. **Consistent Behavior**: All exchanges follow same patterns
3. **Reduced Development Time**: No need to create new frameworks
4. **Unified Data Model**: `MarketDataSnapshot` works across all exchanges
5. **Built-in Features**: CVD, depth calculations come for free
6. **Tested Infrastructure**: Base classes are already production-tested