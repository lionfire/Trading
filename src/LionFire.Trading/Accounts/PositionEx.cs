#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using System.Collections.Generic;

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



        public double RiskPercent => RiskValue / Account.Equity;
        public double RewardPercent => RewardValue / Account.Equity;

        public double RiskValue => Risk * Symbol.PointValue();
        public double RewardValue => Reward * Symbol.PointValue();
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

                if (TradeType == TradeType.Buy)
                {
                    return (target.Value - Symbol.Bid) * Symbol.PointValue();
                }
                else // Sell
                {
                    return (Symbol.Ask - target.Value) * Symbol.PointValue();
                }
            }
        }

        public double BalanceRiskPercent => BalanceRiskValue / Account.Balance;
        public double BalanceRiskValue => BalanceRisk * Symbol.PointValue();

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

        public double BalanceReward
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
