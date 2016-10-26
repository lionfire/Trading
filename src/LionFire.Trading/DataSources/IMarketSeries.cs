using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IMarketSeries
    {
        HistoricalPlaybackState HistoricalPlaybackState { get; set; }


        #region Identity

        string Key { get; }
        TimeFrame TimeFrame { get; }
        string SymbolCode { get; }

        #endregion

        #region Relationships

        IDataSource Source { get; }

        #endregion

        #region Data

        //IBarSeries Bars { get; }

        

        TimeSeries OpenTime { get; }
        IDataSeries Open { get; }
        IDataSeries High { get; }
        IDataSeries Low { get; }
        IDataSeries Close { get; }
        IDataSeries TickVolume { get; }

        //IDataSeries Median { get; }
        //IDataSeries Typical { get; }
        //[Obsolete("Use WeightedClose instead")]
        //IDataSeries Weighted { get; }
        //IDataSeries WeightedClose { get; }

        #region Derived

        int Count { get; }

        #endregion

        #endregion

        #region Bar Accessors

        TimedBar LastBar { get; }
        

        int FindIndex(DateTime time);

        TimedBar this[DateTime time] { get; }
        TimedBar this[int index] { get; }

        #endregion

        //IEnumerable<SymbolBar> GetBars(DateTime fromTimeExclusive, DateTime endTimeInclusive);

        IObservable<TimedBar> LatestBar { get; }
        bool LatestBarHasObservers {
            get;
        }

        //event Action<MarketSeries> BarReceived;
        event Action<MarketSeries, double/*bid*/, double/*ask*/> TickReceived;
    }

    internal interface IMarketSeriesInternal : IMarketSeries
    {
        void OnBar(TimedBar bar, bool finishedBar = false);
        void OnTick(DateTime time, double bid, double ask);
    }
}
