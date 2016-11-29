using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public interface IMarketDataProvider
    {
        MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, bool historical = false);
    }

    public class MarketDataProvider : IMarketDataProvider
    {
        #region Relationships

        IAccount market;

        #endregion

        #region Configuration

        public DataSourceCollection HistoricalDataSources { get; private set; }
        public DataSourceCollection LiveDataSources { get; private set; }

        public List<HistoricalDataSource> HistoricalDataSourcesList { get { return historicalDataSources; } }

        List<HistoricalDataSource> historicalDataSources = new List<HistoricalDataSource>();

        #endregion

        internal IEnumerable<IMarketSeries> ActiveLiveSeries
        {
            get
            {
                foreach (var series in LiveDataSources.Dict)
                {
                    // TODO OPTIMIZE UNLOAD: Allow deactivate
                    yield return series.Value;
                }
            }
        }
        internal IEnumerable<IMarketSeries> ActiveHistoricalSeries
        {
            get
            {
                foreach (var series in HistoricalDataSources.Dict)
                {
                    // TODO OPTIMIZE UNLOAD: Allow deactivate
                    yield return series.Value;
                }
            }
        }

        #region Subscription

        //Dictionary<string, MarketDataSubscriptions> subscriptions = new Dictionary<string, MarketDataSubscriptions>();

        //public void Subscribe(MarketParticipant actor, MarketDataSubscriptions sub)
        //{
        //    MarketDataSubscriptions sub;
        //    if (subscriptions.Contains(sub.Key))
        //    {
        //        sub = subscriptions[sub.Key];
        //    }
        //    else
        //    {
        //        sub = new MarketDataSubscriptions()
        //        {

        //        };
        //        subscriptions.Add(sub.Key, sub);
        //    }
        //}

        #endregion

        #region Construction

        public MarketDataProvider(IAccount market)
        {
            this.market = market;
            HistoricalDataSources = new DataSourceCollection(true, market);
            LiveDataSources = new DataSourceCollection(false, market);
        }

        #endregion

        #region Accessors

        public MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, bool historical = false)
        {
            if (historical) { return this.HistoricalDataSources.GetMarketSeries(symbol, timeFrame); }
            else {
                return this.LiveDataSources.GetMarketSeries(symbol, timeFrame);
            }
        }
        public MarketSeries GetMarketSeries(Symbol symbol, TimeFrame timeFrame, bool historical = false)
        {
            return GetMarketSeries(symbol.Code, timeFrame.Name, historical);
        }

        public IEnumerable<string> HistoricalSymbolsAvailable
        {
            get
            {
                var results = new HashSet<string>();
                foreach (var source in HistoricalDataSourcesList)
                {
                    foreach (var symbol in source.SymbolsAvailable)
                    {
                        if (results.Contains(symbol)) continue;
                        results.Add(symbol);
                        yield return symbol;
                    }
                }
            }
        }

        #endregion
    }
}
