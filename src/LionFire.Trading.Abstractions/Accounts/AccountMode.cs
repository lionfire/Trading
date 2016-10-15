using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    [Flags]
    public enum AccountMode
    {
        Unspecified = 0,
        Demo = 1 << 0,
        Live = 1 << 1,
        Any = Demo | Live,
    }
}
