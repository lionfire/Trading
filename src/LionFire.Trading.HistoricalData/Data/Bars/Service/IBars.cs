using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.Data;
using Microsoft.CodeAnalysis.Operations;

namespace LionFire.Trading.HistoricalData.Retrieval;

public interface IChunkedBars : IBars
{
    // TODO: Instead of null IBarsResult, return a persistence flag code to indicate NotFound, etc.

    DateChunker HistoricalDataChunkRangeProvider { get; }
    //Task<IEnumerable<IBarsResult>> ChunkedBars(SymbolBarsRange barsRangeReference, QueryOptions? options = null);
    //    //Task<IEnumerable<BarsChunkInfo>> LocalBarsAvailable(SymbolReference symbolReference); // TODO?
    Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunkSource = true, QueryOptions? options = null);

    Task<IBarsResult?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null);
}

// See also: BarsX for methods that are more useful than the ones on this interface.
public interface IBars : ITradingDataSource
{

    async Task<IBarsResult> Get(SymbolBarsRange range, QueryOptions? options = null)
    {
        if (options != null)
        {
            bool strictChunkMode = false;
            if (options.Flags.HasFlag(HistoricalDataQueryFlags.ShortChunk))
            {
                strictChunkMode = true;
                var result = await ((IChunkedBars)this).GetShortChunk(range, options.Flags.HasFlag(HistoricalDataQueryFlags.LongChunk), options);
                if (result?.Bars != null) { return result; }
            }
            else if (options.Flags.HasFlag(HistoricalDataQueryFlags.LongChunk))
            {
                strictChunkMode = true;
                var result = await ((IChunkedBars)this).GetLongChunk(range, options);
                if (result?.Bars != null) { return result; }
            }
            if (strictChunkMode)
            {
                throw new ArgumentOutOfRangeException($"{nameof(strictChunkMode)} enabled but range does not fall on chunk boundary");
            }
        }
        if (this is IChunkedBars cb)
        {
            var values = await cb.Bars(range, options);
            return new BarsResult<IKline>
            {
                Values = values.ToList(), // ALLOC
                Start = range.Start,
                EndExclusive = range.EndExclusive,
                TimeFrame = range.TimeFrame,
            };
        }
        else
        {
            throw new NotImplementedException("IBars.GetSourcesInfo must be implemented if this is not IChunkedBars");
        }
    }

    // REVIEW -  Add more features to IHistoricalTimeSeries<T>?

    #region GetSeries

    // OPTIMIZE: Cache rather than recreate, for these methods

    IHistoricalTimeSeries<IKline> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
        => new BarSeries(exchangeSymbolTimeFrame, this);

    IHistoricalTimeSeries<decimal> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, DataPointAspect aspect)
        => new BarAspectSeries<decimal>(exchangeSymbolTimeFrame, aspect, this);

    #endregion

}

