﻿// TODO - mj-map:///C:\st\Projects\FireLynx\FireLynx%20Dash.mmap#oid={F7CABBAD-DA83-4FF0-8BB7-C4DFB4D2077B}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Hosting;
using LionFire.Trading.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using Winton.Extensions.Configuration.Consul;
using Winton.Extensions.Configuration.Consul.Parsers;
using Microsoft.Extensions.Configuration;
using LionFire.Trading.HistoricalData.Sources;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;
using System.Xml.Linq;
using LionFire.Trading.Indicators;

return await Host.CreateDefaultBuilder()
    .LionFire()
    .ConfigureHostConfiguration(c =>
        c.AddConsul("LionFire.Trading", options => { options.Optional = true; options.Parser = new SimpleConfigurationParser(); })
#if DEBUG
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["LionFire.Trading:HistoricalData:Windows:BaseDir"] = @"c:\st\Investing-HistoricalData", // HARDCODE
            ["LionFire.Trading:HistoricalData:Unix:BaseDir"] = @"/st/Investing-HistoricalData", // HARDCODE
        })
#endif
        )

    .ConfigureServices((context, services) =>
    {
        services
            .Configure<HistoricalDataPaths>(context.Configuration.GetSection("HistoricalData")
                .GetSection(OperatingSystem.IsWindows() ? "Windows" : "Unix"))
            .AddSingleton<BinanceClientProvider>()
            .AddSingleton<KlineArrayFileProvider>()
            .AddSingleton<BarsFileSource>()
            .AddSingleton<HistoricalDataChunkRangeProvider>()
            .AddSingleton<IndicatorProvider>()
        ;
    })
    .MultiCommandProgram()
        .Command("backtest", 
            typeof(BacktestCommand)
        )
        .Command("data",
            typeof(ListAvailableHistoricalDataCommand),
            typeof(DumpBarsHierarchicalDataCommand)
        )
        .Command("indicator",
            typeof(ListIndicatorsCommand),
            typeof(CalculateIndicatorCommand)
        )
.Run(args)
//.RunOaktonCommands(args);
    ;
