using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if cAlgo
//using Timerame = LionFire.Trading.LionFireTimeFrame;
using cAlgo.API;
#endif
using System.Threading.Tasks;


namespace LionFire.Trading
{

#if !cAlgo

#if cAlgo
    public class LionFireTimeFrame
#else
    public class TimeFrame
#endif
    {

    #region Static

#if cAlgo
        static LionFireTimeFrame()
#else
        static TimeFrame()
#endif
        {
            t1 = new TimeFrame("t1");
            m1 = new TimeFrame("m1");
            h1 = new TimeFrame("h1");
            h2 = new TimeFrame("h2");
            h4 = new TimeFrame("h4");
        }

        public static TimeFrame t1 { get; private set; }
        public static TimeFrame m1 { get; private set; }
        public static TimeFrame h1 { get; private set; }
        public static TimeFrame h2 { get; private set; }
        public static TimeFrame h4 { get; private set; }

    #endregion

    #region Construction

#if cAlgo
        public LionFireTimeFrame(string name)
#else
                public TimeFrame(string name)
#endif
        {
            this.Name = name;
            string type = name[0].ToString();

            switch (type)
            {
                case "t":
                    TimeFrameUnit = TimeFrameUnit.Tick;
                    break;
                case "s":
                    TimeFrameUnit = TimeFrameUnit.Second;
                    break;
                case "m":
                    TimeFrameUnit = TimeFrameUnit.Minute;
                    break;
                case "h":
                    TimeFrameUnit = TimeFrameUnit.Hour;
                    break;
                case "d":
                    TimeFrameUnit = TimeFrameUnit.Day;
                    break;
                case "w":
                    TimeFrameUnit = TimeFrameUnit.Week;
                    break;
                case "M":
                    TimeFrameUnit = TimeFrameUnit.Month;
                    break;
                default:
                    break;
            }

            TimeFrameValue = Int32.Parse(name.Substring(1));
        }

        public static TimeFrame TryParse(string timeFrameCode)
        {
            if (string.IsNullOrWhiteSpace(timeFrameCode)) return null;

            var pi = typeof(TimeFrame).GetProperty(timeFrameCode, BindingFlags.Public | BindingFlags.Static);
            return pi?.GetValue(null) as TimeFrame;
        }

        public static implicit operator TimeFrame(string timeFrameCode)
        {
            return TryParse(timeFrameCode);
        }

    #endregion

    #region Operators

        public static implicit operator string(TimeFrame tf)
        {
            return tf.Name;
        }

    #endregion

    #region Properties

        public string Name { get; set; }

        public TimeFrameUnit TimeFrameUnit { get; set; }

        public int TimeFrameValue { get; set; }

    #endregion

    #region Derived

        public TimeSpan TimeSpan
        { // TODO - Init this readonly during construction
            get
            {
                if (timeSpan == default(TimeSpan))
                {
                    switch (TimeFrameUnit)
                    {
                        case TimeFrameUnit.Tick:
                            timeSpan = TimeSpan.Zero;
                            break;
                        case TimeFrameUnit.Minute:
                            timeSpan = TimeSpan.FromMinutes(TimeFrameValue);
                            break;
                        case TimeFrameUnit.Hour:
                            timeSpan = TimeSpan.FromHours(TimeFrameValue);
                            break;
                        case TimeFrameUnit.Day:
                            timeSpan = TimeSpan.FromDays(TimeFrameValue);
                            break;
                        case TimeFrameUnit.Week:
                            timeSpan = TimeSpan.FromDays(TimeFrameValue * 7);
                            break;
                        case TimeFrameUnit.Month:
                            timeSpan = TimeSpan.Zero;
                            break;
                        default:
                            throw new ArgumentNullException("TimeFrameUnit");
                    }
                }
                return timeSpan;
            }
        }
        private TimeSpan timeSpan;



    #endregion


    #region Misc

        public override string ToString()
        {
            return Name;
        }


        internal DateTime Round(DateTime value)
        {
            if (this.TimeFrameValue != 1 && TimeFrameUnit != TimeFrameUnit.Tick)
            {
                Console.WriteLine($"WARN: Round not fully implemented for {this}");
            }

            switch (this.TimeFrameUnit)
            {
                case TimeFrameUnit.Tick:
                    return value;
                case TimeFrameUnit.Second:
                    return value.RoundMillisecondsToZero();
                case TimeFrameUnit.Minute:
                    return value.RoundSecondsToZero();
                case TimeFrameUnit.Hour:
                    return value.RoundMinutesToZero();
                case TimeFrameUnit.Day:
                    return value.RoundHoursToZero();
                case TimeFrameUnit.Week:
                    throw new NotImplementedException();
                case TimeFrameUnit.Month:
                    return value.RoundDaysToOne();
                case TimeFrameUnit.Year:
                    return value.RoundMonthsToOne();
                default:
                    throw new NotImplementedException();
            }
        }

        // FUTURE: Get factors of a timeframe
        public TimeFrame MoreGranular(bool factorOfTwo = true)
        {
            var val = TimeFrameValue - 1;

            for (TimeFrameUnit unit = TimeFrameUnit; unit != TimeFrameUnit.Unspecified; unit = unit.MoreGranular())
            {
                for (; val >= 1;)
                {
                    var tf = TimeFrame.TryGet(unit, val);
                    if (tf != null) return tf;
                    if (factorOfTwo)
                    {
                        val /= 2;
                    }
                    else
                    {
                        val--;
                    }
                }
                val = Math.Max(1, val);
            }
            return null;
        }

        public static TimeFrame TryGet(TimeFrameUnit unit, int val)
        {
            return TryParse(unit.ToLetterCode() + val.ToString());
        }

    #endregion
    }
#endif

    public static class TimeFrameExtensions
    {
        public static int GetBarsFromMinutes(this TimeFrame timeFrame, double minutes)
        {
            return (int)timeFrame._GetBarsFromMinutes(minutes);
        }
        private static double _GetBarsFromMinutes(this TimeFrame timeFrame, double minutes)
        {
#if !cAlgo
#else
#endif
            switch (timeFrame.ToShortString())
            {
                case "m":
                case "m1":
                    return minutes;
                case "m2":
                    return minutes / 2.0;
                case "m3":
                    return minutes / 3.0;
                case "m4":
                    return minutes / 4.0;
                case "m5":
                    return minutes / 5.0;
                case "m6":
                    return minutes / 6.0;
                case "m7":
                    return minutes / 7.0;
                case "m8":
                    return minutes / 8.0;
                case "m9":
                    return minutes / 9.0;
                case "m10":
                    return minutes / 10.0;
                case "m15":
                    return minutes / 15.0;
                case "m20":
                    return minutes / 20.0;
                case "m30":
                    return minutes / 30.0;
                case "m45":
                    return minutes / 45.0;
                case "h1":
                case "h":
                    return minutes / 60.0;
                case "h2":
                    return minutes / (60.0 * 2);
                case "h4":
                    return minutes / (60.0 * 4);
                case "h6":
                    return minutes / (60.0 * 6);
                case "h8":
                    return minutes / (60.0 * 8);
                case "h10":
                    return minutes / (60.0 * 10);
                case "h12":
                    return minutes / (60.0 * 12);
                case "h16":
                    return minutes / (60.0 * 16);
                case "h18":
                    return minutes / (60.0 * 18);
                case "d":
                case "d1":
                    return minutes / (60.0 * 24);
                case "d2":
                    return minutes / (60.0 * 24 * 2);
                case "d3":
                    return minutes / (60.0 * 24 * 3);
                case "w":
                case "w1":
                    return minutes / (60.0 * 24 * 7);
            }
            throw new NotImplementedException(timeFrame.ToString());
        }



        public static string ToShortString(this TimeFrame tf)
        {
            var str = tf.ToString();
#if cAlgo
            str = str.Replace("Day", "d");
            str = str.Replace("Hour", "h");
            str = str.Replace("Minute", "m");
            str = str.Replace("Tick", "t");
            if (str.Length == 1) { str += "1"; }
#endif
            return str;

        }


    }

}
