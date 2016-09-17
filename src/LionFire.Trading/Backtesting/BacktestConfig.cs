using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{


    public class BacktestConfig
    {
        public double StartingBalance { get; set; } = 1000.0;

        public Dictionary<string, BacktestSymbolSettings> BacktestSymbolSettings = new Dictionary<string, BacktestSymbolSettings>();

        public BacktestAccountSettings AccountSettings { get; set; } = new BacktestAccountSettings();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeFrame TimeFrame { get; set; }
        public string BrokerName { get; set; }


        public double StopOutLevel { get; set; } = 80;

        public List<string> Symbols { get; set; }
        public List<Type> Bots { get; set; }
    }
}
