using Binance.Net.Interfaces;

namespace LionFire.Trading.Binance_;

[Alias("historical-bars-chunk")]
public interface IHistoricalBarsChunk : IGrainWithStringKey
{

    Task<IEnumerable<IBinanceKline>> GetBars(DateTime start, DateTime endExclusive);
}
