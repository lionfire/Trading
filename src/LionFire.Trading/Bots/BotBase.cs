//#define BackTestResult_Debug
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
using LionFire.Threading.Tasks;
using System.Collections.ObjectModel;
using LionFire.ExtensionMethods;
using LionFire.States;
#if cAlgo
using cAlgo.API.Internals;
using cAlgo.API;
#endif
using LionFire.Instantiating;


namespace LionFire.Trading.Bots
{
    // TODO: Rename BotBase to SingleSeriesBotBase  and make a new BotBase that is more generic

    //public class MBotModeChanged
    //{
    //    public IBot Bot { get; set; }

    //}

    [InstantiatorType(typeof(PBot))]
    [State]
    public partial class BotBase<TBotType> : BotBase
        //, ITemplateInstance<TBot>
        , IBot
where TBotType : TBot, ITBot, new()
    {
        TBot IBot.Template { get { return Template; } set { Template = (TBotType)value; } }

        public  TBotType Template { get; set; } = new TBotType();
        protected override ITBot TBot => Template;

        // FUTURE: Allow non-templated bots?
        //public override TBot Template { get { return this.Template; } set { Template = value; } }
    }
    

    public class Bot : BotBase, ITBot
    {
        protected override ITBot TBot { get { return this; } }

        public string Id { get; set; }

    }

    public partial abstract class BotBase :
#if cAlgo
    Robot,
#endif
        // REVIEW OPTIMIZE - eliminate INPC for cAlgo?
        IInitializable, INotifyPropertyChanged, IExecutableEx
    {
        protected abstract ITBot TBot { get; }
        //public virtual TBot Template { get { return null;  } set { } }

        public string Version { get; set; } = "0.0.0";
        public virtual string BotName => this.GetType().Name;

        protected DateTime? StartDate = null;
        protected DateTime? EndDate = null;

        #region Configuration

        public LosingTradeLimiterConfig LosingTradeLimiterConfig { get; set; } = new LosingTradeLimiterConfig();

        #region Modes


        [State]
        public BotMode Modes
        {
            get
            {
#if cAlgo
                return Account.IsLive ? BotMode.Live : BotMode.Demo; // REVIEW
#else
                return mode;
#endif
            }
            set
            {
                if (mode == value) return;

                if (this.IsStarted())
                {
                    this.Restart(actionDuringShutdown: () => mode = value).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                {
                    mode = value;
#if !cAlgo
                    if (ExecutionStateFlags.HasFlag(ExecutionStateFlags.Autostart) && mode != BotMode.None)
                    {
                        TaskManager.OnNewTask(Start(), TaskFlags.Unowned);
                    }
#endif
                }
                if (mode == BotMode.None)
                {
#if !cAlgo
                    if (this.IsStarted())
                    {
                        TaskManager.OnNewTask(this.Stop(), TaskFlags.Unowned);
                    }
#endif
                }
                OnPropertyChanged(nameof(Modes));
            }
        }
        private BotMode mode;

        //#endif

        #endregion

        #region Derived

        //public bool IsScanner { get { return Mode.HasFlag(BotMode.Paper); } }

        #endregion

        #endregion

        #region Lifecycle

        //public IInstantiator ToInstantiator(InstantiationContext context = null)
        //{
        //    context.Dependencies.TryAdd(Template);

        //    return new InstantiationPipeline
        //    {
        //        new PBot
        //        {
        //            Id = Template.Id,
        //            TypeName = Template.GetType().FullName,
        //            DesiredExecutionStateEx = DesiredExecutionStateEx,
        //        },
        //        new StateRestorer(this),
        //    };
        //}
        //public IEnumerable<IInstantiator> Instantiators
        //{
        //    get
        //    {
        //        //yield return Template.ToInstantiator();
        //        yield return new PBot(this);
        //        yield return this.ToStateRestorer();
        //    }
        //}

        public BotBase()
        {
#if cAlgo
            LionFireEnvironment.ProgramName = "Trading";
#endif
            InitExchangeRates();
        }

        public static bool LogIfNoTemplate = true;
        public
#if !cAlgo
        override async
#endif
         Task<bool> Initialize()
        {
#if !cAlgo
            if (!await base.Initialize().ConfigureAwait(false)) return false;
#endif
            logger = this.GetLogger(this.ToString().Replace(' ', '.'), TBot != null ? TBot.Log : LogIfNoTemplate);
#if !cAlgo
            return true;
#else
            return Task.FromResult(true);
#endif
        }

        public bool GotTick;
        
        // Handler for LionFire.  Invoked by cAlgo's OnStart
#if cAlgo
        protected virtual async Task OnStarting()
#else
        protected override Task OnStarting()
#endif
        {
            if (TBot == null) tBot = new TBot();

            if (IsBacktesting && String.IsNullOrWhiteSpace(TBot.Id))
            {
                TBot.Id = IdUtils.GenerateId();
            }
            StartDate = null;
            EndDate = null;
#if cAlgo
            await Initialize();
#endif

            logger?.LogInformation($"------- START {this} -------");
#if cAlgo
#else
            return Task.CompletedTask;
#endif
        }
        partial void OnStarting_();

#if cAlgo
        protected virtual Task OnStarted()
#else
        protected override Task OnStarted()
#endif
        {
            //#if cAlgo
            //#else
            return Task.CompletedTask;
            //#endif
        }

        #endregion

        #region State

        #region Execution State
#if cAlgo

        public ExecutionStateEx State
        {
            get { return state; }
            protected set
            {
                if (state == value) return;
                state = value;
                StateChangedToFor?.Invoke(state, this);
            }
        }
        private ExecutionStateEx state;

        public event Action<ExecutionStateEx, object> StateChangedToFor;
#endif
        #endregion


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
                return count < TBot.MaxLongPositions;
            }
        }
        public bool CanOpenShort
        {
            get
            {
                var count = Positions.Where(p => p.TradeType == TradeType.Sell).Count();
                return count < TBot.MaxShortPositions;
            }
        }
        public bool CanOpen
        {
            get
            {
#if !cAlgo
                if (Account.IsDemo && !Modes.HasFlag(BotMode.Demo))
                {
                    return false;
                }
                if (!Account.IsDemo && !Modes.HasFlag(BotMode.Live))
                {
                    return false;
                }
#endif
                var count = Positions.Count;
                return TBot.MaxOpenPositions == 0 || count < TBot.MaxOpenPositions;
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

        public const string NoTicksIdPrefix = "NT-";

#if cAlgo
        protected override
#else
        public virtual
#endif
         double GetFitness(GetFitnessArgs args)
        {

            try
            {
                var dd = args.MaxEquityDrawdownPercentages;
                dd = Math.Max(FitnessMinDrawdown, dd);

                var initialBalance = args.History.Count == 0 ? args.Equity : args.History[0].Balance - args.History[0].NetProfit;
                if (dd > FitnessMaxDrawdown || args.Equity < initialBalance) { return -dd; }

                var botType = this.GetType().FullName;
#if cAlgo
                if (this.GetType().FullName.StartsWith("cAlgo."))
                {
                    botType = this.GetType().GetTypeInfo().BaseType.FullName;
                }
                if (TBot.TimeFrame == null)
                {
                    TBot.TimeFrame = this.TimeFrame.ToShortString();
                }
                if (TBot.Symbol == null)
                {
                    TBot.Symbol = this.Symbol.Code;
                }

                if (!StartDate.HasValue || !EndDate.HasValue)
                {
                    throw new ArgumentException("StartDate or EndDate not set.  Are you calling base.OnBar in OnBar?");
                }
#endif

                if(string.IsNullOrWhiteSpace(TBot.Id)) TBot.Id = IdUtils.GenerateId();

                if (!GotTick && !TBot.Id.StartsWith(NoTicksIdPrefix)) { TBot.Id = NoTicksIdPrefix + TBot.Id; }
                var backtestResult = new BacktestResult()
                {
                    BacktestDate = DateTime.UtcNow,
                    BotType = botType,
                    BotConfigType = this.TBot.GetType().AssemblyQualifiedName,
                    Config = this.TBot,
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

                    var minTrades = TBot.BacktestMinTradesPerMonth;

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

#if BackTestResult_Debug
                this.BacktestResult = backtestResult;
#endif
                return fitness;

            }
            catch (Exception ex)
            {
                Logger?.LogError("GetFitness threw exception: " + ex);
                throw;
                //return 0;
            }
        }

#if BackTestResult_Debug
        public BacktestResult BacktestResult { get; set; }
#endif

        protected virtual void DoLogBacktest(GetFitnessArgs args, BacktestResult backtestResult)
        {
            double fitness = backtestResult.Fitness;
            double initialBalance = backtestResult.InitialBalance;
            TimeSpan timeSpan = backtestResult.Duration;
            double aroi = backtestResult.Aroi;

            if (!double.IsNaN(TBot.LogBacktestThreshold) && fitness > TBot.LogBacktestThreshold)
            {

                BacktestLogger = this.GetLogger(this.ToString().Replace(' ', '.') + ".Backtest");

                try
                {
                    string resultJson = "";
                    resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(backtestResult);

                    var profit = args.Equity / initialBalance;

                    this.BacktestLogger?.LogInformation($"${args.Equity} ({profit.ToString("N1")}x) #{args.History.Count} {args.MaxEquityDrawdownPercentages.ToString("N2")}%dd [from ${initialBalance.ToString("N2")} to ${args.Equity.ToString("N2")}] [fit {fitness.ToString("N1")}] {Environment.NewLine} result = {resultJson} ");
                    var id = TBot.Id;


                    backtestResult.GetAverageDaysPerTrade(args);



                    SaveResult(args, backtestResult, fitness, resultJson, id, timeSpan);
                }
                catch (Exception ex)
                {
                    this.BacktestLogger?.LogError(ex.ToString());
                    throw;
                }
            }
        }

#if !cAlgo
        public TimeFrame TimeFrame => (this as IHasSingleSeries)?.MarketSeries?.TimeFrame;
#endif

        public string BacktestResultSaveDir(TimeFrame timeFrame) => Path.Combine(LionFireEnvironment.AppProgramDataDir, "Results", Symbol.Code, this.BotName, TimeFrame.ToShortString());

        public static bool CreateResultsDirIfMissing = true;

        private async void SaveResult(GetFitnessArgs args, BacktestResult backtestResult, double fitness, string json, string id, TimeSpan timeSpan)
        {

            //var filename = DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss.fff ") + this.GetType().Name + " " + Symbol.Code + " " + id;
            var sym =
#if cAlgo
            MarketSeries.SymbolCode;
#else
            TBot.Symbol;
#endif

            var tf =
#if cAlgo
                TimeFrame.ToShortString();
#else
            (this as IHasSingleSeries)?.MarketSeries?.TimeFrame?.Name;
#endif



            var tradesPerMonth = (args.TotalTrades / (timeSpan.TotalDays / 31)).ToString("F1");
            var filename = fitness.ToString("00.0") + $"ad {tradesPerMonth}tpm {timeSpan.TotalDays.ToString("F0")}d  {backtestResult.AverageDaysPerWinningTrade.ToString("F2")}adwt bot={this.GetType().Name} sym={sym} tf={tf} id={id}";
            var ext = ".json";
            int i = 0;
            var dir = BacktestResultSaveDir(TimeFrame);
            var path = Path.Combine(dir, filename + ext);
            if (CreateResultsDirIfMissing && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            for (; File.Exists(path); i++, path = Path.Combine(dir, filename + $" ({i})" + ext)) ;
            using (var sw = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                await sw.WriteAsync(json).ConfigureAwait(false);
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
            if (step == 0) step = 1; // REVIEW
            return amount - (amount % step);
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
            var volumeStep = Symbol.VolumeStep;
            if (volumeStep == 0) volumeStep = 1;
            var volumeMin = Symbol.VolumeMin;
            if (volumeMin == 0) volumeMin = 1;

            if (volumeStep != volumeMin)
            {
                throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            }

            var price = (tradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask);
            long volume_MinPositionSize = long.MinValue;

            if (TBot.MinPositionSize > 0)
            {
                volume_MinPositionSize = Symbol.VolumeMin + (TBot.MinPositionSize - 1) * volumeStep;
            }

            long volume_PositionPercentOfEquity = long.MinValue;

            if (TBot.PositionPercentOfEquity > 0)
            {
                string fromCurrency = GetFromCurrency(Symbol.Code);
                var toCurrency = Account.Currency;

                var equityRiskAmount = this.Account.Equity * (TBot.PositionPercentOfEquity / 100.0);

                var quantity = (long)equityRiskAmount;
                quantity = VolumeToStep(quantity);
                volume_PositionPercentOfEquity = (long)(quantity);


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
        public long GetPositionVolume(double? stopLossDistance, TradeType tradeType = TradeType.Buy)
        {
            if (!stopLossDistance.HasValue) return GetPositionVolume(tradeType);
            if (stopLossDistance == 0) throw new ArgumentException("stopLossDistance is 0.  Use null for no stop loss.");

            var volumeStep = Symbol.VolumeStep;
            if (volumeStep == 0) volumeStep = 1;
            var volumeMin = Symbol.VolumeMin;
            if (volumeMin == 0) volumeMin = 1;

            if (volumeStep != volumeMin)
            {
                throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            }

            var stopLossValuePerVolumeStep = stopLossDistance * volumeStep;

            var price = (tradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask);
            long volume_MinPositionSize = long.MinValue;

            if (TBot.MinPositionSize > 0)
            {
                volume_MinPositionSize = volumeMin + (TBot.MinPositionSize - 1) * volumeStep;
            }

            long volume_MinPositionRiskPercent = long.MinValue;

            if (TBot.PositionRiskPercent > 0)
            {
                string fromCurrency = GetFromCurrency(Symbol.Code);
                var toCurrency = Account.Currency;

                var stopLossDistanceAccountCurrency = ConvertToCurrency(stopLossDistance.Value, fromCurrency, toCurrency);

                var equityRiskAmount = this.Account.Equity * (TBot.PositionRiskPercent / 100.0);

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
            try
            {
                if (amount == 0) return 0;

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
            catch (Exception ex)
            {
                throw new Exception($"Error converting currency from {fromCurrency} to {toCurrency}", ex);
            }
        }

        #endregion

        #region Misc

        public virtual string Label
        {
            get
            {
                if (label != null) return label;
                return TBot?.Id + $" {this.GetType().Name}";
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

    // MOVE to separate class, reference in cTrader project
    public static class BacktestUtilities
    {
        public static void GetAverageDaysPerTrade(this BacktestResult results, GetFitnessArgs args)
        {
            double sum = 0;
            int trades = 0;
            double winSum = 0;
            int winningTrades = 0;

            double lossSum = 0;
            int losingTrades = 0;
            foreach (var trade in args.History)
            {
                if (double.IsNaN(trade.ClosingPrice)) continue;
                sum += (trade.ClosingTime - trade.EntryTime).TotalDays;
                trades++;

                if (trade.NetProfit >= 0)
                {
                    winSum += (trade.ClosingTime - trade.EntryTime).TotalDays;
                    winningTrades++;
                }
                else
                {
                    lossSum += (trade.ClosingTime - trade.EntryTime).TotalDays;
                    losingTrades++;
                }
            }

            results.AverageDaysPerTrade = sum / trades;
            results.AverageDaysPerWinningTrade = winSum / winningTrades;
            results.AverageDaysPerLosingTrade = lossSum / losingTrades;
        }
    }
}
