using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class HistoricalPlaybackState
    {
        #region HistoricalSource

        public IMarketSeries HistoricalSeries {
            get { return historicalSeries; }
            set { historicalSeries = value; NextHistoricalIndex = -1; }
        }
        private IMarketSeries historicalSeries;

        #endregion

        public int NextHistoricalIndex { get; set; } 

        public DateTime NextHistoricalBar { get; set; }
        public TimedBar NextBarInProgress { get; internal set; }
    }
}
