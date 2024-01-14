using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.Trading.HistoricalData.Sources;

namespace LionFire.Trading.HistoricalData.Retrieval;

//public interface IBars : ITradingDataSource
//{
//    //Task<IEnumerable<BarsChunkInfo>> LocalBarsAvailable(SymbolReference symbolReference); // TODO
//}

public interface IBars : ITradingDataSource 
{
    HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }
    //Task<IEnumerable<IBarsResult>> ChunkedBars(SymbolBarsRange barsRangeReference, QueryOptions? options = null);
    Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunkSource = true, QueryOptions? options = null);

    Task<IBarsResult?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null);
}
public static class BarsX
{
    public static async Task<IEnumerable<IBarsResult>> ChunkedBars(this IBars @this, SymbolBarsRange r, QueryOptions? options = null)
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
    public static async Task<IBarsResult> BarResults(this IBars bars, SymbolBarsRange barsRangeReference, QueryOptions? options = null)
    {
        return (await bars.ChunkedBars(barsRangeReference, options)).AggregateResults();
    }
    public static IBarsResult AggregateResults(this IEnumerable<IBarsResult> barsResults)
    {
        throw new NotImplementedException();
        //return barsResults.SelectMany(r => r.Bars);
    }
    public static async Task<IEnumerable<IKline>> Bars(this IBars bars, SymbolBarsRange barsRangeReference, QueryOptions? options = null)
    {
        return (await bars.ChunkedBars(barsRangeReference, options)).AggregateBars();
    }
    public static IEnumerable<IKline> AggregateBars(this IEnumerable<IBarsResult> barsResults)
    {
        return barsResults.SelectMany(r => r.Bars);
    }
}

// TODO: Bring in ideas from CompositeHistoricalDataProvider2 if it's really helpful
public class BarsService : IBars, IListableBarsSource
{
    public string Name => this.GetType().Name;
    public HistoricalDataSourceKind2 SourceType => HistoricalDataSourceKind2.Compound;

    public IServiceProvider ServiceProvider { get; }
    public BarsFileSource BarsFileSource { get; }
    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }

    public BarsService(IServiceProvider serviceProvider, BarsFileSource barsFileSource
        , IOptionsMonitor<BarFilesPaths> historicalDataPathsOptions
        , HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider
        )
    {
        ServiceProvider = serviceProvider;
        BarsFileSource = barsFileSource;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
        //HistoricalDataPaths = historicalDataPathsOptions.CurrentValue;
    }

  

    public async Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true, QueryOptions? options = null)
    {
        options ??= QueryOptions.Default;

        var chunk = await BarsFileSource.GetShortChunk(range, fallbackToLongChunkSource: fallbackToLongChunk);
        if (chunk != null && chunk.IsUpToDate) { return chunk; }

        if (options.RetrieveSources.HasFlag(HistoricalDataSourceKind.Exchange))
        {
            var j = ActivatorUtilities.CreateInstance<RetrieveHistoricalDataJob>(ServiceProvider); // TODO: Refactor, get exchange-specific service

            var resultList = await j.Execute2(new(range));
            if (resultList != null)
            {
                if (resultList.Count != 1) throw new Exception("Expected exactly zero or one result.");
                chunk = resultList.Single();
            }
        }
        return chunk;
    }

    public Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true) => ((IBars)BarsFileSource).GetShortChunk(range, fallbackToLongChunk);

    public Task<BarChunksAvailable> List(ExchangeSymbolTimeFrame reference) => ((IListableBarsSource)BarsFileSource).List(reference);

    public async Task<IBarsResult?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null)
    {
        options ??= QueryOptions.Default;

        var chunk = await BarsFileSource.GetLongChunk(range);
        if (chunk != null && chunk.IsUpToDate) { return chunk; }

        if (options.RetrieveSources.HasFlag(HistoricalDataSourceKind.Exchange))
        {
            var j = ActivatorUtilities.CreateInstance<RetrieveHistoricalDataJob>(ServiceProvider); // TODO: Refactor, get exchange-specific service

            var resultList = await j.Execute2(new(range));
            if (resultList != null)
            {
                if (resultList.Count != 1) throw new Exception("Expected exactly zero or one result.");
                chunk = resultList.Single();
            }
        }
        return chunk;
    }
}

