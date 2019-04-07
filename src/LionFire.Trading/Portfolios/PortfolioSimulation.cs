using System;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Portfolios
{
    
    public class PortfolioSimulationStats
    {

        public PortfolioAnalysisOptions Options { get; set; }

        public double MaxEquityDrawdownPercent { get; set; }
        public double MaxEquityDrawdown { get; set; }
        public double MaxBalanceDrawdownPercent { get; set; }
        public double MaxBalanceDrawdown { get; set; }
        public double ProfitPercent { get; set; }


    }
    public class PortfolioSimulation : PortfolioSimulationStats
    {
        public static PortfolioSimulationStats operator -(PortfolioSimulation s1, PortfolioSimulation s2) {
            var result = new PortfolioSimulationStats {
                Options = s1.Options,
                MaxEquityDrawdownPercent = s1.MaxEquityDrawdownPercent - s2.MaxEquityDrawdownPercent,
                MaxEquityDrawdown = s1.MaxEquityDrawdown - s2.MaxEquityDrawdown,
                MaxBalanceDrawdownPercent = s1.MaxBalanceDrawdownPercent - s2.MaxBalanceDrawdownPercent,
                MaxBalanceDrawdown = s1.MaxBalanceDrawdown - s2.MaxBalanceDrawdown,
                ProfitPercent = s1.ProfitPercent - s2.ProfitPercent
            };

            return result;
        }

        public Portfolio Portfolio { get; set; }
        public PortfolioSimulation(Portfolio portfolio, PortfolioAnalysisOptions options) {
            this.Portfolio = portfolio;
            this.Options = options;
        }

        public double Max { get; set; } = double.NaN;

        public List<PortfolioBacktestBar> EquityBars { get; set; }
        public List<PortfolioBacktestBar> BalanceBars { get; set; }


        #region (Derived) Stats

        private double GetProfitPercent() {
            double profitPercent;
            if (BalanceBars == null || BalanceBars.Count == 0) { profitPercent = double.NaN; } else {
                profitPercent = (BalanceBars.Last().Close - Options.InitialBalance) / Options.InitialBalance;
            }
            return profitPercent;
        }

        public Dictionary<string, List<PortfolioBacktestBar>> AssetExposureBars { get; set; }
        public DateTime EffectiveStartTime {
            get {
                if (!Portfolio.Start.HasValue) return Options.StartTime;
                return (Options.StartTime != default && Options.StartTime > Portfolio.Start.Value) ? Options.StartTime : Portfolio.Start.Value;
            }
        }

        #endregion

        public void OnStopped() {
            ProfitPercent = GetProfitPercent();
        }
    }

    //public class PortfolioBacktest
    //{
    //    public string Id { get; set; }

    //    public double AD { get; set; }
    //    public double MaxEquityDrawdown { get; set; }
    //    public double MaxEquityDrawdownPercent { get; set; }
    //    public double MaxBalanceDrawdown { get; set; }
    //    public double MaxBalanceDrawdownPercent { get; set; }
    //    public double NetProfit { get; set; }
    //}

}
