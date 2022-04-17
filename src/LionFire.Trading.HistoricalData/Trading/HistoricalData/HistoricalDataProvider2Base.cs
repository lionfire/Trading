using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.HistoricalData
{
    public class HistoricalDataProvider2Options
    {
        public bool UseMemoryCache { get; set; } = true;
    }

    public abstract class HistoricalDataProvider2Base : IHistoricalDataProvider2
    {
        public virtual string SourceId => this.GetType().Name.Replace("Source","").Replace("2","");
        
        void SetDefaults(ref DateTime? endExclusive, ref HistoricalDataQueryParameters? retrieveOptions)
        {
            endExclusive ??= DateTime.UtcNow + TimeSpan.FromHours(1);
            retrieveOptions ??= HistoricalDataQueryParameters.Default;
        }

        #region CanProvide

        public virtual Task<bool> CanGet<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null)
        {
            SetDefaults(ref endExclusive, ref retrieveParameters);
            return CanGet<T>(timeFrame, symbol, start, endExclusive, retrieveParameters);
        }
        protected abstract Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions);

        #endregion

        #region Get

        public virtual async Task<T[]?> Get<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime? endExclusive = null, HistoricalDataQueryParameters? retrieveParameters = null)
        {
            SetDefaults(ref endExclusive, ref retrieveParameters);

            if (!await CanGet<T>(timeFrame, symbol, start, endExclusive, retrieveParameters).ConfigureAwait(false)) { return null; }

            return await GetImpl<T>(timeFrame, symbol, start, endExclusive!.Value, retrieveParameters!).ConfigureAwait(false);
        }

        

        protected abstract Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions);

        #endregion

    }
}
