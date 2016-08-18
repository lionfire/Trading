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
}
