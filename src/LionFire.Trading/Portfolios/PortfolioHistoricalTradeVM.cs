using System;
using System.Collections.Generic;
using LionFire.Trading.Backtesting;

namespace LionFire.Trading.Portfolios
{
    public struct PortfolioHistoricalTradeVM
    {
        public _HistoricalTrade Trade { get; set; }
        public PortfolioComponent Component { get; set; }
        public double LongVolumeAtEntry { get; internal set; }
        public double ShortVolumeAtEntry { get; internal set; }
        public string LongAsset { get; set; }
        public string ShortAsset { get; set; }

        public List<PortfolioBacktestBar> NetExposure { get; set; }

        public double InterpolatedNetProfit(DateTime currentTime)
        {
            var totalTime = Trade.ClosingTime - Trade.EntryTime;
            var progressTime = currentTime - Trade.EntryTime;
            var progress = progressTime.TotalMilliseconds / totalTime.TotalMilliseconds;
            return progress * Trade.NetProfit;
        }
    }
}
