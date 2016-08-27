using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum TimeFrameUnit
    {
        Unspecified = 0,
        Tick = 2,
        Second = 1,
        Minute = 1 * 60,
        Hour = 60 * 60,
        Day = 1440 * 60,
        Week = 10080 * 60,
        Month = 44640 * 60,
        Year = 1440 * 60 * 365,

    }

    public static class TimeFrameUnitExtensions
    {
        public static string ToLetterCode(this TimeFrameUnit unit)
        {
            switch (unit)
            {
                //case TimeFrameUnit.Unspecified:
                //    break;
                case TimeFrameUnit.Tick:
                    return "t";
                case TimeFrameUnit.Second:
                    return "s";
                case TimeFrameUnit.Minute:
                    return "m";
                case TimeFrameUnit.Hour:
                    return "h";
                case TimeFrameUnit.Day:
                    return "d";
                case TimeFrameUnit.Week:
                    return "w";
                case TimeFrameUnit.Month:
                    return "M";
                case TimeFrameUnit.Year:
                    return "y";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static TimeFrameUnit MoreGranular(this TimeFrameUnit unit)
        {
            switch (unit)
            {
                case TimeFrameUnit.Unspecified:
                    return TimeFrameUnit.Unspecified;
                case TimeFrameUnit.Tick:
                    return TimeFrameUnit.Unspecified;
                case TimeFrameUnit.Second:
                    return TimeFrameUnit.Tick;
                case TimeFrameUnit.Minute:
                    return TimeFrameUnit.Second;
                case TimeFrameUnit.Hour:
                    return TimeFrameUnit.Minute;
                case TimeFrameUnit.Day:
                    return TimeFrameUnit.Hour;
                case TimeFrameUnit.Week:
                    return TimeFrameUnit.Day;
                case TimeFrameUnit.Month:
                    return TimeFrameUnit.Week;
                case TimeFrameUnit.Year:
                    return TimeFrameUnit.Month;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
