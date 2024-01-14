namespace LionFire.Trading.HistoricalData.Retrieval;

public interface ITradingDataSource // see also: IBarFileSources
{
    string Name { get; }
    HistoricalDataSourceKind2 SourceType { get; }

}

