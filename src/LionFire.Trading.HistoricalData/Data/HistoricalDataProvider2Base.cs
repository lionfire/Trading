using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.HistoricalData;

[Obsolete]
public class HistoricalDataProvider2Options
{
    public bool UseMemoryCache { get; set; } = true;
}

[Obsolete]
public abstract class HistoricalDataProvider2Base : IHistoricalDataProvider2
{
    public virtual string Name => this.GetType().Name.Replace("Source","").Replace("2","");
    
    void SetDefaults(ref DateTimeOffset? endExclusive, ref HistoricalDataQueryParameters? retrieveOptions)
    {
        endExclusive ??= DateTimeOffset.UtcNow + TimeSpan.FromHours(1);
        retrieveOptions ??= HistoricalDataQueryParameters.Default;
    }

    #region CanProvide

    public virtual Task<bool> CanGet<T>(TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null)
    {
        SetDefaults(ref endExclusive, ref retrieveParameters);
        return CanGet<T>(timeFrame, symbol, start, endExclusive, retrieveParameters);
    }
    protected abstract Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTimeOffset Start, DateTimeOffset endExclusive, HistoricalDataQueryParameters retrieveOptions);

    #endregion

    #region Get

    public virtual async Task<T[]?> Get<T>(TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null)
    {
        SetDefaults(ref endExclusive, ref retrieveParameters);

        if (!await CanGet<T>(timeFrame, symbol, start, endExclusive, retrieveParameters).ConfigureAwait(false)) { return null; }

        return await GetImpl<T>(timeFrame, symbol, start, endExclusive!.Value, retrieveParameters!).ConfigureAwait(false);
    }

    

    protected abstract Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTimeOffset start, DateTimeOffset endExclusive, HistoricalDataQueryParameters retrieveOptions);

    #endregion

}
