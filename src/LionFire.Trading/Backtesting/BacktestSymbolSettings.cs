using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    public class BacktestSymbolSettings
    {

        public BacktestSpreadMode SpreadMode { get; set; } = BacktestSpreadMode.Fixed;

        public double FixedSpread { get; set; } = 0.0;
        public double MinRandomSpread { get; set; } = 0.5;
        public double MaxRandomSpread { get; set; } = 2.0;

        public double MarginRequirement { get; set; } = 1.0 / 100.0;
        public double CommissionPerMillion { get; internal set; } = 30.0;

        public double GetSpread()
        {
            switch (SpreadMode)
            {
                case BacktestSpreadMode.Fixed:
                    return FixedSpread;
                case BacktestSpreadMode.Random:
                    return MinRandomSpread + Singleton<Random>.Instance.NextDouble() * (MaxRandomSpread - MinRandomSpread);
                case BacktestSpreadMode.Lookup:
                    throw new NotImplementedException("SpreadMode.Lookup");
                default:
                    throw new ArgumentException();
            }
        }
    }
}
