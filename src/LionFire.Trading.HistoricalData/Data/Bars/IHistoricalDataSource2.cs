
namespace LionFire.Trading.HistoricalData;

public interface IHistoricalDataSource2 : IHistoricalDataProvider2
{
    string Name { get; }

    HistoricalDataSourceKind Kind { get; }
}

