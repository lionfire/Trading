using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ISingleSeriesIndicator : IHasSingleSeries
    {
        int Periods { get; }
    }
}
