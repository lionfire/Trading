using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.Trading.HistoricalData.Sources;

namespace LionFire.Trading.HistoricalData.Retrieval;



public interface IBarsSource
{
    string Name { get; }
    HistoricalDataSourceKind2 SourceType { get; }

}
public interface IBars : IBarsSource
{

    Task<BarsResult> Bars(SymbolBarsRange barsRangeReference, bool localOnly = false);
    //Task<IEnumerable<BarsChunkInfo>> LocalBarsAvailable(SymbolReference symbolReference); // TODO
}

public interface IChunkedBars : IBarsSource
{
    Task<BarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true, bool localOnly = false);

    Task<BarsResult> GetLongChunk(SymbolBarsRange range);
}

public interface IChunkedBarsWriter
{

}




public class BarsService : IBars, IChunkedBars
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

    public async Task<BarsResult> Bars(SymbolBarsRange barsRangeReference, bool localOnly = false)
    {

        var j = ActivatorUtilities.CreateInstance<RetrieveHistoricalDataJob>(ServiceProvider);
        await j.Execute2(new(barsRangeReference));

        //var dir = this.HistoricalDataPaths.GetDataDir(Exchange.ToLowerInvariant(), ExchangeArea, Symbol, TimeFrame);
        //this.HistoricalDataChunkRangeProvider.LongRangeForDate(start, TimeFrame);
        //var file = KlineArrayFileProvider.GetFile(Exchange, ExchangeArea, Symbol, TimeFrame, start);
    }


    public async Task<BarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true, bool localOnly = false)
    {
        var chunk = await _TryGetShortChunk(range);

        if (chunk == null && fallbackToLongChunk)
        {
            chunk = await GetLongChunk(range);

            if (chunk != null)
            {
                chunk = chunk.Trim(range.Start, range.EndExclusive);
            }
        }
        return chunk;


        Task<object?> _TryGetShortChunk(SymbolBarsRange range)
        {
            var path = BarFilesPaths.GetPath(exchange, exchangeArea, symbol, timeFrame, info, options);
            // try getting short chunk file

        }
    }

    public Task<BarsResult> GetLongChunk(SymbolBarsRange range)
    {
        var path = BarFilesPaths.GetPath(exchange, exchangeArea, symbol, timeFrame, info, options);

    }

    #region (Private)


    #endregion

}

