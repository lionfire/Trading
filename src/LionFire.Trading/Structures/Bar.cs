using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    //public class SymbolTimedBar : TimedBar
    //{
    //    public string SymbolCode { get; set; }
    //}

    public class Bar
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public override string ToString()
        {
            var chars = 8;
            var vol = Volume > 0 ? $" [v:{Volume.ToString().PadLeft(chars)}]" : "";
            return $"o:{Open.ToString().PadRight(chars, '0')} h:{High.ToString().PadRight(chars, '0')} l:{Low.ToString().PadRight(chars, '0')} c:{Close.ToString().PadRight(chars, '0')}{vol}";
        }
    }
}
