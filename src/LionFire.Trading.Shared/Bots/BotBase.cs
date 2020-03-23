//#define BackTestResult_Debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Reflection;
using LionFire.Execution;
using System.IO;
using LionFire.Trading.Backtesting;
using System.ComponentModel;
using LionFire.Threading.Tasks;
using LionFire.States;
#if NewtonsoftJson
using Newtonsoft.Json;
#endif
using LionFire.Trading.Link.Messages;
using LionFire.Trading.Link;
using LionFire.ExtensionMethods.Copying;
using LionFire.Threading;
using System.Text.Json;
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
        ITBot IBot.Template { get => Template; set => Template = (TBotType)value; }

        public new TBotType Template { get; set; } = new TBotType();
        protected override ITBot TBot => Template;

        protected override void CreateDefaultTBot() => Template = new TBotType();
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

        public static bool LogIfNoRuntimeSettings = true;

        #endregion

        #region Identity

        public Guid Guid => guid;
        private readonly Guid guid = Guid.NewGuid();

        protected abstract ITBot TBot { get; }
        public virtual ITBot Template { get => TBot; set => throw new NotImplementedException(); } // TODO: REFACTOR / remove this?
        protected abstract void CreateDefaultTBot();


        public string Version { get; set; } = "0.0.0";
        public virtual string BotName => GetType().Name;

        #endregion

        #region Parameters

        protected DateTime? StartDate = null;
        protected DateTime? EndDate = null;

        #endregion

        #region Settings

        public bool Diag => RuntimeSettings.Diag == true;

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
                if (mode == value)
                {
                    return;
                }

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

        public bool Link => RuntimeSettings?.Link == true;

        public TimeSpan LinkStatusInterval { get; set; } = TimeSpan.FromSeconds(10);

        protected async Task LinkSendInfo()
        {

            var msg = new MBotInfo()
            {
                Guid = guid,
                Id = Template?.Id,
                Time = DateTime.UtcNow,
                Broker = Account.BrokerName,
                IsLive = Account.IsLive,
                AccountType = Account.AccountType.ToString(),

#if cTrader
                AccountId = Account.Number.ToString(),
#else
                AccountId = this.Account.AccountId,
#endif
                //Mode = this.Account.AccountType.ToString(), // this.Modes.ToString(),  // REVIEW TODO
                BotType = GetType().Name, // TODO: base class for cTrader
                Platform = "?",
                Hostname = Environment.MachineName,
                Symbol = Symbol.Code,
                Timeframe = TimeFrame.ToShortString(),
                StatusInterval = LinkStatusInterval
            };
            await BotLinkExtensions.Send(this, msg).ConfigureAwait(false);
        }

        protected async Task LinkSendStatus()
        {

            double netPosition = 0;
            var botPositions = BotPositions;
            List<LinkPosition> positions = null;
            if (botPositions.Length > 0)
            {
                positions = new List<LinkPosition>();
                foreach (var p in botPositions)
                {
                    netPosition += p.TradeType == TradeType.Buy ? p.VolumeInUnits : -p.VolumeInUnits;

                    positions.Add(new LinkPosition()
                    {
                        Comment = p.Comment,
                        Commission = p.Commissions,
                        Label = p.Label,
                        NetProfit = p.NetProfit,
                        OpenTime = p.EntryTime,
                        Swap = p.Swap,
                        Symbol = p.SymbolCode,
                        TradeType = (LinkTradeType)Enum.Parse(typeof(LinkTradeType), p.TradeType.ToString()),
                        Volume = p.VolumeInUnits
                    });
                }
            }

            MStatus msg = new MStatus()
            {
                Guid = guid,
                Bid = Symbol.Bid,
                Ask = Symbol.Ask,
                State = DesiredExecutionState,
                //StatusMessage = "test msg",
                Balance = Account.Balance,
                Equity = Account.Equity,
                Margin = Account.Margin,
                MarginLevel = Account.MarginLevel,
                Time = DateTime.UtcNow,
                Positions = positions
            };
            await BotLinkExtensions.Send(this, msg).ConfigureAwait(false);
        }

        protected async void OnLinkStatusTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) => await LinkSendStatus().ConfigureAwait(false);

        #endregion

        #region Configuration - External Settings

        #region External Settings

        public string BotSettingsDir { get; set; } = @"C:\st\Investing-Bots\cTrader\Settings";

        public string BotSettingsFileName = "settings.json";

        public string SettingsPath => Path.Combine(BotSettingsDir, BotSettingsFileName);
        public string MachineSettingsPath => Path.Combine(BotSettingsDir, Environment.MachineName, BotSettingsFileName);

        //private static BotSettings BotSettingsCache;

        public static string Printable(string s) => s.Replace("{", "{{").Replace("}", "}}");

        public static T Deserialize<T>(string json)
        {
#if NewtonsoftJson
            return JsonConvert.DeserializeObject<BotSettings>(json);
#else
            return Utf8Json.JsonSerializer.Deserialize<T>(json);
#endif
        }
        public static void Serialize<T>(Stream stream, T obj)
        {
            throw new NotImplementedException();
//#if NewtonsoftJson
//            JsonConvert.SerializeObject<BotSettings>(json);
//#else
//            Utf8Json.JsonSerializer.Serialize<T>(json);
//#endif
        }

        public BotSettings LoadSettings()
        {
            try
            {
                var settings = new BotSettings();
                settings.AssignPropertiesFrom(BotSettings.Default);

                if (File.Exists(SettingsPath))
                {
                    settings.AssignNonNullPropertiesFrom(Deserialize<BotSettings>(File.ReadAllText(SettingsPath)));

#if cAlgo
                    Print("Loaded settings from " + SettingsPath + " " + Printable(settings.ToXamlAttribute()));
#endif
                }
                if (File.Exists(MachineSettingsPath))
                {
                    settings.AssignNonNullPropertiesFrom(Deserialize<BotSettings>(File.ReadAllText(MachineSettingsPath)));

#if cAlgo
                    Print("Loaded machine-specific settings from " + MachineSettingsPath + " " + Printable(settings.ToXamlAttribute()));
#endif
                }
                return settings;
            }
#if cAlgo
            catch (Exception ex)
            {
                Print("Exception when loading settings: " + ex.ToString());
#else
            catch (Exception)
            {
#endif
            }
            return null;
        }

        public static BotSettings RuntimeSettings => BotSettingsCache.Settings;

        public void LoadSettingsIfNeeded()
        {
            //if (SettingsCache == null)
            //{
            //    SettingsCache = new BotSettingsCache();
            //}

            if (BotSettingsCache.IsExpired || BotSettingsCache.Settings == null)
            {
                BotSettingsCache.Settings = LoadSettings();
#if cTrader
                if (Diag)
                {
                    Print("SettingsCache.IsExpired, Loaded settings");
                }
#endif
            }
            else
            {
#if cTrader
                if (Diag)
                {
                    Print("Using cached settings");
                }
#endif
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
#if cTrader
            if (!LionFireEnvironment.IsMainAppInfoSet)
            {
                LionFireEnvironment.MainAppInfo = new AppInfo()
                {
                    CompanyName = "LionFire",
                    ProgramName = "Trading",
                };
            }
#endif

            InitExchangeRates();
        }

        #endregion

        #region Initialization

        private void Link_OnPositionClosed(PositionClosedEventArgs args)
        {
            //var position = args.Position;
            //Print("Position closed with {0} profit", position.GrossProfit);
            LinkSendStatus().FireAndForget();
        }
        private void Link_OnPositionOpened(PositionOpenedEventArgs args)
        {
            //var position = args.Position;
            //Print("Position closed with {0} profit", position.GrossProfit);
            LinkSendStatus().FireAndForget();
        }

        public
#if !cAlgo
        override async
#endif

         Task<bool> Initialize()
        {
            logger = this.GetLogger(ToString().Replace(' ', '.'), RuntimeSettings?.Log ?? LogIfNoRuntimeSettings);

            IsLinkEnabled = Link && (!IsBacktesting || RuntimeSettings.LinkBacktesting == true);

            if (IsLinkEnabled)
            {
#if cAlgo
                Print("Link is enabled.  Will send position updates.");
#endif
                //this.BotPositionChanged
                Positions.Closed += Link_OnPositionClosed;
                Positions.Opened += Link_OnPositionOpened;
            }

#if !cAlgo
            if (!await base.Initialize().ConfigureAwait(false)) return false;
#endif

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
            if (TBot == null)
            {
                CreateDefaultTBot();
            }

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

            if (TBot.MinPositionSize == 0)
            {
                TBot.MinPositionSize = 1;
            }

            if (IsLinkEnabled)
            {
                LinkSendInfo().FireAndForget();
                linkStatusTimer = new System.Timers.Timer(LinkStatusInterval.TotalMilliseconds);
                linkStatusTimer.Elapsed += OnLinkStatusTimerElapsed;
                OnLinkStatusTimerElapsed(null, null);
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
                LinkSendStatus().FireAndForget();
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
            get => state;
            protected set
            {
                if (state == value)
                {
                    return;
                }

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
                var count = BotPositions.Where(p => p.TradeType == TradeType.Buy).Count();
                return TBot.MaxLongPositions == 0 || count < TBot.MaxLongPositions;
            }
        }
        public bool CanOpenShort
        {
            get
            {
                var count = BotPositions.Where(p => p.TradeType == TradeType.Sell).Count();
                return TBot.MaxShortPositions == 0 || count < TBot.MaxShortPositions;
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
                var count = BotPositions.Length;
                return TBot.MaxOpenPositions == 0 || count < TBot.MaxOpenPositions;
            }
        }

        public bool CanOpenType(TradeType tradeType)
        {
            if (!CanOpen)
            {
                return false;
            }

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

        private AverageTrueRange Atr; // RENAME ATR
        private AverageTrueRange DailyAtr;
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
            {
                return;
            }

            if (BotLongPositionCount >= TBot.MaxLongPositions)
            {
                return;
            }

            if (BotShortPositionCount >= TBot.MaxShortPositions)
            {
                return;
            }

            if (!PassesFilters(tt))
            {
                return;
            }

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

        public int BotPositionCount => BotPositions.Length;

        public IEnumerable<Position> BotLongPositions => BotPositions.Where(p => p.TradeType == TradeType.Buy);
        public int BotLongPositionCount => BotLongPositions.Count();
        public IEnumerable<Position> BotShortPositions => BotPositions.Where(p => p.TradeType == TradeType.Sell);
        public int BotShortPositionCount => BotShortPositions.Count();

#if !cAlgo
        public Position[] BotPositions
        {
            get
            {

                throw new NotImplementedException("TODO: Review that BotPositions actually works in lionFire mode.");
                //return botPositions;
            }
        }
        //protected Position[] botPositions = new Positions();
#else
        public Position[] BotPositions => Positions.FindAll(Label);
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

        protected virtual void RaisePositionEvent(PositionEvent e) => BotPositionChanged?.Invoke(e);

        #endregion

        #region Position Sizing


        // MOVE?
        public double VolumeToStep(double amount, double step = 0)
        {
            if (step == 0)
            {
                step = Symbol.VolumeInUnitsStep;
            }

            if (step == 0)
            {
                step = 1; // REVIEW
            }

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

        public double GetFixedPositionVolume()
        {
            //var volumeStep = Symbol.VolumeInUnitsStep;
            //if (volumeStep == 0)
            //{
            //    volumeStep = 1;
            //}

            //var volumeMin = Symbol.VolumeInUnitsMin;
            //if (volumeMin == 0)
            //{
            //    volumeMin = 1;
            //}

            //if (volumeStep != volumeMin)
            //{
            //    throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            //}

            var tBotMin = TBot.MinPositionSize;
            return Symbol.VolumeInUnitsMin * (tBotMin > 0 ? tBotMin : 1.0); // Simple, based on min size

            //return Symbol.VolumeInUnitsMin + (effectiveMinPositionSize - 1) * Symbol.VolumeInUnitsStep;


            #region Rounding (unnecessary here)

            //volume = VolumeToStep(volume); // Unnecessary here

            //#if cTrader
            //            volume = this.Symbol.NormalizeVolumeInUnits(volume, RoundingMode.Down);
            //#endif
            //return volume;

            #endregion

        }

        /// <summary>
        /// PositionPercentOfEquity (REVIEW - cTrader recently changed from long to double)
        /// </summary>
        /// <param name="tradeType"></param>
        /// <returns></returns>
        public double GetPositionVolume(TradeType tradeType = TradeType.Buy)
        {
            var volumeStep = Symbol.VolumeInUnitsStep;
            if (volumeStep == 0)
            {
                volumeStep = 1;
            }

            var volumeMin = Symbol.VolumeInUnitsMin;
            if (volumeMin == 0)
            {
                volumeMin = 1;
            }

            if (volumeStep != volumeMin)
            {
                throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            }

            var price = (tradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask);
            double volume_MinPositionSize = double.MinValue;

            if (TBot.MinPositionSize > 0)
            {
                volume_MinPositionSize = Symbol.VolumeInUnitsMin + (TBot.MinPositionSize - 1) * volumeStep;
            }

            double volume_PositionPercentOfEquity = double.MinValue;

            if (TBot.PositionPercentOfEquity > 0)
            {
                string fromCurrency = GetFromCurrency(Symbol.Code);
                var toCurrency = Account.Currency;

                var equityRiskAmount = Account.Equity * (TBot.PositionPercentOfEquity / 100.0);

                var quantity = equityRiskAmount;
                quantity = VolumeToStep(quantity);
                volume_PositionPercentOfEquity = quantity;


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
#if cTrader
            volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.Down);
#endif
            return volume;
        }

        // TODO: Also Use PositionPercentOfEquity, REFACTOR to combine with above method
        // MOVE?
        public double GetPositionVolume(double? stopLossDistance, TradeType tradeType)
        {
            if (!stopLossDistance.HasValue)
            {
                return GetPositionVolume(tradeType);
            }

            if (stopLossDistance == 0)
            {
                throw new ArgumentException("stopLossDistance is 0.  Use null for no stop loss.");
            }

            var volumeStep = Symbol.VolumeInUnitsStep;
            if (volumeStep == 0)
            {
                volumeStep = 1;
            }

            var volumeMin = Symbol.VolumeInUnitsMin;
            if (volumeMin == 0)
            {
                volumeMin = 1;
            }

            if (volumeStep != volumeMin)
            {
                throw new NotImplementedException("Position sizing not implemented when Symbol.VolumeStep != Symbol.VolumeMin");
            }

            var stopLossValuePerVolumeStep = stopLossDistance * volumeStep;

            var price = (tradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask);
            double volume_MinPositionSize = double.MinValue;

            if (TBot.MinPositionSize > 0)
            {
                volume_MinPositionSize = volumeMin + (TBot.MinPositionSize - 1) * volumeStep;
            }

            double volume_MinPositionRiskPercent = double.MinValue;

            if (TBot.PositionRiskPercent > 0)
            {
                string fromCurrency = GetFromCurrency(Symbol.Code);
                var toCurrency = Account.Currency;

                var stopLossDistanceAccountCurrency = ConvertToCurrency(stopLossDistance.Value, fromCurrency, toCurrency);

                var equityRiskAmount = Account.Equity * (TBot.PositionRiskPercent / 100.0);

                var quantity = equityRiskAmount / (stopLossDistanceAccountCurrency);
                quantity = VolumeToStep(quantity);

                volume_MinPositionRiskPercent = quantity;

                var slDistAcct = stopLossDistanceAccountCurrency.ToString("N3");

#if TRACE_RISK
                //l.Debug($"[PositionRiskPercent] {BotConfig.PositionRiskPercent}% x {Account.Equity} equity = {equityRiskAmount}. SLDist({Symbol.Code.Substring(3)}): {stopLossDistance}, SLDist({Account.Currency}): {slDistAcct}, riskAmt/SLDist({Account.Currency}): {quantity} ");
#endif

                //var volumeByRisk = (long)(this.Account.Equity * BotConfig.MinPositionRiskPercent);
            }

            double volume_PositionRiskPricePercent = double.MinValue;

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

        private Dictionary<string, SortedList<int, double>> ExchangeRates = new Dictionary<string, SortedList<int, double>>();

        private void InitExchangeRates() // MOVE?
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
                if (amount == 0)
                {
                    return 0;
                }

                ValidateCurrency(fromCurrency, nameof(fromCurrency));
                ValidateCurrency(toCurrency, nameof(toCurrency));

                if (toCurrency == null)
                {
                    toCurrency = Account.Currency;
                }

                if (fromCurrency == toCurrency)
                {
                    return amount;
                }

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

        public void CloseExistingOpposite(TradeType tt, bool? inProfit = null)
        {
            foreach (var p in BotPositions)
            {
                if (p.TradeType == tt)
                {
                    continue;
                }

                if (inProfit.HasValue)
                {
                    if (inProfit.Value)
                    {
                        if (p.NetProfit <= 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (p.NetProfit >= 0)
                        {
                            continue;
                        }
                    }
                }
                ClosePosition(p);
            }
        }

        #endregion

        #endregion

        #region Position Management

        public void CloseAllIfBacktesting()
        {
            if (IsBacktesting)
            {
                foreach (var p in BotPositions)
                {
                    ClosePosition(p);
                }
            }
        }

        #endregion

        #region Symbol Utils

        public double? PointsToPips(double? points) => Symbol.SymbolPointsToPips(points);

        #endregion

#if !cTrader // MOVE
        public MarketSeries MarketSeries // REVIEW RECENTCHANGE
        {
            get
            {
                return MarketData.GetSeries(this.TimeFrame);
            }
        }
#endif

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
        public const double FitnessMinDrawdown = 0.1;

        public const string NoTicksIdSuffix = "-NT";

#if cAlgo
        protected override
#else
        public virtual
#endif
         double GetFitness(GetFitnessArgs args)
        {

            try
            {
                var botType = GetType().FullName;
#if cAlgo
                if (GetType().FullName.StartsWith("cAlgo."))
                {
                    botType = GetType().GetTypeInfo().BaseType.FullName;
                }
#endif

                if (Diag && args == null)
                {
                    var fakeBacktestResult = new BacktestResult()
                    {
                        Broker = Account.BrokerName,
                        BacktestDate = DateTime.UtcNow,
                        BotType = botType,
                        BotConfigType = TBot.GetType().AssemblyQualifiedName,
                        BotName = TBot.Name,
                        Config = TBot,
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
                var ddForFitness = args.MaxEquityDrawdownPercentages;
                ddForFitness = Math.Max(FitnessMinDrawdown, ddForFitness);

                var initialBalance = args.History.Count == 0 ? args.Equity : args.History[0].Balance - args.History[0].NetProfit;
                double fitness;

                if (!TBot.AllowLong && !TBot.AllowShort)
                {
                    return 0.0;
                }

#if cAlgo
                if (TBot.TimeFrame == null)
                {
                    TBot.TimeFrame = TimeFrame.ToShortString();
                }
                if (TBot.Symbol == null)
                {
                    TBot.Symbol = Symbol.Code;
                }

                if (!StartDate.HasValue)
                {
                    Print("StartDate not set.  Are you calling base.OnBar in OnBar?");
                return -99999;
                    throw new ArgumentException("StartDate not set.  Are you calling base.OnBar in OnBar?");
                }
                if (!EndDate.HasValue)
                {
                    throw new ArgumentException("EndDate not set.  Are you calling base.OnBar in OnBar?");
                }
#endif

                if (!EndDate.HasValue || !StartDate.HasValue)
                {
                    return -1000.0;
                }

                if (ddForFitness > FitnessMaxDrawdown || args.Equity < initialBalance) { return -ddForFitness; }


                if (string.IsNullOrWhiteSpace(TBot.Id))
                {
                    TBot.Id = IdUtils.GenerateId() + "-Gen";
                }

                if (!GotTick && !TBot.Id.EndsWith(NoTicksIdSuffix)) { TBot.Id = TBot.Id + NoTicksIdSuffix; }
                var backtestResult = new BacktestResult()
                {
                    Broker = Account.BrokerName,
                    BacktestDate = DateTime.UtcNow,
                    BotType = botType,
                    BotConfigType = TBot.GetType().AssemblyQualifiedName,
                    Config = TBot,
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

                var duration = EndDate.Value - StartDate.Value; // Slight optimization of backtestResult.Duration
                var totalMonths = duration.TotalDays / 31;
                var tradesPerMonth = backtestResult.TradesPerMonth;

                //var aroi = (args.NetProfit / initialBalance) / (timeSpan.TotalDays / 365);
                var aroi = backtestResult.Aroi;

                //if (ddForFitness < //args.LosingTrades == 0)
                //{
                //    fitness = aroi;
                //}
                //else
                //{
                fitness = aroi / ddForFitness;
                //}

                var minTrades = RuntimeSettings.MinTradesPerMonth;

                //#if cAlgo
                //                logger?.LogInformation($"dd: {args.MaxEquityDrawdownPercentages }  Template.BacktestMinTradesPerMonth {Template.BacktestMinTradesPerMonth} tradesPerMonth {tradesPerMonth}");
                //#endif
                //Print("tpm " + tradesPerMonth + " fitness: " 
                //    + fitness + " aroi " + aroi + " ddForFitness " + ddForFitness + " NetProfit " 
                //    + backtestResult.NetProfit + " InitialBalance "  
                //    + backtestResult.InitialBalance + " Duration.TotalDays " + backtestResult.Duration.TotalDays);

                if (minTrades > 0 && tradesPerMonth < minTrades && fitness > 0 && RuntimeSettings.BacktestMinTradesPerMonthExponent.HasValue)
                {
                    fitness *= Math.Pow(tradesPerMonth / minTrades.Value, RuntimeSettings.BacktestMinTradesPerMonthExponent.Value);
                }

                //#if cAlgo
                //            Print($"Fitness: {StartDate.Value} - {EndDate.Value} profit: {args.NetProfit} years: {((EndDate.Value - StartDate.Value).TotalDays / 365)} constrained eqDD:{dd} aroi:{aroi} aroi/DD:{aroiVsDD}");
                //#endif

                fitness *= 100.0;

                backtestResult.Fitness = fitness;
                DoLogBacktest(args, backtestResult);


#if BackTestResult_Debug
                this.BacktestResult = backtestResult;
#endif
                backtestResult.Dispose();
                backtestResult = null;
                return fitness;

            }
            catch (Exception ex)
            {
#if cAlgo
                Print(ex.ToString());
#endif
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

            if (RuntimeSettings.RobustnessMode == true || RuntimeSettings.LogBacktestThreshold != null && !double.IsNaN(RuntimeSettings.LogBacktestThreshold.Value) && fitness > RuntimeSettings.LogBacktestThreshold)
            {
                try
                {
                    SaveResult(args, backtestResult, fitness, TBot.Id, GotTick ? "ticks" : "no-ticks");
                }
                catch (Exception ex)
                {
                    BacktestLogger?.LogError(ex.ToString());
                    throw;
                }
            }
        }

#if !cAlgo
        public TimeFrame TimeFrame => (this as IHasSingleSeries)?.MarketSeries?.TimeFrame;
#endif

        public static string StandardizedSymbolCode(string unsanitizedSymbol) // MOVE
        {
            var result = unsanitizedSymbol;
            result = result.Replace('-', '_');
            result = result.Replace('/', '-');
            result = result.Replace('\\', '-');
            return result;
        }
        public static string MachineName => Environment.MachineName; // FUTURE: Get custom value from config
        public string BacktestResultSaveDir(TimeFrame timeFrame) => Path.Combine(BacktestResultSaveDirBase, StandardizedSymbolCode(Symbol.Code), BotName, TimeFrame.ToShortString());
        public string RobustnessBacktestResultSaveDir(TimeFrame timeFrame) => Path.Combine(BacktestResultSaveDirBase, "Robustness", $"{StandardizedSymbolCode(Symbol.Code)} {BotName} {TimeFrame.ToShortString()}");
        public static string BacktestResultSaveDirBase = Path.Combine(LionFireEnvironment.Directories.AppProgramDataDir, "Results", MachineName);

        public static bool CreateResultsDirIfMissing = true;

        public enum BacktestSaveType
        {
            Result,
            Trades
        }

        public string GetSavePath(BacktestSaveType type, GetFitnessArgs args, BacktestResult backtestResult, double fitness, string id, TimeSpan timeSpan, string backtestFlags = null)
        {
            var sym =
#if cAlgo
            MarketSeries.SymbolCode;
#else
            TBot.Symbol;
#endif
            sym = StandardizedSymbolCode(sym);

            var tradesPerMonth = (args.TotalTrades / (timeSpan.TotalDays / 31)).ToString("F1");

            var tf =
#if cAlgo
                TimeFrame.ToShortString();
#else
            (this as IHasSingleSeries)?.MarketSeries?.TimeFrame?.Name;
#endif

            var filename = fitness.ToString("0000.0") + $"f " + (100.0 * backtestResult.Aroi / backtestResult.MaxEquityDrawdownPercentages).ToString("00.0") + $"ad {tradesPerMonth}tpm {timeSpan.TotalDays.ToString("F0")}d  {backtestResult.AverageDaysPerWinningTrade.ToString("F2")}adwt bot={GetType().Name} sym={sym} tf={tf} id={id}";
            if (backtestFlags != null)
            {
                filename += $" bt={backtestFlags}";
            }
            if (type == BacktestSaveType.Trades)
            {
                filename += ".trades";
            }
            var ext = ".json";
            var dir = RuntimeSettings.RobustnessMode == true ? RobustnessBacktestResultSaveDir(TimeFrame) : BacktestResultSaveDir(TimeFrame);
            if (dir != lastDirCreated && CreateResultsDirIfMissing && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                lastDirCreated = dir;
            }
            var path = Path.Combine(dir, filename + ext);

            for (int i = 0; File.Exists(path); i++, path = Path.Combine(dir, filename + $" ({i})" + ext))
            {
                ;
            }

            return path;
        }
        private static string lastDirCreated = null;

        private static int saveCounter = 0;

        private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
           //PropertyNamingPolicy  
        };
        private void SaveResult(GetFitnessArgs args, BacktestResult backtestResult, double fitness, string id, string backtestFlags = null)
        {
            var duration = backtestResult.End.Value - backtestResult.Start.Value;

#if LogBacktestResults
            var profit = args.Equity / initialBalance;
            backtestResult.GetAverageDaysPerTrade(args);
            try
            {
                BacktestLogger = this.GetLogger(ToString().Replace(' ', '.') + ".Backtest");
            }
            catch { } // EMPTYCATCH
            BacktestLogger?.LogInformation($"${args.Equity} ({profit.ToString("N1")}x) #{args.History.Count} {args.MaxEquityDrawdownPercentages.ToString("N2")}%dd [from ${initialBalance.ToString("N2")} to ${args.Equity.ToString("N2")}] [fit {fitness.ToString("N1")}] {Environment.NewLine} result = {resultJson} ");
#endif

            //var filename = DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss.fff ") + this.GetType().Name + " " + Symbol.Code + " " + id;

            //var filename = fitness.ToString("0000.0") + $"f " + (backtestResult.Aroi / backtestResult.MaxEquityDrawdownPercentages).ToString("00.0") + $"ad {tradesPerMonth}tpm {timeSpan.TotalDays.ToString("F0")}d  {backtestResult.AverageDaysPerWinningTrade.ToString("F2")}adwt bot={this.GetType().Name} sym={sym} tf={tf} id={id}";
            //if (backtestFlags != null)
            //{
            //    filename += $" bt={backtestFlags}";
            //}

            //var ext = ".json";
            //int i = 0;
            //var dir = BacktestResultSaveDir(TimeFrame);
            //var path = Path.Combine(dir, filename + ext);
            //if (CreateResultsDirIfMissing && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            //for (; File.Exists(path); i++, path = Path.Combine(dir, filename + $" ({i})" + ext)) ;

#if NewtonsoftJson
            var serializer = new JsonSerializer();
#endif
#if NewtonsoftJson
            using (var sw = new StreamWriter(new FileStream(GetSavePath(BacktestSaveType.Result, args, backtestResult, fitness, id, timeSpan, backtestFlags), FileMode.Create)))
            {
                serializer.Serialize(sw, backtestResult);

                //Newtonsoft.Json.JsonConvert.SerializeObject(backtestResult);
                //await sw.WriteAsync(json).ConfigureAwait(false);
            }
#else
            using (var stream = new FileStream(GetSavePath(BacktestSaveType.Result, args, backtestResult, fitness, id, duration, backtestFlags), FileMode.Create))
            {
                Utf8Json.JsonSerializer.Serialize(stream, backtestResult);
                //System.Text.Json.JsonSerializer.SerializeAsync(stream, backtestResult);
            }
#endif

            if (fitness >= RuntimeSettings.LogBacktestDetailThreshold)
            {
#if NewtonsoftJson
                using (var sw = new StreamWriter(new FileStream(GetSavePath(BacktestSaveType.Trades, args, backtestResult, fitness, id, timeSpan, backtestFlags), FileMode.Create)))
                {
                    serializer.Serialize(sw, args.History.ToArray());
                    //await sw.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(args.History.ToArray())).ConfigureAwait(false);
                }
#else
                using (var stream = new FileStream(GetSavePath(BacktestSaveType.Trades, args, backtestResult, fitness, id, duration, backtestFlags), FileMode.Create))
                {
                    Utf8Json.JsonSerializer.Serialize(stream, args.History.ToArray());
                    // System.Text.Json.JsonSerializer.SerializeAsync(stream, args.History.ToArray());
                    //System.Text.Json.JsonSerializer.SerializeAsync(stream, args.History);
                }
#endif

                if (saveCounter++ > 50)
                {
                    GC.Collect();
                    saveCounter = 0;
                }
            }
        }

        public Microsoft.Extensions.Logging.ILogger BacktestLogger { get; protected set; }

        #endregion

        #region Misc

        public virtual string Label
        {
            get
            {
                if (label != null)
                {
                    return label;
                }

                return TBot?.Id + $" {GetType().Name}";
            }
            set => label = value;
        }
        private string label = null;

        //public Microsoft.Extensions.Logging.ILogger BacktestLogger { get; protected set; }

#if cAlgo
        public Microsoft.Extensions.Logging.ILogger Logger => logger;
        protected Microsoft.Extensions.Logging.ILogger logger { get; set; }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
            {
                return null;
            }

            return points / symbol.PipSize;
        }

        public static double? TightenPips(this double? pips, double? otherPips, bool discardZeroes = true)
        {
            if (!pips.HasValue || (discardZeroes && pips.Value == 0))
            {
                return otherPips;
            }

            if (!otherPips.HasValue || (discardZeroes && otherPips.Value == 0))
            {
                return pips;
            }

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
                if (double.IsNaN(trade.ClosingPrice))
                {
                    continue;
                }

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
