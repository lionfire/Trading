using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IDoubleDataSeries : ISeries<double>
    {
    }
    public interface ITimeSeries : ISeries<DateTime>
    {
    }

    

    public interface ISeries<T>
    {
        T this[int index] { get; set; }

        int Count { get; }

        T LastValue { get; }

        T Last(int index);

        /// <summary>
        /// Returns int.MinValue if no items
        /// </summary>
        int LastIndex { get; }

        int FirstIndex { get; }

        T UnsetValue { get; }
    }

    public static class ISeriesExtensions
    {
        public static void SetBlank<T>(this ISeries<T> s, int index)
        {
            s[index] = s.UnsetValue;
        }
        public static bool IsUnsetOrDefault<T>(this ISeries<T> s, int index)
        {
            var val = s[index];
            
            return val == null || val.Equals(s.UnsetValue) || val.Equals(default(T));
        }
    }
}
