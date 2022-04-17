using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IDataSourceCollection
    {
        IMarketSeries GetMarketSeries(string seriesKey);
        IMarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame);
    }

    public class DataSourceCollection
    {
        #region Relationships

        public IFeed Feed { get; protected set; }

        #endregion

        #region Configuration

        public bool IsHistorical { get; private set; }

        public List<IDataSource> Sources { get; private set; } = new List<IDataSource>();


        #endregion

        #region Construction

        public DataSourceCollection(bool isHistorical, IFeed feed)
        {
            this.Feed = feed;
            this.IsHistorical = isHistorical;

            if (isHistorical)
            {
                InitHistoricalSources();
            }
        }

        public void InitHistoricalSources()
        {
            //{
            //    var d = new HistoricalDataSource()
            //    {
            //        SourceName = "DukasCopy",
            //        RootDir = @"E:\st\Projects\Investing\Historical Data\dukascopy\",
            //    };
            //    Sources.Add(d);
            //}
            {
                var d = new HistoricalDataSource()
                {
                    SourceName = "DukasCopy (TickDownloader)",
                    RootDir = @"c:\TickDownloader\tickdata\", // HARDCODE HARDPATH  - TODO: move to Trading root and get from options
                };
                Sources.Add(d);
            }
            //{
            //    var d = new HistoricalDataSource()
            //    {
            //        SourceName = "HistData",
            //        RootDir = @"E:\st\Projects\Investing\Historical Data\histdata.com",
            //    };
            //    sources.Add(d);
            //}
        }

        #endregion

        #region State

        internal Dictionary<string, MarketSeries> Dict => dict;

        readonly Dictionary<string, MarketSeries> dict = new();

        #endregion

        #region Accessors

        public MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, DateTime? startDate = null, DateTime? endDate = null)
        {
            return GetMarketSeries(symbol.GetSeriesKey(timeFrame), startDate, endDate);
        }
        public MarketSeries GetMarketSeries(string key, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (dict.ContainsKey(key))
            {
                var result = dict[key];
                
                if (startDate.HasValue && result.OpenTime.Count > 0) {
                    var diff = result.OpenTime[0] - startDate.Value;
                    
                    string symbolCode;
                    TimeFrame timeFrame;
                    MarketSeriesUtilities.DecodeKey(key, out symbolCode, out timeFrame);
                    if (diff > timeFrame.TimeSpan)
                    {
                        // TODO: Try to reload with requested startDate
                        Console.WriteLine($"WARN GetMarketSeries({key}) first data item has open time of {result.OpenTime[0]} but requested start time is {startDate.Value}");
                    }
                    // TODO - same check with endDate
                }
                return result;
            }

            if (IsHistorical)
            {
                foreach (var source in Sources)
                {
                    var series = source.GetMarketSeries(key, startDate, endDate);
                    if (series == null) continue;
                    dict.Add(key, series);
                    return series;
                }
            }
            else
            {
                var split = key.Split(';');
                // TODO: Return null if live data not available
                var series = (MarketSeries) this.Feed.GetMarketSeries(split[0],split[1]); // REVIEW Cast
                dict.Add(key, series);
                return series;
            }

            //foreach (var source in historical ? historicalMarketSeries : Data.Dat)
            //{
            //    var data = source[symbol]?[timeFrame];
            //    if (data != null) { return data; }
            //}
            return null;
        }
        //public IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol)
        //{
        //    var results = new HashSet<string>();
        //    foreach (var source in this.Sources)
        //    {
        //        foreach (var tf in source.GetTimeFramesAvailable(symbol))
        //        {
        //            if (!results.Contains(tf))
        //            {
        //                results.Add(tf);
        //            }
        //        }
        //    }
        //    return results;
        //}

        #endregion

    }
}
