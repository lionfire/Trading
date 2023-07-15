using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Trading;


namespace LionFire.Hosting;

public static class TradingHostingExtensions
{
    public static ILionFireHostBuilder Trading(this LionFireHostBuilder builder)
        => builder.ForHostBuilder(b => b.ConfigureServices(services => services
            .AddSingleton<IAccountProvider, AccountProvider>()
            .AddSingleton<SymbolNameNormalizer>()
            .AddVirtualFilesystem()
            ));

}
