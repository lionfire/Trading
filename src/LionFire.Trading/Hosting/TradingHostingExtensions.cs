using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Trading;
using Microsoft.Extensions.Configuration;


namespace LionFire.Hosting;

public static class TradingHostingExtensions
{
    public static ILionFireHostBuilder Trading(this ILionFireHostBuilder builder)
        => builder.ForHostBuilder(b => b.ConfigureServices(services => services
            .AddSingleton<IAccountProvider, AccountProvider>()
            .AddSingleton<SymbolNameNormalizer>()
            .AddVirtualFilesystem()
            ));

    public static IServiceCollection Backtesting(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<BacktestOptions>(configuration.GetSection("LionFire:Trading:Backtesting"))
            ;
    public static ILionFireHostBuilder Backtesting(this ILionFireHostBuilder builder)
        => builder.ForHostBuilder(b => b.ConfigureServices(services => services
            .Backtesting(builder.Configuration)
            ));
}
