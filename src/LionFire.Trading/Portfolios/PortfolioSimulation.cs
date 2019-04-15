using System;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Portfolios
{
    public class PortfolioSimulation
    {
        public PortfolioAnalysisOptions Options { get; set; }

        #region Construction

        public Portfolio Portfolio { get; set; }
        public PortfolioSimulation(Portfolio portfolio, PortfolioAnalysisOptions options)
        {
            this.Portfolio = portfolio;
            this.Options = options;
        }

        #endregion

        public double Max { get; set; } = double.NaN;

        [Ignore]
        public List<PortfolioBacktestBar> EquityBars { get; set; }
        public List<PortfolioBacktestBar> BalanceBars { get; set; }

        #region State

        #region Exceptions

        public List<Exception> Exceptions { get; set; }

        /// <returns>true if processing should continue, if possible</returns>
        public bool OnException(Exception ex)
        {
            if (Exceptions == null)
            {
                Exceptions = new List<Exception>();
            }
            Exceptions.Add(ex);

            return Options.ContinueOnError;
        }

        #endregion

        #endregion

        #region (Derived) Stats

        #region Time

        public DateTime EffectiveStartTime {
            get {
                if (!Portfolio.Start.HasValue) return Options.StartTime;
                return (Options.StartTime != default && Options.StartTime > Portfolio.Start.Value) ? Options.StartTime : Portfolio.Start.Value;
            }
        }

        #endregion

        public PortfolioSimulationStats Stats { get; set; } = new PortfolioSimulationStats();

        #region Asset Exposure

        public Dictionary<string, List<PortfolioBacktestBar>> AssetExposureBars { get; set; }

        #endregion

        private double GetProfitPercent()
        {
            double profitPercent;
            if (BalanceBars == null || BalanceBars.Count == 0) { profitPercent = double.NaN; }
            else
            {
                profitPercent = (BalanceBars.Last().Close - Options.InitialBalance) / Options.InitialBalance;
            }
            return profitPercent;
        }

        #endregion

        #region Event Handling

        public void OnStopped()
        {            
            Stats.ProfitPercent = GetProfitPercent();
        }

        #endregion

        
    }
}
