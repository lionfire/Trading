# LionFire Trading Feeds

Real-time market data collection system for cryptocurrency perpetual futures trading, with CVD (Cumulative Volume Delta) tracking and order book depth monitoring.

## Project Structure

```
LionFire.Trading.Feeds/           - Core abstractions and FasterLog storage
LionFire.Trading.Feeds.Binance/   - Binance futures feed collector
LionFire.Trading.Feeds.Bybit/     - Bybit futures feed collector  
LionFire.Trading.Feeds.MEXC/      - MEXC stub (future implementation)
LionFire.Trading.Exchanges/       - Shared exchange client abstractions
```

## Key Features

### 1. CVD (Cumulative Volume Delta) Tracking
- Real-time calculation of buy/sell volume imbalance
- Tracks net buying/selling pressure for each symbol
- Essential for order flow analysis

### 2. Order Book Depth Monitoring
- Captures liquidity at multiple price levels:
  - 0.1%, 0.25%, 0.5%, 0.75%, 1%, 2% from mid price
- Tracks bid/ask volumes and prices at each level
- Updates on trade or order book change

### 3. FasterLog Time-Series Storage
- High-performance append-only log storage
- Efficient for sequential writes and time-range queries
- Configurable segment sizes and memory allocation

### 4. Multi-Exchange Support
- Unified data model across exchanges
- Exchange-specific collectors with common base class
- Easy to add new exchanges

## Data Model

Each market data snapshot includes:
- **Timestamp** - When the snapshot was taken
- **CVD** - Current cumulative volume delta
- **Trade Data** - Last trade price, volume, and side
- **Bid/Ask** - Current best bid and ask prices
- **Order Book Depth** - Liquidity at configured levels
- **Trigger** - What caused the snapshot (trade, order book change, timer)

## Building and Running

### Build the solution:
```bash
dotnet build LionFire.Trading.Feeds.sln
```

### Run individual collectors:
```bash
# Binance
dotnet run --project LionFire.Trading.Feeds.Binance

# Bybit
dotnet run --project LionFire.Trading.Feeds.Bybit
```

## Configuration

Each collector has its own `appsettings.json`:

```json
{
  "BinanceFeed": {
    "Symbols": ["BTCUSDT", "ETHUSDT"],
    "CollectTrades": true,
    "CollectOrderBook": true,
    "CollectOnTradeOnly": true,
    "OrderBookDepth": {
      "Collect01Percent": true,
      "Collect025Percent": true,
      // ... other levels
    }
  }
}
```

## Implementation Notes

### Binance Collector
- Uses Binance.Net library
- Supports USDT perpetual futures
- WebSocket connections for real-time data
- Note: API method signatures may need updating for latest Binance.Net version

### Bybit Collector
- Uses Bybit.Net library
- Supports linear perpetual contracts
- Individual subscriptions per symbol

### MEXC Collector
- Stub implementation for future development
- Structure ready for integration

## Data Access

Use `ITimeSeriesStorage` to read collected data:

```csharp
var storage = serviceProvider.GetRequiredService<ITimeSeriesStorage>();
var snapshots = await storage.ReadRangeAsync(
    "BTCUSDT",
    DateTime.UtcNow.AddHours(-1),
    DateTime.UtcNow);

foreach (var snapshot in snapshots)
{
    Console.WriteLine($"{snapshot.Timestamp}: CVD={snapshot.CumulativeVolumeDelta}");
}
```

## Next Steps

1. **Update Exchange APIs** - Ensure compatibility with latest exchange library versions
2. **Add Authentication** - Configure API credentials for private data if needed
3. **Implement MEXC** - Complete the MEXC integration
4. **Add Data Analysis** - Build tools to analyze collected CVD and order book data
5. **Backtesting Integration** - Use collected data for strategy backtesting

## Storage Considerations

- Default storage location: `./data/{exchange}/`
- FasterLog creates append-only log files
- Consider disk space for long-running collectors
- Implement data archival/rotation as needed