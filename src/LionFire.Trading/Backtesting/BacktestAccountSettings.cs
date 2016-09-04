using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    public class BacktestAccountSettings
    {
        public BacktestSymbolSettings DefaultSymbolSettings { get; set; }

        public Dictionary<string, BacktestSymbolSettings> SymbolSettings { get; set; } = new Dictionary<string, BacktestSymbolSettings>();

        public double StopOutLevel { get; set; } = 0.8;
    }
}
