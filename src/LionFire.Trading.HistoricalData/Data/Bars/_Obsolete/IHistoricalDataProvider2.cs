using System.Buffers;
using System.Threading.Tasks;

namespace LionFire.Trading.HistoricalData;

[Obsolete("Migrate to IBars")]
public interface IHistoricalDataProvider2
{
    Task<bool> CanGet<T>(TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null);

    Task<T[]?> Get<T>(TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null);
}

public static class IHistoricalDataProvider2Extensions
{
    public static Task<OhlcvItem[]?> GetOhlcv(this IHistoricalDataProvider2 hdp, TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null)
        => hdp.Get<OhlcvItem>(timeFrame, symbol, start, endExclusive, retrieveParameters);
    public static Task<OhlcDoubleItem[]?> GetOhlcDouble(this IHistoricalDataProvider2 hdp, TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null)
        => hdp.Get<OhlcDoubleItem>(timeFrame, symbol, start, endExclusive, retrieveParameters);

}
