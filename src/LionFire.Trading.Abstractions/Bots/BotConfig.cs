using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class BotConfig
    {
        public bool Log { get; set; } = true;

        public bool UseTicks { get; set; } = false;

        /// <summary>
        /// In units of quantity, not volume
        /// </summary>
        public long MinPositionSize { get; set; }

        public bool ScalePositionSizeWithEquity { get; set; } = true;

        ///// <summary>
        ///// Uses Equity balance but Symbol pricing -- may use different currencies!!
        ///// </summary>
        //public double PositionRiskPricePercent { get; set; }

        public double PositionRiskPercent { get; set; }

        public bool AllowLong { get; set; } = true;
        public bool AllowShort { get; set; } = true;

        /// <summary>
        /// 0 or MaxValue means no limit
        /// </summary>
        public int MaxOpenPositions { get; set; } = int.MaxValue;
        public int MaxLongPositions { get; set; } = 1;
        public int MaxShortPositions { get; set; } = 1;

        public double BacktestProfitTPMultiplierOnSL { get; set; } = 0.0;

        //public bool UseTakeProfit {
        //    get {
        //        throw new NotImplementedException();
        //    }

        //    set {
        //        throw new NotImplementedException();
        //    }
        //}

    }
}
