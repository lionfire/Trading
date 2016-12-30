using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITIndicator : ITemplate
    {
        string Symbol { get; set; } // Move to ITSingleSeriesIndicator?
        string TimeFrame { get; set; } // Move to ITSingleSeriesIndicator?

        bool Log { get; set; }

        double SignalThreshold { get; set; } // // Move to ITSignalIndicator?
    }
    
}
