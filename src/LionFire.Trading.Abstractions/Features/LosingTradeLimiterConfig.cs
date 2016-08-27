using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    // TOIMPLEMENT
    public class LosingTradeLimiterConfig
    {
        public double MaxLosingTradePercent { get; set; } = 0.9;
        public double MaxLosingTradeWindowMagnitude { get; set; } = 10;

        /// <summary>
        /// Leave null for bot default timeframe
        /// </summary>
        public ITimeFrame MaxLosingTradeWindowUnit { get; set; } = null;
        public double MaxLosingTradeMinThreshold { get; set; } = 5;

    }
}
