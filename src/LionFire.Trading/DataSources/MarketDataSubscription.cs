using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{ 
    public class MarketDataSubscription
    {
        #region Construction

        public MarketDataSubscription(string symbol, string timeFrame, bool isOptional = false)
        {
            this.Symbol = symbol;
            this.TimeFrame = TimeFrame.TryParse(timeFrame);
            this.IsOptional = isOptional;
        }
        public MarketDataSubscription(string symbol, TimeFrame timeFrame, bool isOptional = false)
        {
            this.Symbol = symbol;
            this.TimeFrame = timeFrame;
            this.IsOptional = isOptional;
        }

        #endregion

        #region Parameters

        public string Symbol { get; set; }
        public TimeFrame TimeFrame { get; set; }

        public bool IsOptional { get; set; }

        #region Derived

        public string Key { get { return Symbol + ";" + TimeFrame.Name; } }

        #endregion

        #endregion

        #region State

        public bool IsActive {
            get; set;
        }
        public IMarketSeries Series { get; internal set; }

        // FUTURE: Stats on quality of feed

        #endregion

        #region Misc

        public override string ToString()
        {
            return $"{Symbol} ({TimeFrame})";
        }

        #endregion
    }
}
