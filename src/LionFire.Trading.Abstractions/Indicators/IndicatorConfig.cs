using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    

    public class IndicatorConfig : IIndicatorConfig
    {
        public IndicatorConfig() { }
        public IndicatorConfig(string symbolCode, string timeFrame)
        {
            this.Symbol = symbolCode;
            this.TimeFrame = timeFrame;
        }

        public string Symbol { get; set; }
        public string TimeFrame { get; set; } = "h1";
        
        public double SignalThreshold { get; set; } = 0.75;
        public bool Log { get; set; } = false;
    }

}
