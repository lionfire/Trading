
using LionFire.Trading.HistoricalData;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading;

public class BinanceDataTest
{
    public IServiceProvider ServiceProvider => ServiceProviderProvider.ServiceProvider;

    public DateChunker HistoricalDataChunkRangeProvider => ServiceProvider.GetRequiredService<DateChunker>();

}
