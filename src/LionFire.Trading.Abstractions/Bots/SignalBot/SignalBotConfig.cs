using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class SignalBotConfig : BotConfig
    {
        public double PointsToLong { get; set; } = 1.0;
        public double PointsToShort { get; set; } = 1.0;


        

    }
}
