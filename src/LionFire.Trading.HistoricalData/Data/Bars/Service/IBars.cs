using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.Data;

namespace LionFire.Trading.HistoricalData.Retrieval;

public interface IBars : ITradingDataSource

{
    HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }
    //Task<IEnumerable<IBarsResult>> ChunkedBars(SymbolBarsRange barsRangeReference, QueryOptions? options = null);
    //    //Task<IEnumerable<BarsChunkInfo>> LocalBarsAvailable(SymbolReference symbolReference); // TODO?
    Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunkSource = true, QueryOptions? options = null);

    Task<IBarsResult?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null);

    // REVIEW - make IBars more generic? Add more features to IHistoricalTimeSeries<T>?

    IHistoricalTimeSeries<IKline> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame);
    IHistoricalTimeSeries<decimal> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, DataPointAspect aspect);


}

