using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;


namespace LionFire.Trading;

public static class BarsX
{
    public static async Task<IEnumerable<IBarsResult<IKline>>> ChunkedBars(this IChunkedBars @this, SymbolBarsRange r, QueryOptions? options = null)
    {
        var resultList = new List<IBarsResult<IKline>>();

        foreach (var c in @this.HistoricalDataChunkRangeProvider.GetBarChunks(r))
        {
            var range = new SymbolBarsRange(r.Exchange, r.Area, r.Symbol, r.TimeFrame, c.Item1.start, c.Item1.endExclusive);
            var chunkResult = await (c.isLong ? @this.GetLongChunk(range, options) : @this.GetShortChunk(range, false, options));
            if (chunkResult != null) { resultList.Add(chunkResult); }
        }
        return resultList;
    }
    public static async Task<IBarsResult<IKline>> BarResults(this IChunkedBars bars, SymbolBarsRange barsRangeReference, QueryOptions? options = null)
    {
        return (await bars.ChunkedBars(barsRangeReference, options)).AggregateResults();
    }
    public static IBarsResult<TValue> AggregateResults<TValue>(this IEnumerable<IBarsResult<TValue>> barsResults)
    {
        throw new NotImplementedException();
        //return barsResults.SelectMany(r => r.Bars);
    }
    public static async Task<IEnumerable<IKline>> Bars(this IChunkedBars bars, SymbolBarsRange barsRangeReference, QueryOptions? options = null)
    {
        return (await bars.ChunkedBars(barsRangeReference, options)).AggregateBars(barsRangeReference.Start, barsRangeReference.EndExclusive);
    }
    public static IEnumerable<TValue> AggregateBars<TValue>(this IEnumerable<IBarsResult<TValue>> barsResults, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    {
        var result = barsResults.SelectMany(r => r.Bars);

        // OPTIMIZE: use math to calculate index to skip:
        if (typeof(TValue).IsAssignableTo(typeof(IKlineWithOpenTime)))
        {
            if (start.HasValue)
            {
                result = result.Cast<IKlineWithOpenTime>().SkipWhile(b => b.OpenTime < start.Value.UtcDateTime).Cast<TValue>();
            }
            if (endExclusive.HasValue)
            {
                result = result.Cast<IKlineWithOpenTime>().TakeWhile(b => b.OpenTime < endExclusive.Value.UtcDateTime).Cast<TValue>(); ;
            }
        }
        else if (start.HasValue || endExclusive.HasValue)
        {
            throw new NotSupportedException($"{nameof(start)} and {nameof(endExclusive)} not supported with this type of Kline");
        }

        return result;
    }
}

