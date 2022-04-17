
namespace LionFire.Trading.HistoricalData;

public interface IHistoricalDataSource2 : IHistoricalDataProvider2
{
    string SourceId { get; }

    HistoricalDataSourceKind Kind { get; }
}

public interface ILocalNetworkHistoricalDataSource2 : IHistoricalDataSource2 { }
public interface ILocalDiskHistoricalDataSource2 : IHistoricalDataSource2 { }

public interface ILocalDiskHistoricalDataWriter
{
    Task Save<T>(string sourceId, T[] data, TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveParameters);
}

