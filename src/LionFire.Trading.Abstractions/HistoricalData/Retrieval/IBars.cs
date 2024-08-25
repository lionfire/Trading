//using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.Data;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Options;
using System;

namespace LionFire.Trading.HistoricalData.Retrieval;

public interface IChunkedBars : IBars
{
    // TODO: Instead of null IBarsResult, return a persistence flag code to indicate NotFound, etc.

    DateChunker HistoricalDataChunkRangeProvider { get; }
    //Task<IEnumerable<IBarsResult>> ChunkedBars(SymbolBarsRange barsRangeReference, QueryOptions? options = null);
    //    //Task<IEnumerable<BarsChunkInfo>> LocalBarsAvailable(SymbolReference symbolReference); // TODO?
    Task<IBarsResult<IKline>?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunkSource = true, QueryOptions? options = null);

    Task<IBarsResult<IKline>?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null);
}

public static class BarsConversion
{
    public static IBarsResult<TDestination> Convert<TDestination>(IBarsResult<IKline> barsResult)
    {
        if (typeof(TDestination) == typeof(HLC<double>))
        {
            return (IBarsResult<TDestination>)(object)(new BarsResult<HLC<double>>
            {
                TimeFrame = barsResult.TimeFrame,
                Start = barsResult.Start,
                EndExclusive = barsResult.EndExclusive,
                Values = (IReadOnlyList<HLC<double>>)(object)barsResult.Values.Select(k => new HLC<double>
                {
                    High = (double)k.HighPrice,
                    Low = (double)k.LowPrice,
                    Close = (double)k.ClosePrice
                }).ToList(), // ALLOC
            });
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}

// See also: BarsX for methods that are more useful than the ones on this interface.
public interface IBars : ITradingDataSource
{

    async Task<IBarsResult<TValue>> Get<TValue>(SymbolBarsRange range, QueryOptions? options = null)
    {
        if (typeof(TValue) == typeof(IKline))
        {
            return (IBarsResult<TValue>)(await Get(range, options));
        }
        else
        {
            return BarsConversion.Convert<TValue>(await Get(range, options));
        }
    }

    async Task<IBarsResult<IKline>> Get(SymbolBarsRange range, QueryOptions? options = null)
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

    IHistoricalTimeSeries<IKline<decimal>> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
        => new BarSeries(exchangeSymbolTimeFrame, this);

    IHistoricalTimeSeries<decimal> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, DataPointAspect aspect)
        => new BarAspectSeries<decimal>(exchangeSymbolTimeFrame, aspect, this);

    #endregion

}

