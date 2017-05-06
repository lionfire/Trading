using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using LionFire.Parsing.String;

using System.Threading.Tasks;
using LionFire.Trading.Bots;

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

        [Unit("tpm")]
        public double TradesPerMonth { get { return TotalTrades / Months; } }

        public double Days { get { return Duration.TotalDays; } }
        public double Months { get { return Duration.TotalDays / 31; } }

        /// <summary>
        /// Annual equity return on investment percent
        /// </summary>
        public double Aroi { get { return (NetProfit / InitialBalance) / (Duration.TotalDays / 365); } }

        #endregion

        /// <summary>
        /// Average trade duration in days
        /// </summary>
        public double AverageDaysPerTrade { get; set; }
        [Unit("adwt")]
        public double AverageDaysPerWinningTrade { get; set; }
        [Unit("adlt")]
        public double AverageDaysPerLosingTrade { get; set; }

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

        public string TimeFrame
        {
            get
            {
                dynamic tbot = (Config as JObject);
                //var tbot = Config as Bots.TBot;
                return tbot?.TimeFrame.Value;
            }
        }

        /// <summary>
        /// Computed at backtest time
        /// </summary>
        public double Fitness { get; set; }

        public double InitialBalance { get; set; }

        /// <summary>
        /// AnnualReturnPercentPerEquityDrawdown
        /// </summary>
        [Unit("ad")]
        public double AD { get; set; }


        public TBot TBot
        {
            get
            {
                if (tBot == null)
                {
                    var backtestResult = this;

                    var templateType = ResolveType(backtestResult.BotConfigType);

                    if (templateType == null)
                    {
                        throw new NotSupportedException($"Bot type not supported: {backtestResult.BotConfigType}");
                    }

                    tBot = (TBot)((JObject)backtestResult.Config).ToObject(templateType);
                }
                return tBot;
            }
        }
        private TBot tBot;

        public Type ResolveType(string typeName)
        {
            typeName = typeName.Replace("LionFire.Trading.cTrader", "LionFire.Trading.Proprietary");

            return TypeResolver.Default.TryResolve(typeName);
        }

    }


}
