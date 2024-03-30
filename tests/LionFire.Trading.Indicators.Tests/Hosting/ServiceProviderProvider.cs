﻿using LionFire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Binance.Net;

public static class ServiceProviderProvider
{
    public static IServiceProvider ServiceProvider => serviceProvider ??= Init();

    private static IServiceProvider Init()
    {
        var Configuration = new ConfigurationManager();
        Configuration.AddInMemoryCollection([
            new ("LionFire.Trading:HistoricalData:Windows:BaseDir", @"F:\st\Investing-HistoricalData\")
        ]);

        IServiceCollection services = new ServiceCollection();
        services
            .AddOptions()
            .AddLogging(b => b.AddConsole())
            .AddSingleton<IConfiguration>(Configuration)
            .AddHistoricalBars(Configuration)
            .AddIndicators()
            .AddQuantConnectIndicators()

            .AddBinance()
            ;

        return services.BuildServiceProvider();
    }
    private static IServiceProvider? serviceProvider;
}
