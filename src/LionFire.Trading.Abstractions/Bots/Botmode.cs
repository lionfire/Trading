using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    [Flags]
    public enum BotMode
    {
        None = 0,
        Live = 1 << 0,
        Demo = 1 << 1,
        Scanner = 1 << 2,
        Paper = 1 << 3,
    }
}
