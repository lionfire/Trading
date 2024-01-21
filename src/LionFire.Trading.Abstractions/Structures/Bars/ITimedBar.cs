﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IMarketDataPoint
    {
        DateTime Time { get; }
    }

    public interface ITimedBar : IMarketDataPoint
    {
        double Open { get; set; }
        double High { get; set; }
        double Low { get; set; }
        double Close { get; set; }
        double Volume { get; set; }
        DateTime OpenTime { get; set; }
    }
}
