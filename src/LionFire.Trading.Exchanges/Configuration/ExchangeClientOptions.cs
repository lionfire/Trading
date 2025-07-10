using System;

namespace LionFire.Trading.Exchanges.Configuration;

public abstract class ExchangeClientOptions
{
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public bool UseTestnet { get; set; } = false;
    public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxReconnectAttempts { get; set; } = 10;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class BinanceClientOptions : ExchangeClientOptions
{
    public string BaseAddress { get; set; } = "https://api.binance.com";
    public string WebSocketAddress { get; set; } = "wss://stream.binance.com:9443";
    public string TestnetBaseAddress { get; set; } = "https://testnet.binance.vision";
    public string TestnetWebSocketAddress { get; set; } = "wss://testnet.binance.vision";
}

public class BybitClientOptions : ExchangeClientOptions
{
    public string BaseAddress { get; set; } = "https://api.bybit.com";
    public string WebSocketAddress { get; set; } = "wss://stream.bybit.com";
    public string TestnetBaseAddress { get; set; } = "https://api-testnet.bybit.com";
    public string TestnetWebSocketAddress { get; set; } = "wss://stream-testnet.bybit.com";
}

public class MexcClientOptions : ExchangeClientOptions
{
    public string BaseAddress { get; set; } = "https://api.mexc.com";
    public string WebSocketAddress { get; set; } = "wss://wbs.mexc.com";
}