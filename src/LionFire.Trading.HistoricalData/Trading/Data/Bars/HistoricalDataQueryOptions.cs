
namespace LionFire.Trading.HistoricalData;
public class HistoricalDataQueryOptions
{
    public bool OptimizeForBacktest { get; set; }

    public HistoricalDataSourceKind RetrieveSources { get; set; }

    public static HistoricalDataQueryOptions Default { get; set; } = new HistoricalDataQueryOptions
    {
        RetrieveSources = HistoricalDataSourceKind.All,
    };

}
