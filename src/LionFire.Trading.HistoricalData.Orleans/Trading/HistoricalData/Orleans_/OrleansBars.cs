﻿using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using Orleans;
using System.Threading.Tasks;

namespace LionFire.Trading.HistoricalData.Orleans_;


public class OrleansBars : ILocalNetworkBars, IChunkedBars
{
    #region Identity

    public string Name => "Orleans";
    public HistoricalDataSourceKind Kind => HistoricalDataSourceKind.LocalNetwork;
    public HistoricalDataSourceKind2 SourceType => HistoricalDataSourceKind2.Local;

    #endregion

    #region Dependencies

    public IClusterClient ClusterClient { get; }

    public DateChunker HistoricalDataChunkRangeProvider { get; }

    #endregion

    #region Lifecycle

    public OrleansBars(IClusterClient clusterClient, DateChunker historicalDataChunkRangeProvider)
    {
        ClusterClient = clusterClient;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
    }

    #endregion

    public async Task<IBarsResult<IKline>?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunkSource = true, QueryOptions? options = null)
    {
        var grainKey = range.ToId();
        var grain = ClusterClient.GetGrain<IHistoricalBarsChunkG>(grainKey);
        var bars = await grain.Bars();

        var result = new BarsResult<IKline>
        {
            Values = bars,
            Start = range.Start,
            EndExclusive = range.EndExclusive,
            TimeFrame = range.TimeFrame,
        };

        return result;
    }

    public Task<IBarsResult<IKline>?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null) => throw new NotSupportedException();

    //protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    //{
    //    throw new NotImplementedException();
    //}

    //protected override Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions) 
    //    => ClusterClient.GetGrain<HistoricalDataChunkGrain>(
    //            HistoricalDataChunkGrainKey.GetKey(timeFrame, symbol, start, endExclusive, retrieveOptions.Exchange, retrieveOptions.ExchangeArea, typeof(Binance.BinanceFuturesKlineItem))
    //        )
    //        .Get<T>(retrieveOptions.Options);

}
