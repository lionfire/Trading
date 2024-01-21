
namespace LionFire.Trading.HistoricalData;

public interface IListableBarsSource
{
    Task<BarChunksAvailable> List(ExchangeSymbolTimeFrame reference);
}

