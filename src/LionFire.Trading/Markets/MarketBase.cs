using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    
    public class MarketBase
    {
        public MarketDataProvider Data { get; private set; }

        public MarketBase()
        {
            Data = new MarketDataProvider((IMarket)this);
        }
    
    }
}
