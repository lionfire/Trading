using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Templating;

namespace LionFire.Trading.Backtesting
{



    public class TBacktestMarket : TMarketSim, ITemplate<BacktestMarket>
    {

        public Dictionary<string, BacktestSymbolSettings> BacktestSymbolSettings = new Dictionary<string, BacktestSymbolSettings>();

        public BacktestAccountSettings AccountSettings { get; set; } = new BacktestAccountSettings();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public TimeFrame TimeFrame { get; set; }


        public double StopOutLevel { get; set; } = 0.5;

        public double StartingBalance { get; set; } = 1000.0;

        //public List<string> Symbols { get; set; }


        public bool ClosePositionsAtEnd { get; set; } = false; // FUTURE


        public bool LogBacktest { get; set; } = true;

        public bool SaveBacktestBotConfigs { get; set; } = true;

        #region Derived

        public TimeSpan TimeSpan {
            get { return EndDate - StartDate; }
        }

        public double TotalBars { get { return TimeSpan.TotalDays / TimeFrame.TimeSpan.TotalDays; } }

        #endregion

    }
}
