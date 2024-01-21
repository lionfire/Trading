
namespace LionFire.Trading.HistoricalData;

public class QueryOptions
{
    //public bool OptimizeForBacktest { get; set; }

    public HistoricalDataSourceKind RetrieveSources { get; set; }

    public static QueryOptions Default { get; set; } = new QueryOptions
    {
        RetrieveSources = HistoricalDataSourceKind.All,
    };

}
