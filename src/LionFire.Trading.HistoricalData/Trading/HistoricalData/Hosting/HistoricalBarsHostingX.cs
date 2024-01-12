﻿// TODO - mj-map:///C:\st\Projects\FireLynx\FireLynx%20Dash.mmap#oid={F7CABBAD-DA83-4FF0-8BB7-C4DFB4D2077B}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LionFire.Hosting;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Configuration;
using LionFire.Trading.HistoricalData.Sources;
using LionFire.Trading.HistoricalData;

public static class HistoricalBarsHostingX
{
    public static IServiceCollection AddHistoricalBars(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<KlineArrayFileProvider>()
            .AddSingleton<BarsFileSource>()
            .AddSingleton<HistoricalDataChunkRangeProvider>()
            .Configure<HistoricalDataPaths>(configuration.GetSection("LionFire.Trading:HistoricalData")
                .GetSection(OperatingSystem.IsWindows() ? "Windows" : "Unix"))
        ;
        return services;
    }

    public static IServiceCollection AddHistoricalDataDiskSource(this IServiceCollection services)
     => services
             .TryAddEnumerableSingleton<ILocalDiskHistoricalDataSource2, BarsFileSource>()
             ;
    public static IServiceCollection AddHistoricalDataDiskWriter(this IServiceCollection services)
        => services
                .AddSingleton<ILocalDiskHistoricalDataWriter, BarArrayFileWriter>()
                ;
}
