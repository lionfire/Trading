using LionFire.Services;
using LionFire.Trading.HistoricalData;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Hosting;

public static class HistoricalDataHostingExtensions
{
    public static IServiceCollection AddHistoricalDataDiskSource(this IServiceCollection services) 
        => services
                .TryAddEnumerableSingleton<ILocalDiskHistoricalDataSource2, BarArrayFileSource2>()
                ;
    public static IServiceCollection AddHistoricalDataDiskWriter(this IServiceCollection services)
        => services
                .AddSingleton<ILocalDiskHistoricalDataWriter, BarArrayFileWriter>()
                ;
}
