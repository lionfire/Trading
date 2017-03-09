using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{
    public static class HistoricalDataUtils
    {
        public static TimeSpan DefaultMaxOutOfDateD1 = TimeSpan.FromDays(1.95);
        public static TimeSpan DefaultMaxOutOfDateH1 = TimeSpan.FromHours(1.95);
        public static TimeSpan DefaultMaxOutOfDateM1 = TimeSpan.FromMinutes(1.95);
        public static TimeSpan DefaultMaxOutOfDateT1 = TimeSpan.FromSeconds(10);

        public static TimeSpan DefaultBacktestingMaxOutOfDateD1 = TimeSpan.FromDays(12);
        public static TimeSpan DefaultBacktestingMaxOutOfDateH1 = TimeSpan.FromHours(12);
        public static TimeSpan DefaultBacktestingMaxOutOfDateM1 = TimeSpan.FromHours(24);
        public static TimeSpan DefaultBacktestingMaxOutOfDateT1 = TimeSpan.FromHours(1);

        public static TimeSpan MaxOutOfDateAfterRetrieve { get; set; } = TimeSpan.FromMinutes(2);

        public static TimeSpan? GetDefaultMaxOutOfDate(MarketSeriesBase series)
        {
            var bt = series.Account.IsBacktesting;

            switch (series.TimeFrame.TimeFrameUnit)
            {
                case TimeFrameUnit.Tick: return bt ? DefaultBacktestingMaxOutOfDateT1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateT1.TotalSeconds * series.TimeFrame.TimeFrameValue);
                case TimeFrameUnit.Minute: return bt ? DefaultBacktestingMaxOutOfDateM1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateM1.TotalSeconds * series.TimeFrame.TimeFrameValue);
                case TimeFrameUnit.Hour: return bt ? DefaultBacktestingMaxOutOfDateH1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateH1.TotalSeconds * series.TimeFrame.TimeFrameValue);
                case TimeFrameUnit.Day: return bt ? DefaultBacktestingMaxOutOfDateD1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateD1.TotalSeconds * series.TimeFrame.TimeFrameValue);
                
                default: throw new NotImplementedException("Timeframe: "+ series.TimeFrame);
            }
        }
    }
}
