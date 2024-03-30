using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;


namespace LionFire.Trading;

public static class BarsX
{
    public static async Task<IEnumerable<IBarsResult>> ChunkedBars(this IChunkedBars @this, SymbolBarsRange r, QueryOptions? options = null)
    {
        var resultList = new List<IBarsResult>();

        foreach (var c in @this.HistoricalDataChunkRangeProvider.GetBarChunks(r))
        {
            var range = new SymbolBarsRange(r.Exchange, r.ExchangeArea, r.Symbol, r.TimeFrame, c.Item1.start, c.Item1.endExclusive);
            var chunkResult = await (c.isLong ? @this.GetLongChunk(range, options) : @this.GetShortChunk(range, false, options));
            if (chunkResult != null) { resultList.Add(chunkResult); }
        }
        return resultList;
    }
    public static async Task<IBarsResult> BarResults(this IChunkedBars bars, SymbolBarsRange barsRangeReference, QueryOptions? options = null)
    {
        return (await bars.ChunkedBars(barsRangeReference, options)).AggregateResults();
    }
    public static IBarsResult AggregateResults(this IEnumerable<IBarsResult> barsResults)
    {
        throw new NotImplementedException();
        //return barsResults.SelectMany(r => r.Bars);
    }
    public static async Task<IEnumerable<IKline>> Bars(this IChunkedBars bars, SymbolBarsRange barsRangeReference, QueryOptions? options = null)
    {
        return (await bars.ChunkedBars(barsRangeReference, options)).AggregateBars(barsRangeReference.Start, barsRangeReference.EndExclusive);
    }
    public static IEnumerable<IKline> AggregateBars(this IEnumerable<IBarsResult> barsResults, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    {
        var result = barsResults.SelectMany(r => r.Bars);

        // OPTIMIZE: use math to calculate index to skip:
        if (start.HasValue)
        {
            result = result.SkipWhile(b => b.OpenTime < start.Value.UtcDateTime);
        }
        if(endExclusive.HasValue)
        {
            result = result.TakeWhile(b => b.OpenTime < endExclusive.Value.UtcDateTime);
        }
        return result;
    }
}

