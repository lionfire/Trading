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
using Newtonsoft.Json;
using LionFire.Trading.Link.Messages;
#if cAlgo
using cAlgo.API.Indicators;
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
        ITBot IBot.Template { get { return Template; } set { Template = (TBotType)value; } }

        public new TBotType Template { get; set; } = new TBotType();
        protected override ITBot TBot => Template;

        protected override void CreateDefaultTBot()
        {
            Template = new TBotType();
        }
        // FUTURE: Allow non-templated bots?
        //public override TBot Template { get { return this.Template; } set { Template = value; } }
    }

    // Just use BotBase?  What's the point of this
    //public abstract class Bot : BotBase
    //    //, IBot
    //{
    //    protected override ITBot TBot { get { return this; } }

    //    public string Id { get; set; }

    //    //protected override void CreateDefaultTBot()
    //    //{
    //    //    Template = new TBotType();
    //    //}
    //}

    public abstract partial class BotBase :
#if cAlgo
    Robot,
#endif
        IBot,
        // REVIEW OPTIMIZE - eliminate INPC for cAlgo?
        IInitializable, INotifyPropertyChanged, IExecutableEx
    {
        #region (Static) Configuration

        public static bool LogIfNoTemplate = true;

        #endregion

        #region Identity

        public Guid Guid => guid;
        private readonly Guid guid = Guid.NewGuid();

        protected abstract ITBot TBot { get; }
        public virtual ITBot Template { get { return TBot; } set { throw new NotImplementedException(); } } // TODO: REFACTOR / remove this?
        protected abstract void CreateDefaultTBot();


        public string Version { get; set; } = "0.0.0";
        public virtual string BotName => this.GetType().Name;

        #endregion

        #region Parameters

        protected DateTime? StartDate = null;
        protected DateTime? EndDate = null;

        #endregion

        #region Settings

        public bool Diag { get; set; }

        #endregion

        #region State

        /// <summary>
        /// Turn this on during a backtest to make sure the bot is operating properly and to troubleshoot problems
        /// </summary>
        public bool GotTick;

        #endregion

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

        #region Link

        /// <summary>
        /// True if link is enabled for this session
        /// </summary>
        protected bool IsLinkEnabled;

        public bool Link { get; set; } = true;
        public bool LinkBacktesting { get; set; } = true;
        public TimeSpan LinkStatusInterval { get; set; } = TimeSpan.FromSeconds(10);

        protected async Task LinkSendInfo()
        {
            var msg = new MBotInfo()
            {
                Guid = this.guid,
                Id = this.Template?.Id,
                Time = DateTime.UtcNow,
                Broker = this.Account.BrokerName,
                Account = "??",
                Mode = this.Account.AccountType.ToString(), // this.Modes.ToString(),  // REVIEW TODO
                BotType = this.GetType().Name, // TODO: base class for cTrader
                Platform = "?",
                Hostname = Environment.MachineName,
                Symbol = this.Symbol.Code,
                Timeframe = this.TimeFrame.ToShortString(),
                StatusInterval = this.LinkStatusInterval
            };
            await BotLinkExtensions.Send(this, msg).ConfigureAwait(false);
        }

        protected async Task LinkSendStatus()
        {
            MStatus state = new MStatus()
            {
                Guid = this.guid,
                State = this.DesiredExecutionState,
                StatusMessage = "test msg",
                Balance = this.Account.Balance,
                Equity = this.Account.Equity,
                Margin = this.Account.Margin,
                MarginLevel = this.Account.MarginLevel,
                Time = DateTime.UtcNow
            };
            await BotLinkExtensions.Send(this, state).ConfigureAwait(false);
        }

        protected async void OnLinkStatusTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await LinkSendStatus().ConfigureAwait(false);
        }

        #endregion

        #region Configuration - External Settings

        #region External Settings

        public string BotSettingsDir { get; set; } = @"C:\st\Investing-Bots\cTrader\Settings";

        public string BotSettingsFileName = "settings.json";

        public string SettingsPath => Path.Combine(BotSettingsDir, BotSettingsFileName);

        static BotSettings BotSettingsCache;

        public static string Printable(string s)
        {
            return s.Replace("{", "{{").Replace("}", "}}");
        }

        public BotSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    /*{
                        var x = new BotSettings
                        {
                            LinkApi = "asdf"
                        };
                        var json = JsonConvert.SerializeObject(x);
                        Print("Serialz test:" + json);
                        File.WriteAllText(Path.GetFileNameWithoutExtension(SettingsPath) + "-test.json", json);
                    }*/
                    var settings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(SettingsPath));
#if cAlgo
                    Print("Loaded settings from " + SettingsPath + " " + Printable(settings.ToXamlAttribute()));
                    //Print(JsonConvert.SerializeObject(settings));
#endif
                    return settings;
                }
            }
            catch (Exception ex)
            {
#if cAlgo
                Print("Exception when loading settings: " + ex.ToString());
#endif
            }
            return null;
        }

        public static BotSettingsCache SettingsCache;

        public void LoadSettingsIfNeeded()
        {
            if (SettingsCache == null)
            {
                SettingsCache = new BotSettingsCache();
            }

            if (SettingsCache.IsExpired)
            {
                if (Diag)
                {
                    this.Logger.LogInformation("SettingsCache.IsExpired, Loading settings");

                }
                SettingsCache.Settings = LoadSettings();
            }
        }
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

        #region Construction

        public BotBase()
        {
#if cAlgo
            LionFireEnvironment.ProgramName = "Trading";
#endif

            InitExchangeRates();
        }

        #endregion

        #region Initialization

        public
#if !cAlgo
        override async
#endif
         Task<bool> Initialize()
        {
            IsLinkEnabled = Link && (!IsBacktesting || LinkBacktesting);

#if !cAlgo
            if (!await base.Initialize().ConfigureAwait(false)) return false;
#endif
            logger = this.GetLogger(this.ToString().Replace(' ', '.'), TBot != null ? TBot.Log : LogIfNoTemplate);

            if (Diag)
            {
                logger.LogInformation("Diagnostic mode enabled.");
            }

#if !cAlgo
            return true;
#else
            return Task.FromResult(true);
#endif
        }

        #endregion



        #region Start

        protected System.Timers.Timer linkStatusTimer;

        // Handler for LionFire.  Invoked by cAlgo's OnStart
#if cAlgo
        protected virtual async Task OnStarting()
#else
        protected override Task OnStarting()
#endif
        {
            if (TBot == null) CreateDefaultTBot();
            

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

            if (IsLinkEnabled)
            {
                LinkSendInfo().FireAndForget();
                linkStatusTimer = new System.Timers.Timer(LinkStatusInterval.TotalMilliseconds);
                linkStatusTimer.Elapsed += OnLinkStatusTimerElapsed;
                OnLinkStatusTimerElapsed(null,null);
                linkStatusTimer.Start();
            }

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
#if cAlgo
            InitializeIndicators_cAlgo();
#endif
            //#if cAlgo
            //#else
            return Task.CompletedTask;
            //#endif
        }

        #endregion

        #region Stop

        protected void OnBotStopping()
        {
            if (IsLinkEnabled)
            {
                LinkSendStatus();
                linkStatusTimer.Stop();
                linkStatusTimer.Elapsed -= OnLinkStatusTimerElapsed;
                linkStatusTimer = null;
            }
        }

        #endregion

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

        #region Indicators

#if cAlgo
        // TODO: REARCH Make additional indicators modular?

        AverageTrueRange Atr; // RENAME ATR
        AverageTrueRange DailyAtr;
#endif

        #endregion

        #region Entry

        #region Filters

        public bool PassesFilters(TradeType tt)
        {
            //#if !cAlgo
            //            throw new NotImplementedException();
            //#else
            foreach (var f in Filters)
            {

                if (!f(tt, MarketSeries))
                {
                    //Print("Failed " + tt + " filter:  pivot: " + DailyPivot);
                    return false;
                }
                //Print("Passed " + tt + " filter:  pivot: " + DailyPivot);
            }
            return true;
            //#endif
        }

        #endregion

        public void TryEnter(TradeType tt, double? slPips = null, double? tpPips = null, int? volume = null)
        {
            if (BotPositionCount >= TBot.MaxOpenPositions)
                return;
            if (BotLongPositionCount >= TBot.MaxLongPositions)
                return;
            if (BotShortPositionCount >= TBot.MaxShortPositions)
                return;

            if (!PassesFilters(tt))
                return;

#if cAlgo
            slPips = slPips.TightenPips(PointsToPips(DailyAtr.Result.Last(1) * (TBot.SLinDailyAtr)));

            tpPips = tpPips.TightenPips(PointsToPips(DailyAtr.Result.Last(1) * (TBot.TPinDailyAtr)));


            slPips = slPips.TightenPips(PointsToPips(Atr.Result.Last(1) * (TBot.SLinAtr)));

            tpPips = tpPips.TightenPips(PointsToPips(Atr.Result.Last(1) * (TBot.TPinAtr)));
#else
            if (TBot.SLinDailyAtr != 0 || TBot.TPinDailyAtr != 0 || TBot.SLinAtr != 0 || TBot.TPinAtr != 0)
            {
                throw new NotImplementedException("SL and TP Atr and DailyATR not implemented");
            }
#endif

            // REVIEW - Compare with SingleSeriesSignalBotBase _Open and consolidate?
            ExecuteMarketOrder(tt, Symbol, volume ?? GetPositionVolume(slPips, tt), Label, slPips, tpPips);
        }

        #endregion

        #region Positions

#if cAlgo
        public int BotPositionCount => BotPositions.Length;
#else
        public int BotPositionCount => BotPositions.Count;
#endif
        public IEnumerable<Position> BotLongPositions => BotPositions.Where(p => p.TradeType == TradeType.Buy);
        public int BotLongPositionCount => BotLongPositions.Count();
        public IEnumerable<Position> BotShortPositions => BotPositions.Where(p => p.TradeType == TradeType.Sell);
        public int BotShortPositionCount => BotShortPositions.Count();

#if !cAlgo
        public Positions BotPositions
        {
            get
            {
                throw new NotImplementedException("TODO: Review that BotPositions actually works in lionFire mode.");
                return botPositions;
            }
        }
        protected Positions botPositions = new Positions();
#else
        public Position[] BotPositions
        {
            get
            {
                return Positions.FindAll(Label);
            }
        }
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
        public long GetPositionVolume(double? stopLossDistance, TradeType tradeType)
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

        #region Position Methods

        public void CloseExistingOpposite(TradeType tt)
        {
            foreach (var p in BotPositions)
            {
                if (p.TradeType != tt)
                {
                    ClosePosition(p);
                }
            }
        }

        #endregion

        #endregion

        #region Position Management

        #endregion

        #region Symbol Utils

        public double? PointsToPips(double? points)
        {
            return Symbol.SymbolPointsToPips(points);
        }

        #endregion


        public MarketSeries MarketSeries // REVIEW RECENTCHANGE
        {
            get
            {
                return MarketData.GetSeries(this.TimeFrame);
            }
        }

        #region On-demand extra inputs: DailySeries

        public MarketSeries DailySeries
        {
            get
            {
                if (dailySeries == null)
                {
                    dailySeries = TimeFrame == TimeFrame.Daily ? MarketSeries : MarketData.GetSeries(TimeFrame.Daily);
                }
                return dailySeries;
            }
        }
        private MarketSeries dailySeries;

        #endregion

        #region Filters

        public List<Func<TradeType, MarketSeries, bool>> Filters = new List<Func<TradeType, MarketSeries, bool>>();

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
                var botType = this.GetType().FullName;
#if cAlgo
                if (this.GetType().FullName.StartsWith("cAlgo."))
                {
                    botType = this.GetType().GetTypeInfo().BaseType.FullName;
                }
#endif

                if (Diag && args == null)
                {
                    var fakeBacktestResult = new BacktestResult()
                    {
                        BacktestDate = DateTime.UtcNow,
                        BotType = botType,
                        BotConfigType = this.TBot.GetType().AssemblyQualifiedName,
                        Config = this.TBot,
                        InitialBalance = 999999,
                        Start = StartDate.Value,
                        End = EndDate.Value,

                        AverageTrade = 5,
                        Equity = 5,

                        //History
                        LosingTrades = 5,
                        MaxBalanceDrawdown = 5,
                        MaxBalanceDrawdownPercentages = 5,
                        MaxEquityDrawdown = 5,
                        MaxEquityDrawdownPercentages = 5,
                        NetProfit = 5,
                        //PendingOrders
                        //Positions
                        ProfitFactor = 5,
                        SharpeRatio = 5,
                        SortinoRatio = 5,
                        TotalTrades = 5,
                        WinningTrades = 5,
                    };
                    DoLogBacktest(args, fakeBacktestResult);
                    return 5555.0;
                }
                var dd = args.MaxEquityDrawdownPercentages;
                dd = Math.Max(FitnessMinDrawdown, dd);

                var initialBalance = args.History.Count == 0 ? args.Equity : args.History[0].Balance - args.History[0].NetProfit;
                if (dd > FitnessMaxDrawdown || args.Equity < initialBalance) { return -dd; }


#if cAlgo
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

                if (string.IsNullOrWhiteSpace(TBot.Id)) TBot.Id = IdUtils.GenerateId();

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
                    //                logger?.LogInformation($"dd: {args.MaxEquityDrawdownPercentages }  Template.BacktestMinTradesPerMonth {Template.BacktestMinTradesPerMonth} tradesPerMonth {tradesPerMonth}");
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
                try
                {
                    BacktestLogger = this.GetLogger(this.ToString().Replace(' ', '.') + ".Backtest");
                }
                catch { } // EMPTYCATCH

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

    // MOVE
    public static class PipUtils
    {
        public static double? SymbolPointsToPips(this Symbol symbol, double? points)
        {
            if (!points.HasValue)
                return null;
            return points / symbol.PipSize;
        }

        public static double? TightenPips(this double? pips, double? otherPips, bool discardZeroes = true)
        {
            if (!pips.HasValue || (discardZeroes && pips.Value == 0))
                return otherPips;
            if (!otherPips.HasValue || (discardZeroes && otherPips.Value == 0))
                return pips;
            return Math.Min(pips.Value, otherPips.Value);
        }

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
