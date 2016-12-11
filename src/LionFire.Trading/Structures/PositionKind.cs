using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum PositionKind
    {
        Unspecified = 0,
        Live = 1 << 0,
        Demo = 1 << 1,
        Paper = 1 << 2,

    }
}
