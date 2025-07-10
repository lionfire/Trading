# Bybit Feed Collector

Real-time market data collector for Bybit perpetual futures, tracking trades, CVD (Cumulative Volume Delta), and order book depth.

## Features

- Real-time trade data collection with CVD tracking
- Order book depth monitoring at multiple price levels (0.1%, 0.25%, 0.5%, 0.75%, 1%, 2%)
- FasterLog-based time-series storage for efficient data access
- Configurable data collection triggers (per trade or per order book update)
- Support for multiple symbols simultaneously

## Configuration

Edit `appsettings.json` to configure:

- **Symbols**: List of perpetual futures symbols to track (e.g., "BTCUSDT", "ETHUSDT")
- **CollectTrades**: Enable/disable trade data collection
- **CollectOrderBook**: Enable/disable order book monitoring
- **CollectOnTradeOnly**: If true, snapshots are only created on trades (not on order book changes)
- **OrderBookDepth**: Configure which depth levels to collect
- **ApiKey/ApiSecret**: Optional Bybit API credentials (not required for public data)
- **UseTestnet**: Use Bybit testnet for development

## Running

```bash
dotnet run
```

Or with specific environment:

```bash
dotnet run --environment Development
```

## Data Storage

Data is stored in the `./data/bybit` directory using FasterLog. Each market data snapshot includes:

- Timestamp
- CVD (Cumulative Volume Delta)
- Last trade information (price, volume, side)
- Current bid/ask prices
- Order book depth at configured percentage levels

## Data Access

Use the `ITimeSeriesStorage` interface to read historical data:

```csharp
var snapshots = await storage.ReadRangeAsync(
    "BTCUSDT", 
    DateTime.UtcNow.AddHours(-1), 
    DateTime.UtcNow);
```