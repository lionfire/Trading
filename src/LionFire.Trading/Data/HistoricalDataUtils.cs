using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{
    public static class HistoricalDataUtils
    {
        public static TimeSpan DefaultMaxOutOfDateH1 = TimeSpan.FromHours(12);
        public static TimeSpan DefaultMaxOutOfDateM1 = TimeSpan.FromHours(24);
        public static TimeSpan DefaultMaxOutOfDateT1 = TimeSpan.FromHours(1);

        public static TimeSpan MaxOutOfDateAfterRetrieve { get; set; } = TimeSpan.FromMinutes(2);

        public static TimeSpan? GetDefaultMaxOutOfDate(TimeFrame timeFrame)
        {
            switch (timeFrame.Name)
            {
                case "t1": return DefaultMaxOutOfDateT1;
                case "m1": return DefaultMaxOutOfDateM1;
                case "h1":
                    return DefaultMaxOutOfDateH1;
                default: throw new ArgumentException();
            }
        }
    }
}
