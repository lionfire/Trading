using LionFire.Services;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Orleans_;
using LionFire.Trading.HistoricalData.Retrieval;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static AnyDiff.DifferenceLines;

namespace LionFire.Hosting;

public static class OrleansHistoricalDataHostingExtensions
{
    public static IServiceCollection AddOrleansHistoricalDataClient(this IServiceCollection services)
        => services
               //.TryAddEnumerableSingleton<ILocalNetworkHistoricalDataSource2, OrleansBars>() // FUTURE: Register as a source available on the local network
                .AddSingleton<OrleansBars>()

                // Register to BarsService (IBars) as one of the potentially several available data sources
                .TryAddEnumerableSingleton<LionFire.Trading.HistoricalData.Retrieval.IBars, OrleansBars>(sp => sp.GetRequiredService<OrleansBars>())
                ;
}
