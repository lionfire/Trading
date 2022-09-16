#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
using TimeFrame = cAlgo.API.TimeFrame;
using Symbol = cAlgo.API.Internals.Symbol;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ISingleSeriesConfig
    {
        TimeFrame TimeFrame { get; }

        Symbol Symbol { get; }

    }
}
