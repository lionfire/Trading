using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace LionFire.Trading
{


    public class TimeFrame
    {

        #region Static

        static TimeFrame()
        {
            m1 = new TimeFrame("m1");
            h1 = new TimeFrame("h1");
            h2 = new TimeFrame("h2");
            h4 = new TimeFrame("h4");
        }

        public static TimeFrame m1 { get; private set; }
        public static TimeFrame h1 { get; private set; }
        public static TimeFrame h2 { get; private set; }
        public static TimeFrame h4 { get; private set; }

        #endregion

        #region Construction

        public TimeFrame(string name)
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

        public static TimeFrame TryParse(string v)
        {
            var pi = typeof(TimeFrame).GetProperty(v, BindingFlags.Public | BindingFlags.Static);
            return pi.GetValue(null) as TimeFrame;
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

        public TimeSpan TimeSpan { // TODO - Init this readonly during construction
            get {
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

        #endregion
    }


}
