using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Execution.Jobs;
using System.Diagnostics;

namespace LionFire.Trading
{

    public interface IMarketDataProvider
    {
        MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, bool historical = false);
    }

    public class MarketDataProvider : IMarketDataProvider
    {
        #region Relationships

        IFeed_Old market;

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

        //public void Subscribe(AccountParticipant actor, MarketDataSubscriptions sub)
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

        public MarketDataProvider(IFeed_Old market)
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
            else
            {
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

        public async Task EnsureDataAvailable(MarketSeriesBase marketSeries, DateTimeOffset? startDate, DateTimeOffset endDate, int totalDesiredBars, bool forceRetrieve = false)
        {
            await marketSeries.Feed.HistoricalDataProvider.GetData(marketSeries, startDate, endDate, totalDesiredBars: totalDesiredBars, forceRetrieve: forceRetrieve).ConfigureAwait(false);

            //if (LoadHistoricalDataAction == null) throw new NotImplementedException();

            //var job = LoadHistoricalDataAction(marketSeries, startDate, endDate, minBars);
            //Debug.WriteLine($"JOB {job.GetHashCode()} ");
            //job = marketSeries.LoadDataJobs.EnqueueOrGet(job);
            //job.Start();
            //return job.RunTask;
        }

        //public Func<MarketSeriesBase, DateTime?, DateTime, int, IJob> LoadHistoricalDataAction;

        #endregion
    }
}
