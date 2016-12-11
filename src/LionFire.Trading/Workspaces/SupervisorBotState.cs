using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    [Flags]
    public enum SupervisorBotState
    {
        None = 0,
        LiveBot = 1 << 0,
        Scanner = 1 << 1,
        DemoBot = 1 << 2,
        ShortTermOptimize = 1 << 10,
    }

}
