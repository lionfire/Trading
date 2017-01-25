using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class TSignalIndicator : TSingleSeriesIndicator, ITSignalIndicator
    {

        #region Construction


        public TSignalIndicator() { }
        public TSignalIndicator(string symbolCode, string timeFrame) : base(symbolCode, timeFrame)
        {
        }

        #endregion

        //public MarketSeries MarketSeries { get; set; }

        public double PointsToOpenLong { get; set; } = 1.0;
        public double PointsToOpenShort { get; set; } = 1.0;
        public double PointsToCloseLong { get; set; } = 1.0;
        public double PointsToCloseShort { get; set; } = 1.0;

        [Obsolete]
        public double PointsToClose { get; set; } = 1.0;

    }
}
