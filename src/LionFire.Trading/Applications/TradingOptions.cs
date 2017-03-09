using LionFire.Assets;
using LionFire.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class TradingOptions : INotifyPropertyChanged
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

        public int DefaultHistoricalDataBars { get; set; } = DefaultHistoricalDataBarsDefault;
        public const int DefaultHistoricalDataBarsDefault = 15;

        public string Test { get; set; }

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
        
        //#region AllowSubscribeToTicks

        //public bool AllowSubscribeToTicks
        //{
        //    get { return allowSubscribeToTicks; }
        //    set
        //    {
        //        if (allowSubscribeToTicks == value) return;
        //        allowSubscribeToTicks = value;
        //        OnPropertyChanged(nameof(AllowSubscribeToTicks));
        //    }
        //}
        //private bool allowSubscribeToTicks;

        //#endregion

        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
        
    }
}
