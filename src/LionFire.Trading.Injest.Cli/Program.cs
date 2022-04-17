using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Hosting;
using LionFire.Trading.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using Oakton;
using Winton.Extensions.Configuration.Consul;
using Winton.Extensions.Configuration.Consul.Parsers;
using Microsoft.Extensions.Configuration;

return await Host.CreateDefaultBuilder()
    .LionFire()
    .ConfigureHostConfiguration(c =>
        c.AddConsul("LionFire.Trading", options => options.Parser = new SimpleConfigurationParser())
#if DEBUG
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["LionFire.Trading:HistoricalData:Windows:BaseDir"] = @"F:\st\Investing-HistoricalData", // HARDCODE
        })
#endif
        )
        
    .ConfigureServices((context, services) =>
    {
        services
            .Configure<HistoricalDataPaths>(context.Configuration.GetSection("HistoricalData").GetSection(OperatingSystem.IsWindows() ? "Windows" : "Unix"))
            .AddSingleton<BinanceClientProvider>()
            .AddSingleton<KlineArrayFileProvider>()
        ;
    })    
    .RunOaktonCommands(args);
    ;

/* TODO
 * 
 *  - retrieve only if not available locally
 *  - verify
 *  - verify: log verification status somehow
 * 
 * Soon
 *  - retrieve only missing parts
 *  
 * DEFER
 *  - support 2nd exchange
 *  - smaller time range files until current period, then roll up into large file
 *  - rollups: 1m > 5m or 1m > 15m etc.
 * 
 */ 