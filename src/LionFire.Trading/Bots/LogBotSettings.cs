using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class LogBotSettings
    {
        public LogBotSettings Instance { get { return LionFire.Structures.Singleton<LogBotSettings>.Instance;  } }
        public int LogEveryXBar { get; set; } = 111;
    }
}
