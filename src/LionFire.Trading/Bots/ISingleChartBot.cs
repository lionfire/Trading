#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public interface ISingleChartBot : IBot, ISingleSeriesConfig, IHasSingleSeries
    {
    }    
}
