using Binance.Net.Interfaces;

namespace LionFire.Trading.Binance_;

[Alias("historical-bars-chunk")]
public interface IHistoricalBarsChunk : IGrainWithStringKey
{

    Task<IEnumerable<IBinanceKline>> BarsInRange(DateTime start, DateTime endExclusive);
    Task<List<IBinanceKline>> Bars();
}
