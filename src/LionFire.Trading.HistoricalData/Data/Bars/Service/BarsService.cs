using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.Trading.HistoricalData.Sources;
using LionFire.Trading.Data;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.HistoricalData.Retrieval;

// TODO: Bring in ideas from CompositeHistoricalDataProvider2 if it's really helpful

/// <summary>
/// A multi-source service for retrieving historical data.
/// 
/// Sources:
/// - BarsFileSource (local disk cache)
/// - RetrieveHistoricalDataJob (retrieve from exchange over Internet)
/// </summary>
public class BarsService : IBars, IListableBarsSource, IChunkedBars
{
    #region Identity

    public string Name => this.GetType().Name;
    public HistoricalDataSourceKind2 SourceType => HistoricalDataSourceKind2.Compound;

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public ILogger<BarsService> Logger { get; }

    #region Components

    public IEnumerable<IBars> AllSources { get; } // UNUSED. FUTURE: configure local/disk/network/remote etc. services from this collection
    public BarsFileSource BarsFileSource { get; } // FUTURE: Replace the type with a more generic interface marker for Local Disk file source

    #endregion

    #endregion

    #region Parameters

    public DateChunker HistoricalDataChunkRangeProvider { get; }

    #endregion

    #region Lifecycle

    public BarsService(IServiceProvider serviceProvider
        , BarsFileSource barsFileSource
        , IOptionsMonitor<BarFilesPaths> historicalDataPathsOptions
        , DateChunker historicalDataChunkRangeProvider
        //, IEnumerable<IBars> allSources
        , ILogger<BarsService> logger
        )
    {
        ServiceProvider = serviceProvider;
        BarsFileSource = barsFileSource;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
        //AllSources = allSources;
        Logger = logger;
        //HistoricalDataPaths = historicalDataPathsOptions.CurrentValue;
        //foreach (var bars in allSources)
        //{
        //    logger.LogInformation($"IBars source: {bars.Name} {bars.SourceType}");
        //}
    }

    #endregion

    #region IChunkedBars

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

    public Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true) => ((IChunkedBars)BarsFileSource).GetShortChunk(range, fallbackToLongChunk);

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

    #endregion

    #region IListableBarsSource

    /// <summary>
    /// Pass-through to BarsFileSource
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    public Task<BarChunksAvailable> List(ExchangeSymbolTimeFrame reference) => ((IListableBarsSource)BarsFileSource).List(reference);

    #endregion
}

