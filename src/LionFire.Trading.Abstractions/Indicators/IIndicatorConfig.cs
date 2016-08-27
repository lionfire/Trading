using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public interface IIndicatorConfig
    {
        string Symbol { get; set; }
        string TimeFrame { get; set; }

        bool Log { get; set; }

        double SignalThreshold { get; set; }
    }
}
