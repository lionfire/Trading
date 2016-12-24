using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public static class MarketSeriesUtilities
    {
        public const char Delimiter = ';';

        public static string GetSeriesKey(this string symbol, TimeFrame timeFrame)
        {
            return symbol + Delimiter.ToString() + timeFrame.Name;
        }

        internal static void DecodeKey(string key, out string symbol, out TimeFrame timeFrame)
        {
            var chunks = key.Split(Delimiter);
            symbol = chunks[0];
            timeFrame = TimeFrame.TryParse(chunks[1]);
        }

    }
}
