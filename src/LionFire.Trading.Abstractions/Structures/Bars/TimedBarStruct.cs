using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    
    public struct TimedBarStruct : ITimedBar
    {
        DateTime IMarketDataPoint.Time { get { return OpenTime; } }
        public DateTime OpenTime { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public override string ToString()
        {
            var date = OpenTime.ToDefaultString();
            var chars = 8;
            var vol = Volume > 0 ? $" [v:{Volume.ToString().PadLeft(chars)}]" : "";
            return $"{date} o:{Open.ToString().PadRight(chars, '0')} h:{High.ToString().PadRight(chars, '0')} l:{Low.ToString().PadRight(chars, '0')} c:{Close.ToString().PadRight(chars, '0')}{vol}";
        }
    }
}
