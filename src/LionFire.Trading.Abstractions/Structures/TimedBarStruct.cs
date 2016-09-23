using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public struct TimedBarStruct
    {
        public DateTime OpenTime;
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public double Volume;

        public override string ToString()
        {
            var date = OpenTime.ToDefaultString();
            var chars = 8;
            var vol = Volume > 0 ? $" [v:{Volume.ToString().PadLeft(chars)}]" : "";
            return $"{date} o:{Open.ToString().PadRight(chars, '0')} h:{High.ToString().PadRight(chars, '0')} l:{Low.ToString().PadRight(chars, '0')} c:{Close.ToString().PadRight(chars, '0')}{vol}";
        }
    }
}
