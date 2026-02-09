using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.Trading.HistoricalData.Sources;
using LionFire.Trading.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

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

    #region Request Coalescing

    /// <summary>
    /// Coalesces concurrent requests for the same data chunk so that only one download
    /// is triggered per unique SymbolBarsRange. All concurrent callers await the same Task.
    /// Uses Lazy&lt;Task&gt; to guarantee exactly one download task is created per key,
    /// even under ConcurrentDictionary.GetOrAdd contention.
    /// </summary>
    private readonly ConcurrentDictionary<SymbolBarsRange, Lazy<Task<IBarsResult<IKline>?>>> _pendingRetrievals = new();

    private async Task<IBarsResult<IKline>?> CoalescedRetrieveAsync(SymbolBarsRange range)
    {
        var lazy = _pendingRetrievals.GetOrAdd(range,
            r => new Lazy<Task<IBarsResult<IKline>?>>(() => RetrieveFromExchangeAsync(r)));

        try
        {
            return await lazy.Value.ConfigureAwait(false);
        }
        finally
        {
            // Only remove if the entry still points to OUR Lazy instance.
            // Using TryRemove(KeyValuePair) prevents a late-finishing awaiter
            // from removing a newer Lazy that was created for a retry.
            _pendingRetrievals.TryRemove(KeyValuePair.Create(range, lazy));
        }
    }

    private async Task<IBarsResult<IKline>?> RetrieveFromExchangeAsync(SymbolBarsRange range)
    {
        Logger.LogInformation("Starting coalesced retrieval for {Exchange}:{Symbol} {TimeFrame} {Start}-{End}",
            range.Exchange, range.Symbol, range.TimeFrame, range.Start, range.EndExclusive);

        var j = ActivatorUtilities.CreateInstance<RetrieveHistoricalDataJob>(ServiceProvider);
        var resultList = await j.Execute2(new(range)).ConfigureAwait(false);

        if (resultList != null)
        {
            if (resultList.Count != 1) throw new Exception("Expected exactly zero or one result.");
            return resultList.Single();
        }
        return null;
    }

    #endregion

    #region IChunkedBars

    public async Task<IBarsResult<IKline>?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true, QueryOptions? options = null)
    {
        options ??= QueryOptions.Default;

        var chunk = await BarsFileSource.GetShortChunk(range, fallbackToLongChunkSource: fallbackToLongChunk);
        if (chunk != null && chunk.IsUpToDate) { return chunk; }

        if (options.RetrieveSources.HasFlag(HistoricalDataSourceKind.Exchange))
        {
            chunk = await CoalescedRetrieveAsync(range).ConfigureAwait(false);
        }
        return chunk;
    }

    public Task<IBarsResult<IKline>?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunk = true) => ((IChunkedBars)BarsFileSource).GetShortChunk(range, fallbackToLongChunk);

    public async Task<IBarsResult<IKline>?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null)
    {
        options ??= QueryOptions.Default;

        bool canTryAgain = true;
    tryAgain:
        var chunk = await BarsFileSource.GetLongChunk(range);
        if (chunk != null)
        {
            if (chunk.IsUpToDate) { return chunk; }
            else { throw new NotImplementedException("TODO: Delete out of date chunk: " + range); }
        }

        if (options.RetrieveSources.HasFlag(HistoricalDataSourceKind.Exchange))
        {
            try
            {
                chunk = await CoalescedRetrieveAsync(range).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!canTryAgain) throw;
                // else exception gets dropped
            }

            if (chunk == null && canTryAgain)
            {
                canTryAgain = false;
                goto tryAgain;
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

