# LionFire.Trading.Exchanges

Shared library for multi-exchange trading client implementations. This library provides common abstractions and utilities for integrating with various cryptocurrency exchanges.

## Features

- Common exchange client interfaces (`IExchangeWebSocketClient`, `IExchangeRestClient`)
- Unified data models for trades, order books, and tickers
- Exchange client factory for creating exchange-specific clients
- Configuration options for multiple exchanges
- Extensible architecture for adding new exchanges

## Supported Exchanges

- **Binance** - Full support via Binance.Net
- **Bybit** - Full support via Bybit.Net
- **MEXC** - Planned (stub implementation)

## Usage

### Configuration

Add exchange configuration to your `appsettings.json`:

```json
{
  "Exchanges": {
    "Binance": {
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret",
      "UseTestnet": false
    },
    "Bybit": {
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret",
      "UseTestnet": false
    }
  }
}
```

### Dependency Injection

```csharp
services.AddTradingExchanges(configuration);
```

### Using the Factory

```csharp
public class MyService
{
    private readonly IExchangeClientFactory _factory;
    
    public MyService(IExchangeClientFactory factory)
    {
        _factory = factory;
    }
    
    public async Task SubscribeToTradesAsync()
    {
        var client = _factory.CreateWebSocketClient("Binance");
        await client.ConnectAsync();
        
        var subscription = await client.SubscribeToTradesAsync(
            "BTCUSDT",
            trade => Console.WriteLine($"Trade: {trade.Price} @ {trade.Quantity}"));
    }
}
```

## Extending

To add a new exchange:

1. Create exchange-specific client implementations of `IExchangeWebSocketClient` and `IExchangeRestClient`
2. Add configuration options extending `ExchangeClientOptions`
3. Update the `ExchangeClientFactory` to support the new exchange
4. Register the implementations in DI

## Data Models

- **ExchangeTrade** - Represents a single trade execution
- **ExchangeOrderBook** - Represents order book snapshot with bids and asks
- **ExchangeTicker** - Represents current market ticker data
- **ExchangeSymbolInfo** - Represents trading pair information