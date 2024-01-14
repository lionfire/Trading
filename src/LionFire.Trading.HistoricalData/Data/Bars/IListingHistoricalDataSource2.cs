
namespace LionFire.Trading.HistoricalData;

public interface IListableBarsSource
{
    Task<BarsAvailable> List(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame);
}

