using System.Collections.Generic;

namespace LionFire.Trading.Portfolios
{
    public class Correlation
    {
        public Correlation(PortfolioComponent component1, PortfolioComponent component2) {
            this.Component1 = component1;
            this.Component2 = component2;
            SameSymbol = component1.BacktestResultId == component2.BacktestResultId;
        }

        public PortfolioComponent Component1 { get; set; }
        public PortfolioComponent Component2 { get; set; }

        public bool SameSymbol { get; }

        public IEnumerable<PortfolioComponent> Components {
            get {
                yield return Component1;
                yield return Component2;
            }
        }
        public double HighScore { get; set; }
        public double LowScore { get; set; }
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
