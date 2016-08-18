using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Trading.Bots;

namespace LionFire.Trading
{
    
    public class SimulatedMarket : SimulatedMarketBase, IMarket
    {
        #region IMarket Implementation

        public override bool IsBacktesting { get { return true; } }
        
        #endregion

    }
}
