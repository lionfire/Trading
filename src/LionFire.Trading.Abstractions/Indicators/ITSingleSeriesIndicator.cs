using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITSingleSeriesIndicator : ITIndicator
    {
        BarComponent IndicatorBarSource { get; }
    }
}
