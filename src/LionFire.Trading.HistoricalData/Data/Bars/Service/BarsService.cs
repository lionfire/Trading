using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.Trading.HistoricalData.Sources;
using LionFire.Trading.Data;

namespace LionFire.Trading.HistoricalData.Retrieval;

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


    // OPTIMIZE: Cache rather than recreate
    public IHistoricalTimeSeries<IKline> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame) => new BarsServiceBarSeries(exchangeSymbolTimeFrame, this);

    // OPTIMIZE: Cache rather than recreate
    public IHistoricalTimeSeries<decimal> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, DataPointAspect aspect) => new BarsServiceAspectSeries<decimal>(exchangeSymbolTimeFrame, this, aspect);

}

