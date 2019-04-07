using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LionFire.Trading.Portfolios
{
    public class PortfolioComponentSimState
    {

        #region State

        /// <summary>
        /// This is from the PortfolioSimulator, which is an interpolation based on trades.
        /// </summary>
        [NotMapped]
        public List<PortfolioBacktestBar> LongExposureBars { get; set; }
        [NotMapped] public List<PortfolioBacktestBar> ShortExposureBars { get; set; }
        [NotMapped] public double? NormalizationMultiplier { get; set; }

        List<_HistoricalTrade> trades = null;

        #endregion

        public void Start()
        {
            LongExposureBars = null;
            ShortExposureBars = null;
            trades = null;
        }

        public void OpenTrade(_HistoricalTrade trade)
        {
            //if (trades == null) {
            //    trades = new List<_HistoricalTrade>();
            //}
            trades.Add(trade);
        }
        public void CloseTrade(_HistoricalTrade trade)
        {
            trades.Remove(trade);
        }
        public void OpenBar(DateTime openTime)
        {
            if (trades == null)
            {
                trades = new List<_HistoricalTrade>();
            }
            if (LongExposureBars == null)
            {
                LongExposureBars = new List<PortfolioBacktestBar>();
                ShortExposureBars = new List<PortfolioBacktestBar>();
            }
            var currentLongExposure = !trades.Any() ? 0 : trades.Select(t => t.Volume * (t.TradeType == TradeType.Buy ? 1 : -1)).Aggregate((x, y) => x + y);
            var currentShortExposure = !trades.Any() ? 0 : trades.Select(t => t.Volume * t.EntryPrice * (t.TradeType == TradeType.Sell ? 1 : -1)).Aggregate((x, y) => x + y);

            LongExposureBars.Add(new PortfolioBacktestBar(openTime, currentLongExposure));
            ShortExposureBars.Add(new PortfolioBacktestBar(openTime, currentShortExposure));
        }

        public void CloseBar()
        {
            LongExposureBars.Last().Close = !trades.Any() ? 0 : trades.Select(t => t.Volume * (t.TradeType == TradeType.Buy ? 1 : -1)).Aggregate((x, y) => x + y);
            ShortExposureBars.Last().Close = !trades.Any() ? 0 : trades.Select(t => t.Volume * t.EntryPrice * (t.TradeType == TradeType.Sell ? 1 : -1)).Aggregate((x, y) => x + y);
        }
    }
         
}
