namespace LionFire.Trading.Portfolios
{
    public class PortfolioSimulationStats
    {
       

        public double MaxEquityDrawdownPercent { get; set; }
        public double MaxEquityDrawdown { get; set; }
        public double MaxBalanceDrawdownPercent { get; set; }
        public double MaxBalanceDrawdown { get; set; }
        public double ProfitPercent { get; set; }

        /// <summary>
        /// AROI / Equity DD
        /// </summary>
        public double AD { get; set; }

        /// <summary>
        /// AROI / Balance DD
        /// </summary>
        public double ABD { get; set; }


        /// <summary>
        /// ROI / Equity DD
        /// </summary>
        public double RD { get; set; }

        /// <summary>
        /// Average ROI / Max Equity Drawdown
        /// </summary>
        public double AAD { get; set; }


        #region (Static) Operator Overloads

        public static PortfolioSimulationStats operator -(PortfolioSimulationStats s1, PortfolioSimulationStats s2)
        {
            var result = new PortfolioSimulationStats
            {
                //Options = s1.Options,
                MaxEquityDrawdownPercent = s1.MaxEquityDrawdownPercent - s2.MaxEquityDrawdownPercent,
                MaxEquityDrawdown = s1.MaxEquityDrawdown - s2.MaxEquityDrawdown,
                MaxBalanceDrawdownPercent = s1.MaxBalanceDrawdownPercent - s2.MaxBalanceDrawdownPercent,
                MaxBalanceDrawdown = s1.MaxBalanceDrawdown - s2.MaxBalanceDrawdown,
                ProfitPercent = s1.ProfitPercent - s2.ProfitPercent
            };

            return result;
        }

        #endregion
    }

}
