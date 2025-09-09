# Exchange Abstraction Layer Design

## Overview
This document details the abstraction layer that enables seamless integration of multiple cryptocurrency exchanges using CCXT as the underlying library.

## Design Principles

### 1. Single Responsibility
Each exchange adapter handles only exchange-specific quirks while delegating common functionality to the base CCXT layer.

### 2. Open/Closed Principle
The system is open for extension (new exchanges) but closed for modification (existing code remains stable).

### 3. Dependency Inversion
High-level trading logic depends on abstractions (IExchangeFeedClient), not concrete implementations.

## Key Benefits of Using CCXT

1. **Unified API**: Single interface for 100+ exchanges
2. **WebSocket Support**: CCXT Pro provides real-time data streams
3. **Battle-tested**: Widely used library with active maintenance
4. **Error Handling**: Built-in retry logic and error normalization
5. **Rate Limiting**: Automatic rate limit management

## Adding a New Exchange

### Step 1: Create Exchange Adapter
```csharp
public class NewExchangeFeedClient : CcxtExchangeFeedClient
{
    public override string ExchangeName => "newexchange";
    
    // Override methods only if exchange has special requirements
    protected override async Task HandleSpecialCase()
    {
        // Exchange-specific logic
    }
}
```

### Step 2: Register in Factory
```csharp
private IExchangeFeedClient CreateExchangeClient(string exchange)
{
    return exchange.ToLower() switch
    {
        "phemex" => new PhemexFeedClient(ccxtClient, logger),
        "mexc" => new MexcFeedClient(ccxtClient, logger),
        "newexchange" => new NewExchangeFeedClient(ccxtClient, logger), // Add here
        _ => throw new NotSupportedException($"Exchange {exchange} not supported")
    };
}
```

### Step 3: Configure Exchange
```json
{
  "Exchanges": {
    "NewExchange": {
      "Enabled": true,
      "ApiKey": "{{NEWEXCHANGE_API_KEY}}",
      "ApiSecret": "{{NEWEXCHANGE_API_SECRET}}",
      "TestMode": true,
      "RateLimit": 10,
      "Symbols": ["BTC/USDT", "ETH/USDT"]
    }
  }
}
```

## Data Flow Through Abstraction Layer

```
1. User Request → MultiExchangeFeedManager
   └─> "Subscribe to BTC/USDT on Phemex"

2. Manager → Exchange Resolution
   └─> GetOrCreateClient("phemex")

3. Client Creation → CcxtClient
   └─> ccxt.phemex({ apiKey, apiSecret })

4. Subscription → CCXT WebSocket
   └─> exchange.watchTrades("BTC/USDT")

5. Data Reception → Transformation
   └─> CCXT Format → Unified Format

6. Event Publishing → Consumers
   └─> OnDataReceived(UnifiedTrade)
```

## Handling Exchange Differences

### Symbol Formatting
Different exchanges use different symbol formats:
- Phemex: "BTCUSDT" (no separator)
- Binance: "BTC/USDT" (slash separator)
- MEXC: "BTC_USDT" (underscore separator)

CCXT handles this automatically, providing unified "BTC/USDT" format.

### Data Field Variations
```csharp
public class ExchangeDataNormalizer
{
    public UnifiedTrade NormalizeTrade(dynamic ccxtTrade, string exchange)
    {
        return new UnifiedTrade
        {
            Exchange = exchange,
            Symbol = ccxtTrade.symbol,
            Price = Convert.ToDecimal(ccxtTrade.price),
            Quantity = Convert.ToDecimal(ccxtTrade.amount),
            Side = ccxtTrade.side,
            TradeId = ccxtTrade.id?.ToString(),
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(
                ccxtTrade.timestamp).DateTime
        };
    }
}
```

### Authentication Methods
```csharp
public class ExchangeAuthenticator
{
    public Dictionary<string, object> GetAuthConfig(string exchange, IConfiguration config)
    {
        var authConfig = new Dictionary<string, object>
        {
            ["apiKey"] = config[$"Exchanges:{exchange}:ApiKey"],
            ["secret"] = config[$"Exchanges:{exchange}:ApiSecret"]
        };
        
        // Some exchanges need additional auth params
        if (exchange == "phemex")
        {
            authConfig["password"] = config[$"Exchanges:{exchange}:Password"];
        }
        
        return authConfig;
    }
}
```

## Error Handling Strategy

### Exchange-Specific Errors
```csharp
public async Task<T> HandleExchangeOperation<T>(Func<Task<T>> operation, string exchange)
{
    try
    {
        return await operation();
    }
    catch (CcxtException ex) when (ex.ErrorCode == "RateLimitExceeded")
    {
        // Apply exchange-specific backoff
        var backoff = GetExchangeBackoff(exchange);
        await Task.Delay(backoff);
        return await operation(); // Retry once
    }
    catch (CcxtException ex) when (ex.ErrorCode == "ExchangeNotAvailable")
    {
        // Switch to backup exchange if configured
        return await HandleFailover(operation, exchange);
    }
}
```

### Failover Support
```csharp
public class ExchangeFailoverManager
{
    private readonly Dictionary<string, List<string>> failoverChains = new()
    {
        ["phemex"] = new() { "mexc", "binance" },
        ["mexc"] = new() { "phemex", "binance" },
        ["binance"] = new() { "phemex", "mexc" }
    };
    
    public async Task<string> GetAvailableExchange(string preferredExchange)
    {
        if (await IsExchangeHealthy(preferredExchange))
            return preferredExchange;
            
        foreach (var backup in failoverChains[preferredExchange])
        {
            if (await IsExchangeHealthy(backup))
                return backup;
        }
        
        throw new NoAvailableExchangeException();
    }
}
```

## Performance Optimization

### Connection Pooling
```csharp
public class ExchangeConnectionPool
{
    private readonly Dictionary<string, Queue<IExchangeFeedClient>> pools;
    private readonly int maxConnectionsPerExchange = 5;
    
    public async Task<IExchangeFeedClient> GetConnection(string exchange)
    {
        if (pools[exchange].Count > 0)
        {
            return pools[exchange].Dequeue();
        }
        
        if (GetActiveConnections(exchange) < maxConnectionsPerExchange)
        {
            return await CreateNewConnection(exchange);
        }
        
        // Wait for available connection
        return await WaitForAvailableConnection(exchange);
    }
}
```

### Message Batching
```csharp
public class MessageBatcher
{
    private readonly Dictionary<string, List<UnifiedMarketData>> buffers;
    private readonly int batchSize = 100;
    private readonly TimeSpan batchTimeout = TimeSpan.FromMilliseconds(100);
    
    public async Task ProcessMessage(UnifiedMarketData data)
    {
        var key = $"{data.Exchange}:{data.Symbol}:{data.DataType}";
        buffers[key].Add(data);
        
        if (buffers[key].Count >= batchSize || 
            DateTime.UtcNow - lastFlush[key] > batchTimeout)
        {
            await FlushBatch(key);
        }
    }
}
```

## Monitoring & Metrics

### Key Metrics to Track
```csharp
public class ExchangeMetrics
{
    // Per-exchange metrics
    public Counter MessagesReceived { get; }
    public Histogram MessageLatency { get; }
    public Gauge ActiveConnections { get; }
    public Counter ReconnectionAttempts { get; }
    public Counter Errors { get; }
    
    // Aggregated metrics
    public Gauge TotalDataRate { get; }
    public Histogram CrossExchangeLatencyDiff { get; }
}
```

### Health Checks
```csharp
public class ExchangeHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        var unhealthyExchanges = new List<string>();
        
        foreach (var exchange in configuredExchanges)
        {
            var client = await manager.GetOrCreateClient(exchange);
            if (client.ConnectionState != ConnectionState.Connected)
            {
                unhealthyExchanges.Add(exchange);
            }
        }
        
        if (unhealthyExchanges.Any())
        {
            return HealthCheckResult.Degraded(
                $"Exchanges disconnected: {string.Join(", ", unhealthyExchanges)}");
        }
        
        return HealthCheckResult.Healthy("All exchanges connected");
    }
}
```

## Testing Strategy

### Unit Testing with Mocks
```csharp
[TestClass]
public class ExchangeAdapterTests
{
    private Mock<ICcxtClient> mockCcxt;
    private PhemexFeedClient client;
    
    [TestMethod]
    public async Task Subscribe_Should_CallCcxtWatchMethod()
    {
        // Arrange
        mockCcxt.Setup(x => x.WatchTrades(It.IsAny<dynamic>(), "BTC/USDT"))
                .ReturnsAsync(CreateMockTradeData());
        
        // Act
        await client.SubscribeAsync("trades", "BTC/USDT");
        
        // Assert
        mockCcxt.Verify(x => x.WatchTrades(It.IsAny<dynamic>(), "BTC/USDT"), Times.Once);
    }
}
```

### Integration Testing
```csharp
[TestClass]
[TestCategory("Integration")]
public class MultiExchangeIntegrationTests
{
    [TestMethod]
    public async Task Should_ReceiveData_FromMultipleExchanges()
    {
        // Connect to testnet/sandbox environments
        var manager = new MultiExchangeFeedManager(config);
        var receivedData = new ConcurrentBag<UnifiedMarketData>();
        
        // Subscribe to multiple exchanges
        await manager.AddSubscription("phemex", new SubscriptionRequest("BTC/USDT", "trades"));
        await manager.AddSubscription("mexc", new SubscriptionRequest("BTC/USDT", "trades"));
        
        // Collect data for 10 seconds
        await Task.Delay(10000);
        
        // Verify data from both exchanges
        Assert.IsTrue(receivedData.Any(d => d.Exchange == "phemex"));
        Assert.IsTrue(receivedData.Any(d => d.Exchange == "mexc"));
    }
}
```