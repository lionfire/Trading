using LionFire.ExtensionMethods;
using LionFire.Structures;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.HistoricalData;

public abstract class HistoricalDataNetworkSource : HistoricalDataProvider2Base, IHistoricalDataSource2
{
    public virtual HistoricalDataSourceKind Kind => HistoricalDataSourceKind.Exchange;

}

public class BinanceSpotHistoricalDataSource : HistoricalDataNetworkSource
{
    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }

    protected override Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }
}

public class BinanceFuturesHistoricalDataSource : HistoricalDataNetworkSource
{
    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }

    protected override Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }
}


public class ExchangeHistoricalDataSourceProvider
{
    public string Exchange { get; set; }
    public Dictionary<string, Type> Areas { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// ENH Deluxe:
///  - options for caching, reading direct from network without saving to disk
///  
/// </remarks>
public class CompositeHistoricalDataProvider2 : HistoricalDataProvider2Base
{

    #region Dependencies

    protected HistoricalDataProvider2Options Options { get; }
    protected HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; set; }

    public ILogger<CompositeHistoricalDataProvider2> Logger { get; }


    public IEnumerable<IHistoricalDataSource2>? LocalDiskSources { get; set; }
    public ILocalDiskHistoricalDataWriter? LocalDiskWriter { get; set; }

    public IEnumerable<IHistoricalDataSource2>? LocalNetwork { get; set; }

    public Dictionary<string, IHistoricalDataSource2> Exchange { get; set; }
    public Dictionary<string, List<IHistoricalDataSource2>> ThirdParty { get; set; }

    #endregion

    public CompositeHistoricalDataProvider2(ILogger<CompositeHistoricalDataProvider2> logger, IOptionsMonitor<HistoricalDataProvider2Options> options, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider, IServiceProvider serviceProvider)
    {
        Logger = logger;
        Options = options.CurrentValue;

        LocalDiskSources = serviceProvider.GetService<IEnumerable<IBarFileSources>>();
        LocalDiskWriter = serviceProvider.GetService<ILocalDiskHistoricalDataWriter>();

        LocalNetwork = serviceProvider.GetService<IEnumerable<ILocalNetworkHistoricalDataSource2>>();
    }

    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        var chunks = HistoricalDataChunkRangeProvider.GetBarChunks(start, endExclusive, timeFrame);

        foreach (var chunk in chunks)
        {

        }

        //throw new NotImplementedException(); // TODO
        return Task.FromResult(true);
    }

    protected override async Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveParameters)
    {
        HistoricalDataMemoryCacheKey? memoryCacheKey;
        if (Options.UseMemoryCache)
        {
            memoryCacheKey = new HistoricalDataMemoryCacheKey
            {
                Type = typeof(T),
                Symbol = symbol,
                Start = start,
                EndExclusive = endExclusive,
                Exchange = retrieveParameters.Exchange,
                ExchangeArea = retrieveParameters.ExchangeArea,
            };

            if (ManualSingleton<HistoricalDataMemoryCache<T>>.GuaranteedInstance.Dict.TryGetValue(memoryCacheKey, out var value))
            {
                Logger.LogTrace($"[cache hit] {memoryCacheKey}"); // TODO TOTELEMETRY - counter for memory cache hit
                return value;
            }
        }
        else
        {
            memoryCacheKey = null;
            Logger.LogTrace($"[CACHE MISS] {timeFrame} {symbol} {start} {endExclusive}"); // TODO TOTELEMETRY - counter for memory cache miss
        }

        if (LocalNetwork != null)
        {
            T[]? result;
            foreach (var c in LocalNetwork)
            {
                result = await c.Get<T>(timeFrame, symbol, start, endExclusive, retrieveParameters).ConfigureAwait(false);
                OnResult(result);
                return result;
            }
        }

        var chunks = HistoricalDataChunkRangeProvider.GetBarChunks(start, endExclusive, timeFrame);

        var lists = new List<T[]>();
        int totalBars = 0;
        foreach (var chunk in chunks)
        {
            var chunkData = await GetChunkImpl<T>(timeFrame, symbol, chunk.Item1, chunk.Item2, retrieveParameters).ConfigureAwait(false);
            lists.Add(chunkData);
            totalBars += chunkData.Length;
        }

        //if (retrieveOptions.OptimizeForBacktest && result.Count > 1)
        //{
        var combined = new List<T>(totalBars);
        foreach (var list in lists) { combined.AddRange(list); }

        var combinedArray = combined.ToArray();

        OnResult(combinedArray);

        void OnResult(T[]? result)
        {
            if (Options.UseMemoryCache)
            {
                ManualSingleton<HistoricalDataMemoryCache<T>>.GuaranteedInstance.Dict[memoryCacheKey] = result;
            }
        }
        return combinedArray;

        //}
    }

    protected async Task<T[]?> GetChunkImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveParameters)
    {
        //HistoricalDataChunkRangeProvider.ValidateIsChunkBoundary(start, endExclusive, timeFrame);
        // TODO:
        //
        // Try reading from disk (if disk reader present)
        // If not present, run the Retrieve Job,
        // write it to disk

        T[]? result;

        if (LocalDiskSources != null)
        {
            foreach (var c in LocalDiskSources)
            {
                result = await c.Get<T>(timeFrame, symbol, start, endExclusive, retrieveParameters).ConfigureAwait(false);
                if(result != null)
                {
                    // TOTELEMETRY - disk cache hit
                    return result;
                }
            }
            // TOTELEMETRY - disk cache miss
        }

        {
            var exchangeSource = Exchange.TryGetValue(retrieveParameters.Exchange ?? throw new ArgumentNullException(nameof(retrieveParameters.Exchange)));
            if (exchangeSource != null)
            {
                result = await exchangeSource.Get<T>(timeFrame, symbol, start, endExclusive, retrieveParameters).ConfigureAwait(false);
                if (result != null)
                {
                    // TOTELEMETRY - exchange retrieve
                    OnRetrievedFromInternet(exchangeSource.Name, result, timeFrame, symbol, start, endExclusive, retrieveParameters);
                    return result;
                }
            }
        }

        {
            var thirdPartySources = ThirdParty.TryGetValue(retrieveParameters.Exchange);
            if (thirdPartySources != null)
            {
                foreach (var source in thirdPartySources)
                {
                    result = await source.Get<T>(timeFrame, symbol, start, endExclusive, retrieveParameters).ConfigureAwait(false);
                    if (result != null)
                    {
                        // TOTELEMETRY - third party retrieve
                        OnRetrievedFromInternet(source.Name, result, timeFrame, symbol, start, endExclusive, retrieveParameters);
                        return result;
                    }
                }
            }
        }

        return null;
    }

    private Task OnRetrievedFromInternet<T>(string sourceId, T[] data, TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveParameters) 
        => LocalDiskWriter == null ? Task.CompletedTask
            : LocalDiskWriter.Save(sourceId, data, timeFrame, symbol, start, endExclusive, retrieveParameters);

    //public virtual Span<OhlcDecimalItem> GetOhlcDecimal(DateTime start, DateTime endExclusive, TimeFrame timeFrame, HistoricalDataQueryOptions? retrieveParameters = null)
    //{
    //    retrieveParameters ??= HistoricalDataQueryOptions.Default;

    //    Span<OhlcDecimalItem> result;
    //    if (retrieveOptions.RetrieveSources.HasFlag(HistoricalDataSourceKind.InMemory) && InMemory != null)
    //    {
    //        result = InMemory.GetOhlcDecimal(start, endExclusive, timeFrame, retrieveOptions);
    //    }

    //}

}

public class BarArrayFileWriter : ILocalDiskHistoricalDataWriter
{
    public Task Save<T>(string sourceId, T[] data, TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveParameters)
    {
        throw new NotImplementedException();
    }
}

