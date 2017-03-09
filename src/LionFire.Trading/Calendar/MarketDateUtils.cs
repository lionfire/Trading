using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Times
{
    public static class MarketDateUtils
    {
        public static int MarketOpenHour = 21;
        public static int MarketCloseHour = 22;

        public static int GetMarketHourDays(DateTime startDate, DateTime? endDate)
        {
            int result = 0;
            if (!endDate.HasValue) endDate = startDate;

            for (DateTime date = startDate; date <= endDate; date += TimeSpan.FromDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday) continue;

                if (date.Month == 12)
                {
                    if (date.Day == 25 || date.Day == 31) continue;
                }
                if (date.Month == 1)
                {
                    if (date.Day == 1 || date.Day == 2) continue;
                }
                result++;
            }
            return result;
        }
    }
}
