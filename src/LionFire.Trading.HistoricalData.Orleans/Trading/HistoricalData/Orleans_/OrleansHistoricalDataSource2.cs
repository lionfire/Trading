using Orleans;

namespace LionFire.Trading.HistoricalData.Orleans_;


public class OrleansHistoricalDataSource2 : HistoricalDataProvider2Base, ILocalNetworkHistoricalDataSource2
{
    public HistoricalDataSourceKind Kind => HistoricalDataSourceKind.LocalNetwork;

    public IClusterClient ClusterClient { get; }

    public OrleansHistoricalDataSource2(IClusterClient clusterClient)
    {
        ClusterClient = clusterClient;
    }

    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }

    protected override Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions) 
        => ClusterClient.GetGrain<HistoricalDataChunkGrain>(
                HistoricalDataChunkGrainKey.GetKey(timeFrame, symbol, start, endExclusive, retrieveOptions.Exchange, retrieveOptions.ExchangeArea)
            )
            .Get<T>(retrieveOptions.Options);

}
