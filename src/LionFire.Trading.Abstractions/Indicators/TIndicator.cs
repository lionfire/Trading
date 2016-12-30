using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    //public class TIndicator : ITIndicator
    //{
    //    public TIndicator() { }
    //}

    public class TSingleSeriesIndicator : ITIndicator, ITSingleSeriesIndicator
    {
        public TSingleSeriesIndicator() { }
        public TSingleSeriesIndicator(string symbolCode, string timeFrame)
        {
            this.Symbol = symbolCode;
            this.TimeFrame = timeFrame;
        }

        public string Symbol { get; set; }
        public string TimeFrame { get; set; }
        public BarComponent IndicatorBarSource { get; set; } = BarComponent.Close;

        public double SignalThreshold { get; set; } = 0.75; // MOVE to Signal Indicator?
        public bool Log { get; set; } = false;
    }


}
