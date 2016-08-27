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
