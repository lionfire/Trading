using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    internal struct SegmentedBarSeriesComponent
    {

        public DateTime FirstOpenTime;
        public DateTime LastOpenTime;
        public IBarSeries BarSeries;
        public int StartIndex;
        public int LastIndex;
        public bool IsReverse;
    }

    /// <summary>
    /// Use cases: 
    ///  - get historical data for human analysis (and then discard to save memory)
    ///  - get recent history when starting bots that may look back a few hundred bars
    ///  - add to history as time progresses
    /// </summary>
    public class SegmentedBarSeries : IBarSeries
    {
        #region Identity

        public string Code { get; private set; }
        public string TimeFrame { get; private set; }

        #endregion

        #region Fields

        List<SegmentedBarSeriesComponent> components = new List<SegmentedBarSeriesComponent>();

        #endregion

        #region Construction

        public SegmentedBarSeries(string code, string timeFrame)
        {
            this.Code = code;
            this.TimeFrame = timeFrame;
        }

        #endregion

        #region Add

        public void AddSeries(BarSeries bars)
        {
            var x = new SegmentedBarSeriesComponent
            {
                BarSeries = bars,
                FirstOpenTime = bars[0].OpenTime,
                LastOpenTime = bars[bars.Count - 1].OpenTime,
                StartIndex = 0,
                IsReverse = false,

            };
            x.LastIndex = x.StartIndex + (x.IsReverse ? bars.Count : -bars.Count);
            components.Add(x);
            OnComponentsChanged();
        }

        SegmentedBarSeriesComponent? highest;

        private void OnComponentsChanged()
        {
            SegmentedBarSeriesComponent? highest = null;

            foreach (var c in components)
            {
                if (highest == null)
                {
                    highest = c;
                }
                else if (c.LastOpenTime > highest.Value.LastOpenTime)
                {
                    highest = c;
                }
            }
            this.highest = highest;
        }

        public void Add(TimedBarStruct bar)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region (Private) Utils

        private void Validate()
        {
            // TODO - if debug, validate no overlap
        }

        #endregion

        #region (Public) Accessors

        public TimedBarStruct this[int index]
        {
            get
            {
                foreach (var c in components)
                {
                    if (index <= c.LastIndex && index >= c.StartIndex)
                    {
                        var physicalIndex = c.IsReverse ? c.StartIndex - index : index + c.StartIndex;
                        return c.BarSeries[physicalIndex];
                    }
                }
                return default(TimedBarStruct);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                return components.Sum(c => c.BarSeries.Count);
            }
        }

        public TimedBarStruct LastValue
        {
            get
            {
                if (!highest.HasValue) return default(TimedBarStruct);
                return highest.Value.BarSeries.LastValue;
            }
        }

        public TimedBarStruct Last(int index)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
