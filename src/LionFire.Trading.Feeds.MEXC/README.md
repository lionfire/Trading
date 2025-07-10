# MEXC Feed Collector (Stub)

This is a minimal stub implementation for MEXC perpetual futures data collection. It provides the basic structure for future development but does not currently collect real data.

## Status

⚠️ **NOT IMPLEMENTED** - This is a placeholder for future MEXC integration.

## TODO

To complete the MEXC implementation:

1. Add MEXC.Net NuGet package (or implement WebSocket client)
2. Implement trade data subscription
3. Implement order book subscription
4. Add authentication support
5. Test with MEXC testnet

## Configuration

The configuration structure is ready in `appsettings.json`, matching the pattern used by Binance and Bybit collectors:

- Symbols to track
- Trade and order book collection settings
- API credentials (when implemented)
- Testnet support

## Running

```bash
dotnet run
```

Currently, this will start the service but log a warning that MEXC is not yet implemented.