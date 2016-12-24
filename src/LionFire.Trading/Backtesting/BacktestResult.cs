using System;
using System.Collections.Generic;
using System.Linq;
#if !cAlgo
using LionFire.Parsing.String;
#endif
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    [Assets.AssetPath("Results")]
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

#if !cAlgo

    public class BacktestResultHandle :IReadHandle<BacktestResult> // TODO: Use IReadHandle or something
    {
        public static implicit operator BacktestResultHandle(BacktestResult r)
        {
            return new BacktestResultHandle { Object = r };
        }

        public bool HasObject { get { return obj != null; } }

        public BacktestResult Object
        {
            get
            {
                if (obj == null && Path != null)
                {
                    try
                    {
                        obj = Newtonsoft.Json.JsonConvert.DeserializeObject<BacktestResult>(System.IO.File.ReadAllText(Path));
                    }
                    catch { }
                }
                return obj;
            }
            set { obj = value; }
        }
        private BacktestResult obj;

        public BacktestResultHandle Self { get { return this; } } // REVIEW - another way to get context from datagrid: ancestor row?
        public string Path { get; set; }

        [Unit("id=")]
        public string Id { get; set; }

        [Unit("bot=")]
        public string Bot { get; set; }

        [Unit("sym=")]
        public string Symbol { get; set; }

        /// <summary>
        /// AROI vs Max Equity Drawdown
        /// </summary>
        [Unit("ad")]
        public double AD { get; set; }

        /// <summary>
        /// Trades Per month
        /// </summary>
        [Unit("tpm")]
        public double TPM { get; set; }

        [Unit("d")]
        public double Days { get; set; }
    }
#endif
}
