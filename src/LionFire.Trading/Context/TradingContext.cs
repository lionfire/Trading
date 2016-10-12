using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class TradingContext : ITradingContext
    {
        #region Configuration

        public TradingOptions Options { get; set; }

        #endregion

        #region Construction


        public TradingContext(TradingOptions options = null)
        {
            this.Options = options;
        }

        #endregion
        
        
    }
}
