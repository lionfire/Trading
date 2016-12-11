using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    public class BacktestResult
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        #region Derived

        public TimeSpan Duration { get { if (!Start.HasValue || !Start.HasValue) { return TimeSpan.Zero; } return End.Value - Start.Value; } }

        public double WinRate { get { return WinningTrades / TotalTrades; } }

        public double TradesPerMonth { get { return TotalTrades / Months; } }

        public double Days { get { return Duration.TotalDays; } }
        public double Months { get { return Duration.TotalDays / 31; } }

        /// <summary>
        /// Annual equity return on investment percent
        /// </summary>
        public double Aroi { get { return (NetProfit / InitialBalance) / (Duration.TotalDays / 365); } }

        #endregion

        public double AverageTrade { get; set; }
        public double Equity { get; set; }
        //public History History { get; set; }
        public double LosingTrades { get; set; }
        public double MaxBalanceDrawdown { get; set; }
        public double MaxBalanceDrawdownPercentages { get; set; }
        public double MaxEquityDrawdown { get; set; }
        public double MaxEquityDrawdownPercentages { get; set; }
        public double NetProfit { get; set; }
        //public PendingOrders PendingOrders { get; set; }
        //public Positions Positions { get; set; }
        public double ProfitFactor { get; set; }
        public double SharpeRatio { get; set; }
        public double SortinoRatio { get; set; }
        public double TotalTrades { get; set; }
        public double WinningTrades { get; set; }

        public DateTime BacktestDate { get; set; }
        public string BotType { get; set; }
        public string BotTypeName { get { return BotType.Substring(BotType.LastIndexOf('.') + 1); } }
        public string BotConfigType { get; set; }
        public object Config { get; set; }

        /// <summary>
        /// Computed at backtest time
        /// </summary>
        public double Fitness { get; set; }
        
        public double InitialBalance { get; set; }

        /// <summary>
        /// AnnualReturnPercentPerEquityDrawdown
        /// </summary>
        public double AD { get; set; }



    }
}
