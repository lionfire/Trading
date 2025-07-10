using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Exchanges.Abstractions;

public interface IExchangeClient : IDisposable
{
    string ExchangeName { get; }
    bool IsConnected { get; }
    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}

public interface IExchangeWebSocketClient : IExchangeClient
{
    event EventHandler<ExchangeConnectionStateChangedEventArgs>? ConnectionStateChanged;
    
    Task<IExchangeSubscription> SubscribeToTradesAsync(
        string symbol,
        Action<ExchangeTrade> onData,
        CancellationToken cancellationToken = default);
    
    Task<IExchangeSubscription> SubscribeToOrderBookAsync(
        string symbol,
        int depth,
        Action<ExchangeOrderBook> onData,
        CancellationToken cancellationToken = default);
    
    Task<IExchangeSubscription> SubscribeToTickerAsync(
        string symbol,
        Action<ExchangeTicker> onData,
        CancellationToken cancellationToken = default);
}

public interface IExchangeRestClient : IExchangeClient
{
    Task<ExchangeSymbolInfo[]> GetSymbolsAsync(CancellationToken cancellationToken = default);
    Task<ExchangeOrderBook> GetOrderBookAsync(string symbol, int depth = 20, CancellationToken cancellationToken = default);
    Task<ExchangeTrade[]> GetRecentTradesAsync(string symbol, int limit = 100, CancellationToken cancellationToken = default);
    Task<ExchangeTicker> GetTickerAsync(string symbol, CancellationToken cancellationToken = default);
}

public interface IExchangeSubscription : IDisposable
{
    string Id { get; }
    bool IsActive { get; }
    Task CloseAsync();
}

public class ExchangeConnectionStateChangedEventArgs : EventArgs
{
    public bool IsConnected { get; init; }
    public string? DisconnectReason { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}