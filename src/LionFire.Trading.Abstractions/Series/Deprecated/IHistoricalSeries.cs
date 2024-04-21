using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading;

public interface IHistoricalSeries<T>
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

public static class IHistoricalSeriesX
{
    public static void SetBlank<T>(this IHistoricalSeries<T> s, int index)
    {
        s[index] = s.UnsetValue;
    }
    public static bool IsUnsetOrDefault<T>(this IHistoricalSeries<T> s, int index)
    {
        var val = s[index];
        
        return val == null || val.Equals(s.UnsetValue) || val.Equals(default(T));
    }
}
