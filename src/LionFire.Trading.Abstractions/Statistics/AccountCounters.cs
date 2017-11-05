using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Statistics
{
    public class AccountCounters
    {
        public int Ticks { get; set; }
        public int M1Bars { get; set; }
        public int H1Bars { get; set; }
        public int Other { get; set; }
    }
    public enum StatEventType
    {
        Unspecified,
        Tick,
        M1Bar,
        H1Bar,
        Other,
    }


    public class AccountStats
    {
        public DateTime LastHourTime { get; set; }
        public DateTime LastMinuteTime { get; set; }
        public AccountCounters Totals { get; private set; } = new AccountCounters();
        public AccountCounters LastMinute { get; private set; } = new AccountCounters();
        public AccountCounters LastHour { get; private set; } = new AccountCounters();

        public void Increment(StatEventType type)
        {
            var now = DateTime.UtcNow;

            if (!(now.Minute == LastMinuteTime.Minute && now.Hour == LastMinuteTime.Hour && now.Day == LastMinuteTime.Day && now.Month == LastMinuteTime.Month &&
                        now.Year == LastMinuteTime.Year))
            {
                LastMinuteTime = now;
                LastMinute = new AccountCounters();
            }

            if (!(now.Hour == LastHourTime.Hour && now.Day == LastHourTime.Day && now.Month == LastHourTime.Month &&
                        now.Year == LastHourTime.Year))
            {
                LastHourTime = now;
                LastHour = new AccountCounters();
            }

            switch (type)
            {
                case StatEventType.Tick:
                    Totals.Ticks++;
                    LastMinute.Ticks++;
                    LastHour.Ticks++;
                    break;
                case StatEventType.M1Bar:
                    Totals.M1Bars++;
                    LastMinute.M1Bars++;
                    LastHour.M1Bars++;
                    break;
                case StatEventType.H1Bar:
                    Totals.H1Bars++;
                    LastMinute.H1Bars++;
                    LastHour.H1Bars++;
                    break;
                case StatEventType.Other:
                    Totals.Other++;
                    LastMinute.Other++;
                    LastHour.Other++;
                    break;
                case StatEventType.Unspecified:
                default:
                    break;
            }
        }
    }

}
