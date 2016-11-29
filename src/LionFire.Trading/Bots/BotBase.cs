using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Reflection;
using LionFire.Structures;
using LionFire.Execution;
using System.IO;
using LionFire.Trading.Backtesting;
#if cAlgo
using cAlgo.API;
#endif
using LionFire.Templating;

namespace LionFire.Trading.Bots
{
    // TODO: Rename BotBase to SingleSeriesBotBase  and make a new BotBase that is more generic

    public partial class BotBase<_TBot> : IBot, IInitializable
        where _TBot : TBot, new()
    {
        public string Version { get; set; } = "0.0.0";

        protected DateTime? StartDate = null;
        protected DateTime? EndDate = null;

        #region Configuration

        ITemplate ITemplateInstance.Template { get { return Template; } set { this.Template = (_TBot)value; } }

        TBot IBot.Template { get { return Template; } set { Template = (_TBot)value; } }

        public _TBot Template { get; set; } = new _TBot();

        public LosingTradeLimiterConfig LosingTradeLimiterConfig { get; set; } = new LosingTradeLimiterConfig();

        public BotMode Mode { get; set; } // REVIEW - disallow change after init?

        #endregion

        public Task<bool> Initialize()
        {
            logger = this.GetLogger(this.ToString().Replace(' ', '.'), Template.Log);
            return Task.FromResult(true);
        }

#if cAlgo
        protected virtual void OnStarting() // This is the main initialization point for cAlgo
#else
        protected override void OnStarting()
#endif
        {
            StartDate = null;
            EndDate = null;
#if cAlgo
            Initialize().Wait();
#endif

            logger.LogInformation($"------- START {this} -------");
        }
        partial void OnStarting_();

        protected virtual void OnStopping()
        {
            logger.LogInformation($"------- STOP {this} -------");
        }

        protected virtual void OnNewBar()
        {
        }

        #region Derived

        public bool CanOpenLong
        {
            get
            {
                var count = Positions.Where(p => p.TradeType == TradeType.Buy).Count();
                return count < Template.MaxLongPositions;
            }
        }
        public bool CanOpenShort
        {
            get
            {
                var count = Positions.Where(p => p.TradeType == TradeType.Sell).Count();
                return count < Template.MaxShortPositions;
            }
        }
        public bool CanOpen
        {
            get
            {
#if !cAlgo
                if (Account.IsDemo && !Mode.HasFlag(BotMode.Demo))
                {
                    return false;
                }
                if (!Account.IsDemo && !Mode.HasFlag(BotMode.Live))
                {
                    return false;
                }
#endif
                var count = Positions.Count;
                return Template.MaxOpenPositions == 0 || count < Template.MaxOpenPositions;
            }
        }

        public bool CanOpenType(TradeType tradeType)
        {
            if (!CanOpen) return false;
            switch (tradeType)
            {
                case TradeType.Buy:
                    return CanOpenLong;
                case TradeType.Sell:
                    return CanOpenShort;
                default:
                    return false;
            }
        }

        #endregion

        #region Backtesting

        public const double FitnessMaxDrawdown = 95;
        public const double FitnessMinDrawdown = 0.001;



#if cAlgo
        protected override
#else
        public virtual
#endif
         double GetFitness(GetFitnessArgs args)
        {
            var dd = args.MaxEquityDrawdownPercentages;
            dd = Math.Max(FitnessMinDrawdown, dd);

            if (dd > FitnessMaxDrawdown) { return -dd; }
            var initialBalance = args.History.Count == 0 ? args.Equity : args.History[0].Balance - args.History[0].NetProfit;

            double result;
            if (!EndDate.HasValue || !StartDate.HasValue)
            {
                result = 0.0;
            }
            else
            {
                var timeSpan = EndDate.Value - StartDate.Value;
                var totalMonths = timeSpan.TotalDays / 31;
                var tradesPerMonth = args.TotalTrades / totalMonths;

                var aroi = (args.NetProfit / initialBalance) / (timeSpan.TotalDays / 365);

                if (args.LosingTrades == 0)
                {
                    result = aroi;
                }
                else
                {
                    result = aroi / dd;
                }

                var minTrades = Template.BacktestMinTradesPerMonth;

                //#if cAlgo
                //                logger.LogInformation($"dd: {args.MaxEquityDrawdownPercentages }  Template.BacktestMinTradesPerMonth {Template.BacktestMinTradesPerMonth} tradesPerMonth {tradesPerMonth}");
                //#endif

                if (minTrades > 0 && tradesPerMonth < minTrades && result > 0)
                {
                    result *= tradesPerMonth / minTrades;
                }

                //#if cAlgo
                //            Print($"Fitness: {StartDate.Value} - {EndDate.Value} profit: {args.NetProfit} years: {((EndDate.Value - StartDate.Value).TotalDays / 365)} constrained eqDD:{dd} aroi:{aroi} aroi/DD:{aroiVsDD}");
                //#endif

                result *= 100.0;
                DoLogBacktest(args, result, initialBalance, timeSpan);
            }

            return result;
        }

        protected virtual void DoLogBacktest(GetFitnessArgs args, double fitness, double initialBalance, TimeSpan timeSpan)
        {

            if (Template.LogBacktestThreshold != 0 && fitness > Template.LogBacktestThreshold)
            {
                var backtestResult = new BacktestResult()
                {
                    BacktestDate = DateTime.UtcNow,
                    BotType = this.GetType().FullName,
                    Config = this.Template,

                    //Start = this.MarketSeries?.OpenTime?[0],
                    //End = this.MarketSeries?.OpenTime?.LastValue,
                    Start = StartDate.Value,
                    End = EndDate.Value,

                    AverageTrade = args.AverageTrade,
                    Equity = args.Equity,
                    //History
                    LosingTrades = args.LosingTrades,
                    MaxBalanceDrawdown = args.MaxBalanceDrawdown,
                    MaxBalanceDrawdownPercentages = args.MaxBalanceDrawdownPercentages,
                    MaxEquityDrawdown = args.MaxEquityDrawdown,
                    MaxEquityDrawdownPercentages = args.MaxEquityDrawdownPercentages,
                    NetProfit = args.NetProfit,
                    //PendingOrders
                    //Positions
                    ProfitFactor = args.ProfitFactor,
                    SharpeRatio = args.SharpeRatio,
                    SortinoRatio = args.SortinoRatio,
                    TotalTrades = args.TotalTrades,
                    WinningTrades = args.WinningTrades,
                };

                BacktestLogger = this.GetLogger(this.ToString().Replace(' ', '.') + ".Backtest");

                try
                {
                    string resultJson = "";
                    resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(backtestResult);

                    var profit = args.Equity / initialBalance;

                    this.BacktestLogger.LogInformation($"${args.Equity} ({profit.ToString("N1")}x) #{args.History.Count} {args.MaxEquityDrawdownPercentages.ToString("N2")}%dd [from ${initialBalance.ToString("N2")} to ${args.Equity.ToString("N2")}] [fit {fitness.ToString("N1")}] {Environment.NewLine} result = {resultJson} ");
                    var id = Template.Id;
                    SaveResult(args, fitness, resultJson, id, timeSpan);
                }
                catch (Exception ex)
                {
                    this.BacktestLogger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        private async void SaveResult(GetFitnessArgs args, double fitness, string json, string id, TimeSpan timeSpan)
        {
            var dir = @"c:\Trading\Results"; // HARDPATH

            var filename = DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss.fff ") + " #" + args.TotalTrades + " " + timeSpan.TotalDays.ToString("N0") + "d " + this.GetType().Name + " " + Symbol.Code + " " + id;
            filename = Math.Log(fitness, 2.0).ToString("00.00") + " " + filename;
            var ext = ".json";
            int i = 0;
            var path = Path.Combine(dir, filename + ext);
            for (; File.Exists(path); i++, path = Path.Combine(dir, filename + $" ({i})" + ext)) ;
            using (var sw = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                await sw.WriteAsync(json);
            }
        }


        public Microsoft.Extensions.Logging.ILogger BacktestLogger { get; protected set; }

        #endregion


        #region Misc

        public virtual string Label
        {
            get
            {
                return label ?? this.GetType().Name;
            }
            set
            {
                label = value;
            }
        }
        private string label = null;

        //public Microsoft.Extensions.Logging.ILogger BacktestLogger { get; protected set; }

#if cAlgo
        public Microsoft.Extensions.Logging.ILogger Logger
        {
            get { return logger; }
        }
        protected Microsoft.Extensions.Logging.ILogger logger { get; set; }
#endif

        #endregion
    }


}
