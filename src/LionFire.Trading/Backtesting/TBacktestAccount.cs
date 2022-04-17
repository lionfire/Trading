using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Instantiating;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Accounts;

namespace LionFire.Trading.Backtesting
{
    public class TBacktestAccount : TSimulatedAccountBase, ITemplate<BacktestAccount>
    {

        public Dictionary<string, BacktestSymbolSettings> BacktestSymbolSettings = new Dictionary<string, BacktestSymbolSettings>();

        public BacktestAccountSettings AccountSettings { get; set; } = new BacktestAccountSettings();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public TimeFrame TimeFrame { get; set; }

        public bool ClosePositionsAtEnd { get; set; } = false; // FUTURE

        //public bool LogBacktest { get; set; } = true;

        public bool SaveBacktestBotConfigs { get; set; } = true;

        #region Derived

        public TimeSpan TimeSpan
        {
            get { return EndDate - StartDate; }
        }

        public double TotalBars { get { return TimeSpan.TotalDays / TimeFrame.TimeSpanApproximation.TotalDays; } }

        public string SimulateAccount { get; set; }

        #endregion

    }
}
