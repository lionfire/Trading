using LionFire.Applications;
using LionFire.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{
    public class DataMaxOutOfDateTimeSpans : Dictionary<TimeFrameUnit, double>
    {
        public static TimeSpan DefaultMaxOutOfDateT1 = TimeSpan.FromSeconds(10);
        public static TimeSpan DefaultMaxOutOfDateM1 = TimeSpan.FromMinutes(1.95);
        public static TimeSpan DefaultMaxOutOfDateH1 = TimeSpan.FromHours(1.95);
        public static TimeSpan DefaultMaxOutOfDateD1 = TimeSpan.FromDays(1.95);

        public static TimeSpan DefaultBacktestingMaxOutOfDateT1 = TimeSpan.FromHours(1);
        public static TimeSpan DefaultBacktestingMaxOutOfDateM1 = TimeSpan.FromHours(24);
        public static TimeSpan DefaultBacktestingMaxOutOfDateH1 = TimeSpan.FromHours(12);
        public static TimeSpan DefaultBacktestingMaxOutOfDateD1 = TimeSpan.FromDays(12);

        public static DataMaxOutOfDateTimeSpans Default
        {
            get
            {
                if (defaults == null)
                {
                    defaults = new DataMaxOutOfDateTimeSpans
                    {
                        [TimeFrameUnit.Tick] = DefaultMaxOutOfDateT1.TotalSeconds,
                        [TimeFrameUnit.Minute] = DefaultMaxOutOfDateM1.TotalSeconds,
                        [TimeFrameUnit.Hour] = DefaultMaxOutOfDateH1.TotalSeconds,
                        [TimeFrameUnit.Day] = DefaultMaxOutOfDateD1.TotalSeconds,
                    };
                }
                return defaults;
            }
        }
        private static DataMaxOutOfDateTimeSpans defaults;

        public static DataMaxOutOfDateTimeSpans BacktestingDefault
        {
            get
            {
                if (backtestingDefaults == null)
                {
                    backtestingDefaults = new DataMaxOutOfDateTimeSpans
                    {
                        [TimeFrameUnit.Tick] = DefaultBacktestingMaxOutOfDateT1.TotalSeconds,
                        [TimeFrameUnit.Minute] = DefaultBacktestingMaxOutOfDateM1.TotalSeconds,
                        [TimeFrameUnit.Hour] = DefaultBacktestingMaxOutOfDateH1.TotalSeconds,
                        [TimeFrameUnit.Day] = DefaultBacktestingMaxOutOfDateD1.TotalSeconds,
                    };
                }
                return backtestingDefaults;
            }
        }
        private static DataMaxOutOfDateTimeSpans backtestingDefaults;

    }
    public static class HistoricalDataUtils
    {

        public static TimeSpan MaxOutOfDateAfterRetrieve { get; set; } = TimeSpan.FromMinutes(2);

        public static TimeSpan? GetDefaultMaxOutOfDate(MarketSeriesBase series)
        {
            var bt = (series.HasAccount && series.Account.IsBacktesting) || !App.Get/*todo*/<TradingOptions>().Features.HasAnyFlag(TradingFeatures.Participants);

            double totalSeconds = default(double);

            var settings = App.GetComponent<DataMaxOutOfDateTimeSpans>();
            if (settings != null)
            {
                totalSeconds = settings.TryGetValue(series.TimeFrame.TimeFrameUnit);
            }

            if (totalSeconds == default(double))
            {
                settings = bt ? DataMaxOutOfDateTimeSpans.BacktestingDefault : DataMaxOutOfDateTimeSpans.Default;
                totalSeconds = settings.TryGetValue(series.TimeFrame.TimeFrameUnit);
            }

            if (totalSeconds == default(double))
            {
                throw new NotImplementedException("No settings found for Timeframe: " + series.TimeFrame);
            }

            var result = TimeSpan.FromSeconds(totalSeconds * series.TimeFrame.TimeFrameValue);
            return result;

            //switch (series.TimeFrame.TimeFrameUnit)
            //{
            //    case TimeFrameUnit.Tick: return bt ? DefaultBacktestingMaxOutOfDateT1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateT1.TotalSeconds * series.TimeFrame.TimeFrameValue);
            //    case TimeFrameUnit.Minute: return bt ? DefaultBacktestingMaxOutOfDateM1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateM1.TotalSeconds * series.TimeFrame.TimeFrameValue);
            //    case TimeFrameUnit.Hour: return bt ? DefaultBacktestingMaxOutOfDateH1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateH1.TotalSeconds * series.TimeFrame.TimeFrameValue);
            //    case TimeFrameUnit.Day: return bt ? DefaultBacktestingMaxOutOfDateD1 : TimeSpan.FromSeconds(DefaultMaxOutOfDateD1.TotalSeconds * series.TimeFrame.TimeFrameValue);

            //    default: throw new NotImplementedException("Timeframe: " + series.TimeFrame);
            //}
        }
    }
}
