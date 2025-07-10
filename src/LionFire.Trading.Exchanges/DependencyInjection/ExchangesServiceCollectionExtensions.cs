using LionFire.Trading.Exchanges.Configuration;
using LionFire.Trading.Exchanges.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Exchanges.DependencyInjection;

public static class ExchangesServiceCollectionExtensions
{
    public static IServiceCollection AddTradingExchanges(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add exchange client factory
        services.AddSingleton<IExchangeClientFactory, ExchangeClientFactory>();
        
        // Configure exchange options
        services.Configure<BinanceClientOptions>(
            configuration.GetSection("Exchanges:Binance"));
        services.Configure<BybitClientOptions>(
            configuration.GetSection("Exchanges:Bybit"));
        services.Configure<MexcClientOptions>(
            configuration.GetSection("Exchanges:MEXC"));
        
        return services;
    }
}