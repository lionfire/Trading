
namespace LionFire.Trading.HistoricalData;

public interface IHistoricalDataSource2 : IHistoricalDataProvider2
{
    string SourceId { get; }

    HistoricalDataSourceKind Kind { get; }
}

