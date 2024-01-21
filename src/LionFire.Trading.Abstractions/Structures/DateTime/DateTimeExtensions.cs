using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public static class DateTimeExtensions
    {
        public static string ToDefaultString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy.MM.dd HH:mm:ss");
        }

        public static string ToCurrencyString(this double val)
        {
            return val.ToString("N2");
        }
        public static string CentsToCurrencyString(this double cents)
        {
            return (cents / 100.0).ToCurrencyString();
        }
    }
}
