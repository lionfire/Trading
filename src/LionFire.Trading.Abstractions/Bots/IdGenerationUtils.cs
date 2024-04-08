using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots;


//http://stackoverflow.com/a/19271062/208304
public static class StaticRandom
{
    static int seed = Environment.TickCount;

    static readonly ThreadLocal<Random> random =
        new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

    public static int Next(int minValue, int maxValue)
    {
        return random.Value!.Next(minValue, maxValue);
    }
}

public static class IdUtils
{
    public static int DefaultIdLength = 12;


    public static string GenerateId(int length = 0)
    {
        if (length == 0) length = DefaultIdLength;

        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            var n = StaticRandom.Next(0, 36);
            if (n <= 25)
            {
                chars[i] = (char)((int)'a' + n);
            }
            else
            {
                chars[i] = (char)((int)'0' + n - 26);
            }
        }
        return new string(chars);
    }
}
