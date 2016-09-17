using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Backtesting
{

    public class BacktestMarket : SimulatedMarketBase, IMarket
    {
        public BacktestConfig Config { get; set; }

        #region IMarket Implementation

        public override bool IsBacktesting { get { return true; } }

        #endregion

        #region Construction

        public BacktestMarket() { }

        public BacktestMarket(BacktestConfig config)
        {
            this.Config = config;
            this.TimeFrame = Config.TimeFrame;
            this.StartDate = Config.StartDate;
            this.EndDate = Config.EndDate;
            this.BrokerName = config.BrokerName;
        }

        #endregion
        


    }
}
