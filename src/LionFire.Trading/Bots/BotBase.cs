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
using System.ComponentModel;
using System.Collections.ObjectModel;
#if cAlgo
using cAlgo.API.Internals;
using cAlgo.API;
#endif
using LionFire.Templating;

namespace LionFire.Trading.Bots
{
    // TODO: Rename BotBase to SingleSeriesBotBase  and make a new BotBase that is more generic

    public partial class BotBase<_TBot> : IBot, IInitializable, INotifyPropertyChanged
        // REVIEW OPTIMIZE - eliminate INPC for cAlgo?
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

        #region Derived

        //public bool IsScanner { get { return Mode.HasFlag(BotMode.Paper); } }

        #endregion

        #endregion

        #region Lifecycle

        public BotBase()
        {
            InitExchangeRates();
        }

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

#if !cAlgo
        public  Task Stop(StopMode mode = StopMode.GracefulShutdown, StopOptions options = StopOptions.StopChildren)
        {
            this.state.OnNext(ExecutionState.Stopping);

            this.state.OnNext(ExecutionState.Stopped);
            return Task.CompletedTask;
        }
#endif

        #endregion

        #region State

        #region Derived

        public DateTime ExtrapolatedServerTime
        {
            get
            {
#if cAlgo
                return Server.Time;
#else
            return Account.ExtrapolatedServerTime;
#endif
            }
        }

#endregion

#endregion

#region Event Handling


        protected virtual void OnNewBar()
        {
        }

#endregion

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

            var botType = this.GetType().FullName;
#if cAlgo
            if (this.GetType().FullName.StartsWith("cAlgo."))
            {
                botType = this.GetType().GetTypeInfo().BaseType.FullName;
            }
            if (Template.TimeFrame == null)
            {
                Template.TimeFrame = this.TimeFrame.ToShortString();
            }
            if (Template.Symbol == null)
            {
                Template.Symbol = this.Symbol.Code;
            }
#endif
            var backtestResult = new BacktestResult()
            {
                BacktestDate = DateTime.UtcNow,
                BotType = botType,
                BotConfigType = this.Template.GetType().AssemblyQualifiedName,
                Config = this.Template,
                InitialBalance = initialBalance,
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

            double fitness;
            if (!EndDate.HasValue || !StartDate.HasValue)
            {
                fitness = 0.0;
            }
            else
            {
                var timeSpan = EndDate.Value - StartDate.Value;
                var totalMonths = timeSpan.TotalDays / 31;
                var tradesPerMonth = backtestResult.TradesPerMonth;

                //var aroi = (args.NetProfit / initialBalance) / (timeSpan.TotalDays / 365);
                var aroi = backtestResult.Aroi;

                if (args.LosingTrades == 0)
                {
                    fitness = aroi;
                }
                else
                {
                    fitness = aroi / dd;
                }

                var minTrades = Template.BacktestMinTradesPerMonth;

                //#if cAlgo
                //                logger.LogInformation($"dd: {args.MaxEquityDrawdownPercentages }  Template.BacktestMinTradesPerMonth {Template.BacktestMinTradesPerMonth} tradesPerMonth {tradesPerMonth}");
                //#endif

                if (minTrades > 0 && tradesPerMonth < minTrades && fitness > 0)
                {
                    fitness *= tradesPerMonth / minTrades;
                }

                //#if cAlgo
                //            Print($"Fitness: {StartDate.Value} - {EndDate.Value} profit: {args.NetProfit} years: {((EndDate.Value - StartDate.Value).TotalDays / 365)} constrained eqDD:{dd} aroi:{aroi} aroi/DD:{aroiVsDD}");
                //#endif

                fitness *= 100.0;

                backtestResult.Fitness = fitness;
                DoLogBacktest(args, backtestResult);
            }

            return fitness;
        }

        protected virtual void DoLogBacktest(GetFitnessArgs args, BacktestResult backtestResult)
        {
            double fitness = backtestResult.Fitness;
            double initialBalance = backtestResult.InitialBalance;
            TimeSpan timeSpan = backtestResult.Duration;
            double aroi = backtestResult.Aroi;

            if (Template.LogBacktestThreshold != 0 && fitness > Template.LogBacktestThreshold)
            {

                BacktestLogger = this.GetLogger(this.ToString().Replace(' ', '.') + ".Backtest");

                try
                {
                    string resultJson = "";
                    resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(backtestResult);

                    var profit = args.Equity / initialBalance;

                    this.BacktestLogger.LogInformation($"${args.Equity} ({profit.ToString("N1")}x) #{args.History.Count} {args.MaxEquityDrawdownPercentages.ToString("N2")}%dd [from ${initialBalance.ToString("N2")} to ${args.Equity.ToString("N2")}] [fit {fitness.ToString("N1")}] {Environment.NewLine} result = {resultJson} ");
                    var id = Template.Id;
                    SaveResult(args, backtestResult, fitness, resultJson, id, timeSpan);
                }
                catch (Exception ex)
                {
                    this.BacktestLogger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        private async void SaveResult(GetFitnessArgs args, BacktestResult backtestResult, double fitness, string json, string id, TimeSpan timeSpan)
        {
            var dir = Path.Combine(LionFireEnvironment.ProgramDataDir, "Results");

            //var filename = DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss.fff ") + this.GetType().Name + " " + Symbol.Code + " " + id;
            var sym =
#if cAlgo
            MarketSeries.SymbolCode;
#else
            Template.Symbol;
#endif

            var tradesPerMonth = (args.TotalTrades / (timeSpan.TotalDays / 31)).ToString("F1");
            var filename = fitness.ToString("00.0") + $"ad {tradesPerMonth}tpm {timeSpan.TotalDays.ToString("F0")}d  bot={this.GetType().Name} sym={sym} id={id}";
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

#if !cAlgo
        public Positions BotPositions
        {
            get
            {
                return botPositions;
            }
        }
        protected Positions botPositions = new Positions();
#endif

        //        public new Positions Positions
        //        {
        //#if cAlgo
        //            get { return base.Positions; }
        //#else
        //            get{return this.positions;}
        //#endif
        //        }
        //#if !cAlgo
        //        // Positions just for this bot
        //        private Positions positions =  new Positions<Position>();
        //#endif

#region Position Events

        public event Action<PositionEvent> BotPositionChanged;

        protected virtual void RaisePositionEvent(PositionEvent e)
        {
            BotPositionChanged?.Invoke(e);
        }

#endregion

#region Position Sizing


        // MOVE?
        public long VolumeToStep(long amount, long step = 0)
        {
            if (step == 0) step = Symbol.VolumeStep;
            return amount - (amount % Symbol.VolumeStep);
        }


        private string GetFromCurrency(string symbolCode)
        {
            string fromCurrency;
            switch (symbolCode)
            {
                case "AUS200":
                    fromCurrency = "AUD";
                    break;
                case "US30":
                case "US500":
                case "US2000":
                case "USTEC":
                    fromCurrency = "USD";
                    break;
                case "DE30":
                case "ES35":
                case "F40":
                case "IT40":
                case "STOXX50":
                    fromCurrency = "EUR";
                    break;
                case "UK100":
                    fromCurrency = "GBP";
                    break;
                case "JP225":
                    fromCurrency = "JPY";
                    break;
                case "HK50":
                    fromCurrency = "HKD";
                    break;
                default:
                    fromCurrency = Symbol.Code.Substring(3);
                    break;
            }
            return fromCurrency;
        }

        /// <summary>
        /// PositionPercentOfEquity
        /// </summary>
        /// <param name="tradeType"></param>
        /// <returns></returns>
        public long GetPositionVolume(TradeType tradeType = TradeType.Buy)
        {
            if (Symbol.VolumeStep != Symbol.VolumeMin)
            {
                throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            }

            var price = (tradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask);
            long volume_MinPositionSize = long.MinValue;

            if (Template.MinPositionSize > 0)
            {
                volume_MinPositionSize = Symbol.VolumeMin + (Template.MinPositionSize - 1) * Symbol.VolumeStep;
            }

            long volume_PositionPercentOfEquity = long.MinValue;

            if (Template.PositionPercentOfEquity > 0)
            {
                string fromCurrency = GetFromCurrency(Symbol.Code);
                var toCurrency = Account.Currency;

                var equityRiskAmount = this.Account.Equity * (Template.PositionPercentOfEquity / 100.0);

                var quantity = (long)equityRiskAmount;
                quantity = VolumeToStep(quantity);
                volume_PositionPercentOfEquity = (long)(quantity) ;


#if TRACE_RISK
                l.Debug($"[PositionRiskPercent] {BotConfig.PositionRiskPercent}% x {Account.Equity} equity = {equityRiskAmount}. SLDist({Symbol.Code.Substring(3)}): {stopLossDistance}, SLDist({Account.Currency}): {slDistAcct}, riskAmt/SLDist({Account.Currency}): {quantity} ");
#endif

                //var volumeByRisk = (long)(this.Account.Equity * BotConfig.MinPositionRiskPercent);
            }

            //long volume_PositionRiskPricePercent = long.MinValue;

            //if (BotConfig.PositionRiskPricePercent > 0)
            //{
            //    var maxRiskEquity = Account.Equity * (BotConfig.PositionRiskPricePercent / 100.0);

            //    var maxVolume = (maxRiskEquity / Math.Abs(stopLossValuePerVolumeStep)) * Symbol.VolumeStep;

            //    volume_MinPositionRiskPercent = (long)maxVolume; // Rounds down

            //    l.Info($"MinPositionRiskPercent - stopLossValuePerVolumeStep: {stopLossValuePerVolumeStep},  MaxVol: {maxVolume}  (Eq: {Account.Equity}, PositionRiskPricePercent: {BotConfig.PositionRiskPricePercent}, Max risk equity: {maxRiskEquity})");

            //    //var volumeByRisk = (long)(this.Account.Equity * BotConfig.MinPositionRiskPercent);
            //}

            //var volume = Math.Max(0, Math.Max(Math.Max(volume_MinPositionSize, volume_MinPositionRiskPercent), volume_PositionRiskPricePercent));
            var volume = Math.Max(0, Math.Max(volume_MinPositionSize, volume_PositionPercentOfEquity));
            if (volume == 0)
            {
                logger.LogWarning("volume == 0");
            }
            volume = VolumeToStep(volume);
            return volume;
        }

        // TODO: Also Use PositionPercentOfEquity, REFACTOR to combine with above method
        // MOVE?
        public long GetPositionVolume(double stopLossDistance, TradeType tradeType = TradeType.Buy)
        {
            if (Symbol.VolumeStep != Symbol.VolumeMin)
            {
                throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            }

            var stopLossValuePerVolumeStep = stopLossDistance * Symbol.VolumeStep;

            var price = (tradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask);
            long volume_MinPositionSize = long.MinValue;

            if (Template.MinPositionSize > 0)
            {
                volume_MinPositionSize = Symbol.VolumeMin + (Template.MinPositionSize - 1) * Symbol.VolumeStep;
            }

            long volume_MinPositionRiskPercent = long.MinValue;

            if (Template.PositionRiskPercent > 0)
            {
                string fromCurrency = GetFromCurrency(Symbol.Code);
                var toCurrency = Account.Currency;

                var stopLossDistanceAccountCurrency = ConvertToCurrency(stopLossDistance, fromCurrency, toCurrency);

                var equityRiskAmount = this.Account.Equity * (Template.PositionRiskPercent / 100.0);

                var quantity = (long)(equityRiskAmount / (stopLossDistanceAccountCurrency));
                quantity = VolumeToStep(quantity);

                volume_MinPositionRiskPercent = (long)(quantity);

                var slDistAcct = stopLossDistanceAccountCurrency.ToString("N3");

#if TRACE_RISK
                //l.Debug($"[PositionRiskPercent] {BotConfig.PositionRiskPercent}% x {Account.Equity} equity = {equityRiskAmount}. SLDist({Symbol.Code.Substring(3)}): {stopLossDistance}, SLDist({Account.Currency}): {slDistAcct}, riskAmt/SLDist({Account.Currency}): {quantity} ");
#endif

                //var volumeByRisk = (long)(this.Account.Equity * BotConfig.MinPositionRiskPercent);
            }

            long volume_PositionRiskPricePercent = long.MinValue;

            //if (BotConfig.PositionRiskPricePercent > 0)
            //{
            //    var maxRiskEquity = Account.Equity * (BotConfig.PositionRiskPricePercent / 100.0);

            //    var maxVolume = (maxRiskEquity / Math.Abs(stopLossValuePerVolumeStep)) * Symbol.VolumeStep;

            //    volume_MinPositionRiskPercent = (long)maxVolume; // Rounds down

            //    l.Info($"MinPositionRiskPercent - stopLossValuePerVolumeStep: {stopLossValuePerVolumeStep},  MaxVol: {maxVolume}  (Eq: {Account.Equity}, PositionRiskPricePercent: {BotConfig.PositionRiskPricePercent}, Max risk equity: {maxRiskEquity})");

            //    //var volumeByRisk = (long)(this.Account.Equity * BotConfig.MinPositionRiskPercent);
            //}

            var volume = Math.Max(0, Math.Max(Math.Max(volume_MinPositionSize, volume_MinPositionRiskPercent), volume_PositionRiskPricePercent));
            if (volume == 0)
            {
                logger.LogWarning("volume == 0");
            }
            volume = VolumeToStep(volume);
            return volume;
        }


        Dictionary<string, SortedList<int, double>> ExchangeRates = new Dictionary<string, SortedList<int, double>>();

        void InitExchangeRates() // MOVE?
        {
            {
                var rate = new SortedList<int, double>();
                //rate.Add("2011", 0.

                ExchangeRates.Add("USDJPY", rate);
            }
            {
                var rate = new SortedList<int, double>();
                //rate.Add("2011", 0.

                ExchangeRates.Add("USDCAD", rate);
            }
            {
                var rate = new SortedList<int, double>();
                //rate.Add("2011", 0.

                ExchangeRates.Add("EURUSD", rate);
            }
        }

        public bool UseParCurrencyConversionFallback = true;

        public void ValidateCurrency(string currency, string paramName)
        {
            if (String.IsNullOrWhiteSpace(currency)) { throw new ArgumentException($"{paramName} invalid currency: {currency}"); }
            if (char.IsDigit(currency[0])) { throw new ArgumentException($"{paramName} invalid currency: {currency}"); }

        }


        // MOVE
        public double ConvertToCurrency(double amount, string fromCurrency, string toCurrency = null)
        {
            ValidateCurrency(fromCurrency, nameof(fromCurrency));
            ValidateCurrency(toCurrency, nameof(toCurrency));

            if (toCurrency == null) toCurrency = Account.Currency;
            if (fromCurrency == toCurrency) return amount;

            var conversionSymbolCode = fromCurrency + toCurrency;
            var conversionSymbolCodeInverse = toCurrency + fromCurrency;

            if (Symbol.Code == conversionSymbolCode)
            {
                return Symbol.Bid * amount;
            }
            else if (Symbol.Code == conversionSymbolCodeInverse)
            {
                return amount / Symbol.Ask;
            }

            if (IsBacktesting)
            {
                if (ExchangeRates != null && ExchangeRates.ContainsKey(conversionSymbolCode))
                {
                    var rates = ExchangeRates[conversionSymbolCode];
                    double rate = 0;
                    if (rates.ContainsKey(Server.Time.Year))
                    {
                        rate = rates[Server.Time.Year];
                        var symbolAmount = amount / rate;
                        return symbolAmount;
                    }
                    else
                    {
                        // REFACTOR - deduplicate with below
                        if (UseParCurrencyConversionFallback)
                        {
                            logger.LogWarning($"Warning: Using par for currency conversion from {fromCurrency} to {toCurrency} --   Not implemented: No exchange rate data available for {conversionSymbolCode} for {Server.Time}");
                            return amount;
                        }
                        else
                        {
                            throw new NotImplementedException($"No exchange rate data available for {conversionSymbolCode} for {Server.Time}");
                        }
                    }
                }
                else
                {
                    // REFACTOR - deduplicate with above
                    if (UseParCurrencyConversionFallback)
                    {
                        logger.LogWarning($"Warning: Using par for currency conversion from {fromCurrency} to {toCurrency} --   Not implemented: No exchange rate data available for {conversionSymbolCode} for {Server.Time}");
                        return amount;
                    }
                    else
                    {
                        throw new NotImplementedException($"No exchange rate data available for {conversionSymbolCode} for {Server.Time}");
                    }

                    //throw new NotImplementedException($"cAlgo doesn't support currency conversion in backtesting.  Require conversion from {Symbol.Code.Substring(3)} to {toCurrency}");
                }
            }
            else
            {
                Symbol conversionSymbol = MarketData.GetSymbol(conversionSymbolCode);
                var symbolAmount = amount / conversionSymbol.Bid;
                return symbolAmount;
            }
        }


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

#region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#endregion

#endif

#endregion
    }


}
