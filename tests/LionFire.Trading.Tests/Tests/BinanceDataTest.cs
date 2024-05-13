using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Indicators.Inputs;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading;

public class BinanceDataTest
{
    public IServiceProvider ServiceProvider => ServiceProviderProvider.ServiceProvider;

    public DateChunker HistoricalDataChunkRangeProvider => ServiceProvider.GetRequiredService<DateChunker>();
    public IHistoricalTimeSeries<T> Resolve<T>(object source) => ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve<T>(source);

}
