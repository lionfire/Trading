// MOVE to LionFire.Trading, link to LionFire.Trading.cTrader
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace LionFire.Trading
{
    public partial class PositionEx
    {
        #region Stats (TODO)

        public double? MaxStopLoss { get; set; }

        public double MaxRisk { get; set; } // TODO
        public double MaxRiskBalancePercent { get; set; } // TODO

        #endregion

        public double? Target { get; set; } 

        // FUTURE: Multiple targets
        //public List<ProfitTarget> Targets { get; set; }

        #region Derived

        public double BalanceRiskPercent => BalanceRiskValue / Account.Balance;
        public double BalanceRiskValue => BalanceRisk * Symbol.PointValue();

        public double RiskPercent => RiskValue / Account.Equity;
        public double RiskValue => Risk * Symbol.PointValue();

        public double BalanceRisk
        {
            get
            {
                if (!StopLoss.HasValue) return double.PositiveInfinity;

                if (TradeType == TradeType.Buy)
                {
                    if (StopLoss.Value > EntryPrice) return 0;

                    return EntryPrice - StopLoss.Value;
                }
                else // Sell
                {
                    if (StopLoss.Value < EntryPrice) return 0;
                    return StopLoss.Value - EntryPrice;
                }
            }
        }
        
        public double Risk
        {
            get
            {
                if (!StopLoss.HasValue) return double.PositiveInfinity;

                if (TradeType == TradeType.Buy)
                {
                    return (Symbol.Bid - StopLoss.Value) * Symbol.PointValue();
                }
                else // Sell
                {
                    return (StopLoss.Value - Symbol.Ask) * Symbol.PointValue();
                }
            }
        }
        public double Reward
        {
            get
            {
                var target = Target ?? TakeProfit;
                if (!target.HasValue) return 0;
                return (target.Value - EntryPrice) * Symbol.PointValue();
            }
        }
        public double RRRatio => (Reward / Risk);

        #endregion

    }
}
