using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using System.Linq;

namespace LionFire.Trading.Indicators.Inputs;

public class SymbolValueAspectInput<TValue> : IHistoricalTimeSeries<TValue>
{
    #region Identity

    public SymbolValueAspect SymbolValueAspect { get; }

    #endregion

    #region Dependencies

    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }
    public IBars Bars { get; }

    #endregion

    #region Lifecycle

    public SymbolValueAspectInput(IBars bars, SymbolValueAspect symbolValueAspect, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider)
    {
        this.SymbolValueAspect = symbolValueAspect;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
        Bars = bars;
    }

    #endregion

    public async ValueTask<HistoricalDataResult<TValue>> TryGetValues(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var chunkedBars = await Bars.ChunkedBars(SymbolValueAspect.ToRange(start, endExclusive));

        List<TValue>? values = null;
        bool gotSomething = false;
        foreach (var chunk in chunkedBars)
        {
            gotSomething = true;
            if (chunk.Bars.Count <= 0) continue;
            values ??= [];
            values.AddRange(chunk.Bars.Select(b => (TValue)(object)SymbolValueAspect.Aspect.GetValue(b))); // REVIEW cast - is there a better way?
        }

        if (!gotSomething || values == null) { return HistoricalDataResult<TValue>.NoData; }
        else return new(values);
    }

    // NOTE on optimization: This probably can't be optimized because we need to select an aspect of the bar
    //public async ValueTask<HistoricalDataResult<IEnumerable<TValue>>?> TryGetValueChunks(DateTimeOffset start, DateTimeOffset endExclusive) { ... }
}
