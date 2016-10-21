using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class TradingOptions
    {
        /// <summary>
        /// If true:
        ///  - bots will connect to the account(s) that are available. 
        /// 
        /// If false, 
        ///  - bots must specify the account to which they should be connected.
        /// </summary>
        public bool AutoConfig { get; set; } = false;

        /// <summary>
        /// If true, bots will connect to 
        /// </summary>
        public bool AllowAutoMultipleAccounts { get; set; } = false;

        public AccountMode AccountModes { get; set; } = AccountMode.Demo;

        public int DefaultHistoricalDataBars { get; set; } = 400;

        #region Static (Defaults)

        public static TradingOptions Auto {
            get {
                return new TradingOptions
                {
                    AutoConfig = true,
                };
            }
        }
        public static TradingOptions AutoMultiAccount {
            get {
                return new TradingOptions
                {
                    AutoConfig = true,
                    AllowAutoMultipleAccounts = true,
                };
            }
        }

        #endregion

    }
}
