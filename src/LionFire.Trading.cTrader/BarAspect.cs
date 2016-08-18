using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum BarAspect
    {
        Unspecified = 0,
        Open = 1 << 0,
        High = 1 << 1,
        Low = 1 << 2,
        Close = 1 << 3,
    }
}
