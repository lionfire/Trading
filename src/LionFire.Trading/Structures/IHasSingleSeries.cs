#if cAlgo
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IHasSingleSeries
    {
        MarketSeries MarketSeries { get; }
    }
}
