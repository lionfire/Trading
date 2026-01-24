// TODO - mj-map:///C:\st\Projects\FireLynx\FireLynx%20Dash.mmap#oid={F7CABBAD-DA83-4FF0-8BB7-C4DFB4D2077B}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Hosting;
using LionFire.Trading.Binance_;
using LionFire.Trading.HistoricalData.Serialization;
using Winton.Extensions.Configuration.Consul;
using Winton.Extensions.Configuration.Consul.Parsers;
using Microsoft.Extensions.Configuration;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators;
using LionFire.Trading.Cli.Commands;
using LionFire.Trading.Hosting;
using LionFire.Hosting.CommandLine;
using LionFire.Trading.Hosting.Configuration;

return await new HostApplicationBuilderProgram()
    .DefaultArgs("help") // Show help when no command is specified
    
    .RootCommand(builder => 
    {
        // Configure with .env file support using standard trading configuration
        builder.ConfigureTradingConfiguration(System.Environment.GetCommandLineArgs());
        
        // Add additional configuration sources
        builder.Configuration
            .AddEnvironmentVariables("DOTNET_")
#if DEBUG // TODO: how to best configure this for real?
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Trading:HistoricalData:Windows:BaseDir"] = @"c:\st\Investing-HistoricalData", // HARDCODE
                ["Trading:HistoricalData:Unix:BaseDir"] = @"/st/Investing-HistoricalData", // HARDCODE
            })
#endif
        ;

        // Apply LionFire hosting extensions
        builder.LionFire()
            .ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton<BinanceClientProvider>()
                    .AddSingleton<IndicatorProvider>()
                    .AddHistoricalBars(context.Configuration)
                    .AddIndicators();
                    // Orleans client removed - not needed for Phemex commands
            });
    })
    
    // Register hierarchical commands with exchange areas
    .Command("phemex spot balance", PhemexHandlers.SpotBalance)
    .Command("phemex futures balance", PhemexHandlers.FuturesBalance)
    .Command("phemex coin-futures balance", PhemexHandlers.CoinFuturesBalance)
    .Command("phemex spot positions", PhemexHandlers.SpotPositions)
    .Command("phemex futures positions", PhemexHandlers.FuturesPositions)
    .Command("phemex coin-futures positions", PhemexHandlers.CoinFuturesPositions)
    .Command("phemex subaccounts", PhemexHandlers.Subaccounts)
    .Command("phemex spot ticker", PhemexHandlers.SpotTicker)
    .Command("phemex futures ticker", PhemexHandlers.FuturesTicker)
    .Command("phemex spot place-order", PhemexHandlers.SpotPlaceOrder)
    .Command("phemex futures place-order", PhemexHandlers.FuturesPlaceOrder)
    .Command("phemex spot open", PhemexHandlers.SpotOpen)
    .Command("phemex futures open", PhemexHandlers.FuturesOpen)
    .Command("phemex spot close", PhemexHandlers.SpotClose)
    .Command("phemex futures close", PhemexHandlers.FuturesClose)

    // Optimization commands
    .Command<OptimizeRunOptions>("optimize run", OptimizeRunHandler.Run,
        builderBuilder: OptimizeRunHandler.ConfigureCommand)

    // Backtest commands
    .Command<BacktestRunOptions>("backtest run", BacktestRunHandler.Run,
        builderBuilder: BacktestRunHandler.ConfigureCommand)

    // Crypto market data commands
    .Command<CryptoMcapOptions>("crypto mcap", CryptoHandlers.Mcap,
        builderBuilder: CryptoHandlers.ConfigureMcapCommand)
    .Command<CryptoVolOptions>("crypto vol", CryptoHandlers.Vol,
        builderBuilder: CryptoHandlers.ConfigureVolCommand)

    .RunAsync(args)
//.RunOaktonCommands(args);
;

public static class HostApplicationBuilderX_Local
{
    public static HostApplicationBuilder For(this HostApplicationBuilder hab, Action<HostApplicationBuilder> a)
    {
        a(hab);
        return hab;
    }

    public static HostApplicationBuilder Configure(this HostApplicationBuilder hab, Action<IConfigurationBuilder> c)
    {
        c(hab.Configuration);
        return hab;
    }

    public static IHostBuilder UseHistoricalBars(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureHostConfiguration(c => 
        {
#if DEBUG // TODO: how to best configure this for real?
            c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Trading:HistoricalData:Windows:BaseDir"] = @"c:\st\Investing-HistoricalData", // HARDCODE
                ["Trading:HistoricalData:Unix:BaseDir"] = @"/st/Investing-HistoricalData", // HARDCODE
            });
#endif
        })
            ;
        return hostBuilder;
    }
}
