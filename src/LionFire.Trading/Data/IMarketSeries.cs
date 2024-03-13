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
        DataSeries Open { get; }
        DataSeries High { get; }
        DataSeries Low { get; }
        DataSeries Close { get; }
        DataSeries TickVolume { get; }

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

        TimedBar Last { get; }
        

        int FindIndex(DateTimeOffset time, bool loadHistoricalData = false);

        TimedBar this[DateTimeOffset time] { get; }
        TimedBar this[int index] { get; }

        #endregion

        //IEnumerable<SymbolBar> GetBars(DateTimeOffset fromTimeExclusive, DateTimeOffset endTimeInclusive);

        event Action<TimedBar> Bar;
        //event Action<SymbolBar> SymbolBar;
        bool BarHasObservers { get; }

        IObservable<TimedBar> LatestBar { get; }
        bool LatestBarHasObservers {
            get;
        }

        //event Action<MarketSeries> BarReceived;

    }

    public interface IMarketSeriesInternal : IMarketSeries
    {
        void OnBar(TimedBar bar, bool finishedBar = false);

        event Action<IMarketSeries, bool> BarHasObserversChanged;
    }
}
