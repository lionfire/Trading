using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using System.Linq;

namespace LionFire.Trading.Indicators.Inputs;

public class BarsServiceAspectSeries<TValue> : IHistoricalTimeSeries<TValue>
{
    #region Identity

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }
    public DataPointAspect Aspect { get; }

    #endregion

    #region Dependencies

    public IBars Bars { get; }
    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }

    #endregion

    #region Lifecycle

    public BarsServiceAspectSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, DataPointAspect aspect, IBars bars, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider)
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
        Aspect = aspect;

        Bars = bars;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
    }

    #endregion

    public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var tf = ExchangeSymbolTimeFrame.TimeFrame;
        var chunkedBars = await Bars.ChunkedBars(ExchangeSymbolTimeFrame.ToRange(start, endExclusive));

        var totalExpectedBars = (int)(tf.GetExpectedBarCount(start, endExclusive) ?? throw new ArgumentOutOfRangeException("Invalid date range");
        List<TValue> values = new(totalExpectedBars);

        bool gotSomething = false;
        foreach (var chunk in chunkedBars)
        {
            gotSomething = true;
            if (chunk.Bars.Count <= 0) continue;

            var expectedChunkBars = tf.GetExpectedBarCount(chunk.Start, chunk.EndExclusive) ?? throw new UnreachableCodeException();
            if (expectedChunkBars != chunk.Bars.Count)
            {
                throw new NotImplementedException($"Not implemented: unexpected bar count: {chunk.Bars.Count}.  Expected: {expectedChunkBars}");
            }

            values.AddRange(chunk.Bars.Select(b => (TValue)(object)SymbolValueAspect.Aspect.GetValue(b))); // REVIEW cast - is there a better way?
        }

        if (values.Count != totalExpectedBars)
        {
            throw new NotImplementedException($"Not implemented: unexpected total bar count: {values.Count}.  Expected: {totalExpectedBars}");
        }

        if (!gotSomething || values == null) { return HistoricalDataResult<TValue>.NoData; }
        else return new(values);
    }

    // OLD - Duplicate
    //public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var bars = await Bars.Bars(SymbolBarsRange.FromExchangeSymbolTimeFrame(ExchangeSymbolTimeFrame, start, endExclusive));

    //    return bars.Any() ? (HistoricalDataResult<TValue>)new(bars.Cast<TValue>()) : HistoricalDataResult<TValue>.NoData;
    //}
}
