
namespace LionFire.Trading.HistoricalData;

public interface ILocalDiskHistoricalDataWriter
{
    Task Save<T>(string sourceId, T[] data, TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset endExclusive, HistoricalDataQueryParameters retrieveParameters);
}

