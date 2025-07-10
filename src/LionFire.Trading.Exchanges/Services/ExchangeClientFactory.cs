using System;
using LionFire.Trading.Exchanges.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Exchanges.Services;

public interface IExchangeClientFactory
{
    IExchangeWebSocketClient CreateWebSocketClient(string exchangeName);
    IExchangeRestClient CreateRestClient(string exchangeName);
}

public class ExchangeClientFactory : IExchangeClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExchangeClientFactory> _logger;

    public ExchangeClientFactory(
        IServiceProvider serviceProvider,
        ILogger<ExchangeClientFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IExchangeWebSocketClient CreateWebSocketClient(string exchangeName)
    {
        var normalizedName = exchangeName.ToUpperInvariant();
        
        return normalizedName switch
        {
            "BINANCE" => _serviceProvider.GetService<IExchangeWebSocketClient>() 
                ?? throw new NotSupportedException($"Binance WebSocket client not registered"),
            "BYBIT" => _serviceProvider.GetService<IExchangeWebSocketClient>() 
                ?? throw new NotSupportedException($"Bybit WebSocket client not registered"),
            "MEXC" => throw new NotImplementedException("MEXC WebSocket client not yet implemented"),
            _ => throw new NotSupportedException($"Exchange '{exchangeName}' is not supported")
        };
    }

    public IExchangeRestClient CreateRestClient(string exchangeName)
    {
        var normalizedName = exchangeName.ToUpperInvariant();
        
        return normalizedName switch
        {
            "BINANCE" => _serviceProvider.GetService<IExchangeRestClient>() 
                ?? throw new NotSupportedException($"Binance REST client not registered"),
            "BYBIT" => _serviceProvider.GetService<IExchangeRestClient>() 
                ?? throw new NotSupportedException($"Bybit REST client not registered"),
            "MEXC" => throw new NotImplementedException("MEXC REST client not yet implemented"),
            _ => throw new NotSupportedException($"Exchange '{exchangeName}' is not supported")
        };
    }
}