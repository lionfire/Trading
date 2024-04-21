using LionFire.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IndicatorDataSeries : IDoubleDataSeries
    {
    }
    /*
    public sealed class BarSeries :
        DataSeries<TimedBar>, IBarSeries

    {
        TimedBar IHistoricalSeries<TimedBar>.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public TimedBar UnsetValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        TimedBar IHistoricalSeries<TimedBar>.LastValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        TimedBar IHistoricalSeries<TimedBar>.Last(int index)
        {
            throw new NotImplementedException();
        }
    }
    */

    public sealed class BarSeriesAdapter : IBarSeries, INotifyOnLoaded, INotifyCollectionChanged
    {
        public MarketSeries MarketSeries { get; set; }
        public BarSeriesAdapter(MarketSeries marketSeries) { this.MarketSeries = marketSeries; }

        public TimedBar UnsetValue { get { return TimedBar.Invalid; } }

        public TimedBar LastValue
        {
            get
            {
                return new TimedBar
                {
                    OpenTime = MarketSeries.OpenTime.LastValue,
                    Open = MarketSeries.Open.LastValue,
                    High = MarketSeries.High.LastValue,
                    Low = MarketSeries.Low.LastValue,
                    Close = MarketSeries.Close.LastValue,
                    Volume = MarketSeries.TickVolume.LastValue,
                };
            }
        }

        public int Count
        {
            get
            {
                return MarketSeries.Count;
            }
        }

        public int LastIndex
        {
            get
            {
                return MarketSeries.LastIndex;
            }
        }

        public int FirstIndex
        {
            get
            {
                return MarketSeries.FirstIndex;
            }
        }

        public TimedBar this[int index]
        {
            get
            {
                return new TimedBar
                {
                    OpenTime = MarketSeries.OpenTime[index],
                    Open = MarketSeries.Open[index],
                    High = MarketSeries.High[index],
                    Low = MarketSeries.Low[index],
                    Close = MarketSeries.Close[index],
                    Volume = MarketSeries.TickVolume[index],
                };
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public TimedBar Last(int index)
        {
            return this[LastIndex - index];  // REVIEW - is this correct? TOVERIFY
        }

        public IEnumerator<TimedBar> GetEnumerator()
        {
            for (int i = FirstIndex; i < LastIndex; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OnLoaded(object persistenceContext = null)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public sealed class DataSeries : DataSeries<double>, IDoubleDataSeries, IndicatorDataSeries
    {
        public double UnsetValue { get { return double.NaN; } }
    }


}
