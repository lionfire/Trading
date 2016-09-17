using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{

    public interface ISignalIndicatorConfig : IIndicatorConfig
    {
        double PointsToOpenLong { get; set; }
        double PointsToOpenShort { get; set; }
        double PointsToCloseLong { get; set; }
        double PointsToCloseShort { get; set; }

        [Obsolete]
        double PointsToClose { get; set; }
    }

    public class SignalIndicatorConfig : IndicatorConfig, ISignalIndicatorConfig
    {

        #region Construction


        public SignalIndicatorConfig() { }
        public SignalIndicatorConfig(string symbolCode, string timeFrame) : base(symbolCode, timeFrame)
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
