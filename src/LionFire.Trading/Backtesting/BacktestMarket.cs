using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Trading.Bots;
using LionFire.Templating;

namespace LionFire.Trading.Backtesting
{

    public class BacktestMarket : SimulatedMarketBase<TBacktestMarket>, IMarket
    {
        #region Relationships

        public new BacktestAccount Account {
            get {
                return (BacktestAccount) base.Accounts.SingleOrDefault();
            }
            set {
                base.Accounts.Clear();
                base.Accounts.Add(value);
            }
        }

        #endregion

        #region IMarket Implementation

        public override bool IsBacktesting { get { return true; } }

        #endregion

        #region Construction

        public BacktestMarket() { }

        public BacktestMarket(TBacktestMarket config)
        {
            this.Config = config;
        }

        #endregion

        #region Initialization

        bool isInitialized = false;
        public override void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            this.TimeFrame = Config.TimeFrame;
            this.StartDate = Config.StartDate;
            this.EndDate = Config.EndDate;
                        
            this.Account = new BacktestAccount(Config.BrokerName)
            {
                Market = this,
            };

            base.Initialize();
        }

        #endregion

        #region Event Handling

        protected override void OnLastExecutedTimeChanged()
        {
            base.OnLastExecutedTimeChanged();
            this.Account.UpdatePositions();
        }

        #endregion
    }
}
