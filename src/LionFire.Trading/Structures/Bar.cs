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
        public double High { get; set; } = double.NaN;
        public double Low { get; set; } = double.NaN;
        public double Open { get; set; } = double.NaN;
        public double Close { get; set; } = double.NaN;
        public double Volume { get; set; } = double.NaN;

        public override string ToString()
        {
            var chars = 8;
            var padChar = ' ';
            //var padChar = '0';
            var vol = Volume > 0 ? $" [v:{Volume.ToString().PadLeft(chars)}]" : "";
            return $"o:{Open.ToString().PadRight(chars, padChar)} h:{High.ToString().PadRight(chars, padChar)} l:{Low.ToString().PadRight(chars, padChar)} c:{Close.ToString().PadRight(chars, padChar)}{vol}";
        }


    }
    public static class StringNumberExtensions
    {
        private static string PadNumberWithDecimal(this string str, int chars)
        {
            var padChar = '0';
            if (!str.Contains(".")) str += ".";
            return str.PadRight(chars, padChar);
        }
    }
}
