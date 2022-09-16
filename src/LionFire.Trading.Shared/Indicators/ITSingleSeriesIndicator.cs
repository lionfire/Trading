using System;
using System.Collections.Generic;
using System.Linq;
#if cAlgo
using cAlgo.API;
#endif
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITSingleSeriesIndicator : ITIndicator
    {
        BarComponent IndicatorBarComponent { get; set; }
        DataSeries IndicatorBarSource { get; set; }

        int Periods { get;  }

    }
}
