using LionFire.Services;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Orleans_;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Hosting;

public static class OrleansHistoricalDataHostingExtensions
{
    public static IServiceCollection AddOrleansHistoricalDataClient(this IServiceCollection services) 
        => services
                .TryAddEnumerableSingleton<ILocalNetworkHistoricalDataSource2, OrleansHistoricalDataSource2>()
                ;
}
