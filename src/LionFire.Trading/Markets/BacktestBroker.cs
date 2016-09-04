using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    public class BacktestBroker
    {
        #region Construction

        public BacktestBroker() { }
        public BacktestBroker(string name) { this.Name = name; }

        #endregion

        public string Name { get; set; }

        public BacktestSymbolSettings DefaultBacktestSymbolSettings { get; set; }



        
    }

    public class BacktestBrokers
    {
        public static BacktestBroker Default { get { return ICMarkets; } }

        public static BacktestBroker ICMarkets { get;private set;}

        static BacktestBrokers()
        {
            ICMarkets = new BacktestBroker("IC Markets")
            {
                DefaultBacktestSymbolSettings = new BacktestSymbolSettings
                {
                    CommissionPerMillion = 30,

                },
            };
        }
            

    }
}
