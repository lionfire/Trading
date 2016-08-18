
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class MarketDataProvider
    {
        #region Configuration

        public DataSourceCollection HistoricalDataSources { get; private set; } = new DataSourceCollection(true);
        public DataSourceCollection LiveDataSources { get; private set; } = new DataSourceCollection(false);

        public List<HistoricalDataSource> HistoricalDataSourcesList { get { return historicalDataSources; } }
        List<HistoricalDataSource> historicalDataSources = new List<HistoricalDataSource>();

        #endregion

        internal IEnumerable<IMarketSeries> ActiveLiveSeries {
            get {
                foreach (var series in LiveDataSources.Dict)
                {
                    // TODO OPTIMIZE UNLOAD: Allow deactivate
                    yield return series.Value;
                }
            }
        }

        #region Subscription

        //Dictionary<string, MarketDataSubscriptions> subscriptions = new Dictionary<string, MarketDataSubscriptions>();

        //public void Subscribe(Actor actor, MarketDataSubscriptions sub)
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

        IMarket market;

        #region Construction

        public MarketDataProvider(IMarket market)
        {
            this.market = market;
        }

        #endregion

        #region Accessors

        public IMarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, bool historical = false)
        {
            if (historical) { return this.HistoricalDataSources.GetMarketSeries(symbol, timeFrame); }
            else { return this.LiveDataSources.GetMarketSeries(symbol, timeFrame); }
        }

        public IEnumerable<string> HistoricalSymbolsAvailable {
            get {
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
