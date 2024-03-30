
namespace LionFire.Trading.HistoricalData;

public enum HistoricalDataQueryFlags
{
    Unspecified = 0,
    //FallbackToLongChunkSource = 1,

    ShortChunk = 1 << 10,
    LongChunk = 1 << 11,
}

public class QueryOptions
{
    //public bool OptimizeForBacktest { get; set; }

    public HistoricalDataSourceKind RetrieveSources { get; set; }

    public HistoricalDataQueryFlags Flags { get; set; }

    public static QueryOptions Default { get; set; } = new QueryOptions
    {
        RetrieveSources = HistoricalDataSourceKind.All,
    };

}
