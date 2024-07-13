using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
//using LionFire.Trading.HistoricalData.Serialization;
using System.Linq;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class BarAspectSeries<TValue> : IHistoricalTimeSeries<TValue>
{
    #region Identity

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }
    public TimeFrame TimeFrame => ExchangeSymbolTimeFrame.TimeFrame;
    public DataPointAspect Aspect { get; }

    public Type ValueType => typeof(TValue);

    #endregion

    #region Dependencies

    public IBars Bars { get; }

    #endregion

    #region Lifecycle

    public BarAspectSeries(
        ExchangeSymbolTimeFrame exchangeSymbolTimeFrame
        , DataPointAspect aspect
        // Injected:
        , IBars bars
        )
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
        Aspect = aspect;

        Bars = bars;
    }

    #endregion

    //private QueryOptions QueryOptions => QueryOptions.Default;


    public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var result = await Bars.Get(ExchangeSymbolTimeFrame.ToRange(start, endExclusive));
        if (result == null || !result.IsSuccess)
        {
            return HistoricalDataResult<TValue>.NoData;
        }
        else
        {
            return new(result.Values!.Select(bar => Aspect.GetValue<TValue>(bar)));
        }
    }

    //public async ValueTask<HistoricalDataResult<TValue>> GetFromChunks(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var tf = ExchangeSymbolTimeFrame.TimeFrame;
    //    var chunkedBars = await Bars.ChunkedBars(ExchangeSymbolTimeFrame.ToRange(start, endExclusive));

    //    var totalExpectedBars = (int)(tf.GetExpectedBarCount(start, endExclusive) ?? throw new ArgumentOutOfRangeException("Invalid date range"));
    //    List<TValue> values = new(totalExpectedBars);

    //    bool gotSomething = false;
    //    foreach (var chunk in chunkedBars)
    //    {
    //        if (chunk.Bars == null)
    //        {
    //            if (chunk.IsSuccess)
    //            {
    //                continue;
    //            }
    //            else
    //            {
    //                throw new Exception("Failed to get chunked bars: " + chunk.FailReason);
    //            }
    //        }

    //        gotSomething = true;
    //        var chunkCount = chunk.Bars.Count();
    //        if (chunkCount <= 0) continue;

    //        var expectedChunkBars = tf.GetExpectedBarCount(chunk.Start, chunk.EndExclusive) ?? throw new UnreachableCodeException();
    //        if (expectedChunkBars != chunkCount)
    //        {
    //            throw new NotImplementedException($"Not implemented: unexpected bar count: {chunkCount}.  Expected: {expectedChunkBars}");
    //        }

    //        values.AddRange(chunk.Bars.Select(b => (TValue)(object)Aspect.GetValue(b))); // REVIEW cast - is there a better way?
    //    }

    //    if (values.Count != totalExpectedBars)
    //    {
    //        throw new NotImplementedException($"Not implemented: unexpected total bar count: {values.Count}.  Expected: {totalExpectedBars}");
    //    }

    //    if (!gotSomething || values == null) { return HistoricalDataResult<TValue>.NoData; }
    //    else return new(values);
    //}

    // OLD - Duplicate, non-chunked reading of input
    //public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var bars = await Bars.Bars(SymbolBarsRange.FromExchangeSymbolTimeFrame(ExchangeSymbolTimeFrame, start, endExclusive));

    //    return bars.Any() ? (HistoricalDataResult<TValue>)new(bars.Cast<TValue>()) : HistoricalDataResult<TValue>.NoData;
    //}
}
