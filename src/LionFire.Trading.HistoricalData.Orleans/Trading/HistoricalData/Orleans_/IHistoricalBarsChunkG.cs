using Binance.Net.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace LionFire.Trading.HistoricalData;

[Alias("historical-bars-chunk")]
public interface IHistoricalBarsChunkG : IGrainWithStringKey
{

    [return: Immutable]
    Task<IEnumerable<IKline>> BarsInRange(DateTime start, DateTime endExclusive);

    [return: Immutable]
    Task<IReadOnlyList<IKline>> Bars();
}


