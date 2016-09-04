using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{

    public class BacktestSettings
    {
        public double StartingBalance { get; set; } = 1000.0;

        public Dictionary<string, BacktestSymbolSettings> BacktestSymbolSettings = new Dictionary<string, BacktestSymbolSettings>();

        public BacktestAccountSettings AccountSettings { get; set; } = new BacktestAccountSettings();

    }
}
