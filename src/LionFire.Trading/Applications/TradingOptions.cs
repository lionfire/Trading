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

        #region Construction

        public TradingOptions() { }
        public TradingOptions(TradingFeatures features) { this.Features = features; }

        #endregion



        /// <summary>
        /// If true:
        ///  - bots will connect to the account(s) that are available. 
        /// 
        /// If false, 
        ///  - bots must specify the account to which they should be connected.
        /// </summary>
        public bool AutoAttachToAccounts { get; set; } = false;

        /// <summary>
        /// If true, bots will connect to 
        /// </summary>
        public bool AllowAutoMultipleAccounts { get; set; } = false;

        public AccountMode AccountModes { get; set; } = AccountMode.Demo;

        public int DefaultHistoricalDataBars { get; set; } = DefaultHistoricalDataBarsDefault;
        public const int DefaultHistoricalDataBarsDefault = 15;

        public string Test { get; set; }

        /// <summary>
        /// Leave null for no whitelist
        /// </summary>
        public List<string> SymbolsWhiteList { get; set; }

        public List<string> SymbolsBlackList { get; set; }

        public DateTime? HistoricalDataStart { get; set; }
        public DateTime EffectiveHistoricalDataStart
        {
            get
            {
                if (HistoricalDataStart.HasValue)
                {
                    var val = HistoricalDataStart.Value;
                    if (val > DateTime.UtcNow + TimeSpan.FromDays(2)) return DateTime.UtcNow + TimeSpan.FromDays(2);
                    return val;
                }
                return DateTime.MinValue;
            }
        }
        public DateTime? HistoricalDataEnd { get; set; }
        public DateTime EffectiveHistoricalDataEnd
        {
            get
            {
                if (HistoricalDataEnd.HasValue && HistoricalDataEnd.Value < DateTime.UtcNow + TimeSpan.FromDays(2)) return HistoricalDataEnd.Value;
                return DateTime.UtcNow + TimeSpan.FromDays(1);
            }
        }

        public List<TimeFrame> HistoricalDataTimeFrames { get; set; }

        #region Static (Defaults)

        public static TradingOptions Auto
        {
            get
            {
                return new TradingOptions
                {
                    AutoAttachToAccounts = true,
                };
            }
        }
        public static TradingOptions AutoMultiAccount
        {
            get
            {
                return new TradingOptions
                {
                    AutoAttachToAccounts = true,
                    AllowAutoMultipleAccounts = true,
                };
            }
        }

        #endregion

        public TradingFeatures Features { get; set; }
        public IEnumerable<string> ExcludeSymbols { get; set; }
        public bool ForceReretrieveEmptyData { get; set; }

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
