using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading
{
    
    public abstract class MarketBase
    {
        public MarketDataProvider Data { get; private set; }

        public MarketData MarketData { get; set; } 

        public MarketBase()
        {
            Data = new MarketDataProvider((IMarket)this);
            logger = this.GetLogger();
        }

        public abstract MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame);



        #region Misc

        protected ILogger logger;

        #endregion
    }
}
