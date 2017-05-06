using LionFire.Assets;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;

namespace LionFire.Trading.Accounts
{

    [AssetPath(@"Accounts")]
    public abstract class TMarketAccount : TFeed
    {
        
        public double CommissionPerMillion { get; set; }
        public string Currency { get; set; }
        public bool IsLive { get; set; }
        public double Leverage { get; set; }

        public int? BackFillMinutes { get; set; }

        public double StopOutLevel { get; set; } = double.NaN;

        #region Derived

        public bool IsDemo { get { return !IsLive; } }
        

        #endregion
    }
}
