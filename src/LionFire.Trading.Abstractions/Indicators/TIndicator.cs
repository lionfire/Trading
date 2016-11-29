using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    

    public class TIndicator : ITIndicator
    {
        public TIndicator() { }
        public TIndicator(string symbolCode, string timeFrame)
        {
            this.Symbol = symbolCode;
            this.TimeFrame = timeFrame;
        }

        public string Symbol { get; set; }
        public string TimeFrame { get; set; } 
        
        public double SignalThreshold { get; set; } = 0.75;
        public bool Log { get; set; } = false;
    }

}
