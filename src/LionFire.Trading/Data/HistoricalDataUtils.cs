using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{
    public static class HistoricalDataUtils
    {
        public static TimeSpan DefaultMaxOutOfDateH1 = TimeSpan.FromHours(1.95);
        public static TimeSpan DefaultMaxOutOfDateM1 = TimeSpan.FromMinutes(1.95);
        public static TimeSpan DefaultMaxOutOfDateT1 = TimeSpan.FromSeconds(10);

        public static TimeSpan DefaultBacktestingMaxOutOfDateH1 = TimeSpan.FromHours(12);
        public static TimeSpan DefaultBacktestingMaxOutOfDateM1 = TimeSpan.FromHours(24);
        public static TimeSpan DefaultBacktestingMaxOutOfDateT1 = TimeSpan.FromHours(1);

        public static TimeSpan MaxOutOfDateAfterRetrieve { get; set; } = TimeSpan.FromMinutes(2);

        public static TimeSpan? GetDefaultMaxOutOfDate(MarketSeriesBase series)
        {
            var bt = series.Account.IsBacktesting;
            
            switch (series.TimeFrame.Name)
            {
                case "t1": return bt ? DefaultBacktestingMaxOutOfDateT1 : DefaultMaxOutOfDateT1;
                case "m1": return bt ? DefaultBacktestingMaxOutOfDateM1 : DefaultMaxOutOfDateM1;
                case "h1":
                    return bt ? DefaultBacktestingMaxOutOfDateH1 : DefaultMaxOutOfDateH1;
                default: throw new ArgumentException();
            }
        }
    }
}
