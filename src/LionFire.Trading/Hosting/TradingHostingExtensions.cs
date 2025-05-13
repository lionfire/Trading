using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Trading;
using Microsoft.Extensions.Configuration;
using LionFire.Trading.Exchanges;
using LionFire.Trading.Automation;


namespace LionFire.Hosting;

public static class TradingHostingExtensions
{
    public static ILionFireHostBuilder Trading(this ILionFireHostBuilder builder)
        => builder.ForHostBuilder(b => b.ConfigureServices(services => services
            .Accounts() // REVIEW: not needed for all Trading apps, so consider moving this out.  (Needed for backtesting (maybe), and bots, and trading clients.) 
            .AddSingleton<SymbolNameNormalizer>()
            .AddVirtualFilesystem() // REVIEW: What is this needed for?
            ));

    public static IServiceCollection Accounts(this IServiceCollection services)
        => services
            .AddSingleton<IAccountProvider, AccountProvider>()
            ;

 
}
