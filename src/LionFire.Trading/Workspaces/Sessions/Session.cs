using LionFire.Execution;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LionFire.Validation;
using System.Threading.Tasks;
using LionFire.Trading.Bots;
using Newtonsoft.Json;
using System.IO;
using LionFire.Trading.Backtesting;
using LionFire.Assets;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using LionFire.Structures;
using System.Diagnostics;
using LionFire.Persistence;
using LionFire.Types;
using LionFire.States;
using LionFire.Threading.Tasks;
using LionFire.Execution.Executables;
using LionFire.DependencyInjection;

namespace LionFire.Trading.Workspaces
{

    public class ScannerSettings : INotifyPropertyChanged
    {

        #region SignalThreshold

        public double SignalThreshold
        {
            get { return signalThreshold; }
            set
            {
                if (signalThreshold == value) return;
                signalThreshold = value;
                OnPropertyChanged(nameof(SignalThreshold));
            }
        }
        private double signalThreshold = 0.5;

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    /// <summary>
    /// Documents for 
    /// </summary>
    public interface ISessionItemViewModel
    {
    }

    public enum LayoutItemDockMode
    {
        Unspecified,
        Document,
        Left,
        Right,
        Bottom,
        Top,
        Tabbed,
    }

    public class LayoutItem
    {
        public LayoutItemDockMode DockMode { get; set; }

        public string Name { get; set; }
        public string ParentName { get; set; }

        public object ViewModel { get; set; }
    }
    public class Layout
    {
        public List<LayoutItem> Items { get; set; }
    }


    public class CategoryViewModel : SignalViewModelBase
    {
    }

    public class SymbolViewModel : SignalViewModelBase
    {
        public Symbol Symbol { get; set; }

        public SymbolViewModel()
        {
        }
    }


    /// <summary>
    /// Trading session:
    ///  - One live account
    ///  - One demo account
    ///  - Mode switch toggles:
    ///     - live
    ///     - demo
    ///     - scan
    ///     - paper
    /// </summary>
    public class Session : ExecutableExBase, IInitializable, IStartable, IStoppableEx, IExecutableEx, INotifyPropertyChanged, IChanged, INotifyOnSaving
        , ITemplateInstance<TSession>
    {

        #region Identity

        [SetOnce]
        public TSession Template { get; set; }

        #endregion

        #region Construction

        public Session()
        {

            Bots.CollectionChanged += (s, e) => RaiseChanged();
        }
        
        #endregion

        #region Properties

        #region Pass-through

        public string Name { get { return Template?.Name; } set { Template.Name = value; } }
        public string Description { get { return Template?.Description; } set { Template.Description = value; } }

        #endregion

        public TradingWorkspace Workspace { get; set; }

        #region Account

        public IAccount LiveAccount { get; set; }
        public IAccount DemoAccount { get; set; }
        public IAccount PaperAccount { get { return GetAccountForMode(PaperAccountMode); } } // REVIEW - how to do a Paper account alongside a real account?
        public IAccount ScannerAccount { get { return GetAccountForMode(ScannerAccountMode); } }

        public AccountMode ScannerAccountMode { get { return Template.ScannerAccountMode; } set { Template.ScannerAccountMode = value; } }
        public AccountMode PaperAccountMode { get { return Template.PaperAccountMode; } set { Template.PaperAccountMode = value; } }

        public IAccount Account
        {
            get
            {
                switch (Mode & ~BotMode.Paper)
                {
                    case BotMode.Live:
                        return LiveAccount;
                    case BotMode.Demo:
                        return DemoAccount;
                    case BotMode.Scanner:
                        return GetAccountForMode(ScannerAccountMode);
                    default:
                        break;
                }
                if (Mode == BotMode.Paper)
                {
                    return PaperAccount;
                }
                return null;
            }
        }

        public IAccount GetAccountForMode(AccountMode mode)
        {
            switch (mode)
            {
                case AccountMode.Demo:
                    return DemoAccount;
                case AccountMode.Live:
                    return LiveAccount;
                case AccountMode.Any:
                    return DemoAccount ?? LiveAccount;
                default:
                    throw new ArgumentException("AccountMode not set");
            }
        }

        #endregion


        public IEnumerable<string> SymbolsAvailable
        {
            get
            {
                if (symbolsAvailable == null)
                {
                    var result = Account.SymbolsAvailable;

                    if (Template.EnabledSymbols != null)
                    {
                        result = result.Intersect(Template.EnabledSymbols);
                    }
                    else if (Template.EnabledSymbols != null)
                    {
                        result = result.Where(r => !Template.DisabledSymbols.Contains(r));
                    }
                    symbolsAvailable = result;
                    OnPropertyChanged(nameof(SymbolsAvailable));
                }
                return symbolsAvailable;
            }
        }
        IEnumerable<string> symbolsAvailable;

        public IEnumerable<string> TimeFramesAvailable
        {
            get
            {
                yield return "t1";
                yield return "m1";
                yield return "h1";
                // TODO: More
            }
        }

        #region Bots


        #region ScannerSettings

        [State]
        public ScannerSettings ScannerSettings
        {
            get { return scannerSettings; }
            set
            {
                if (scannerSettings == value) return;
                scannerSettings = value;
                OnPropertyChanged(nameof(ScannerSettings));
            }
        }
        private ScannerSettings scannerSettings = new ScannerSettings();

        #endregion

        #region Bots

        public ObservableCollection<IBot> Bots
        {
            get { return bots; }
            set
            {
                bots = value;
            }
        }
        private ObservableCollection<IBot> bots = new ObservableCollection<IBot>();

        private HashSet<string> BotIds = new HashSet<string>();

        #endregion

        #endregion

        #endregion

        #region Configuration

        public BotMode Mode
        {
            get
            {
                return Template.Mode;
            }
            set
            {
                bool wasStarted = false;
                if (State == ExecutionStateEx.Started)
                {
                    wasStarted = true;
                }
                Template.Mode = value;
                symbolsAvailable = null;
                if (wasStarted)
                {
                    Start().Wait();
                }
            }
        }

        #endregion

        public List<string> EnabledSymbols { get; set; }
        public List<string> DisabledSymbols { get; set; }
        public object Lock { get; private set; } = new object();

        TradingOptions TradingOptions => DependencyContext.Current.GetService<TradingOptions>();
        #region Lifecycle

        public async Task<bool> Initialize()
        {
            this.Validate().PropertyNonDefault(nameof(Workspace), Workspace).EnsureValid();

            State = ExecutionStateEx.Initializing;

            LiveAccount = Workspace.GetAccount(Template.LiveAccount);
            DemoAccount = Workspace.GetAccount(Template.DemoAccount);

            //if (TradingOptions.Features.HasFlag(TradingFeatures.Bots))
            {
                if (!await InitBots().ConfigureAwait(continueOnCapturedContext: false))
                {
                    State = ExecutionStateEx.Faulted;
                    return false;
                }
            }

            State = ExecutionStateEx.Ready;

            return true;
        }

        //[Idempotent] -- TODO make this idempotent by checking for existing bots
        private async Task<bool> InitBots()
        {
            //if (account == null) { return false; }

            //account.StatusTextChanged += OnAccountStatusTextChanged;
            //StatusText = account.StatusText;

            //var symbols = new ObservableCollection<SymbolVM>();

            //foreach (var s in account.SymbolsAvailable)
            //{
            //    var symbol = account.GetSymbol(s);
            //    var vm = new SymbolVM(symbol, account);
            //    symbols.Add(vm);
            //}
            //VM.Symbols = symbols;


            //if (Mode.HasFlag(BotMode.Live)) { await LoadBots(Template.LiveBots, ref liveBots, BotMode.Live); }
            //if (Mode.HasFlag(BotMode.Demo)) { await LoadBots(Template.DemoBots, ref demoBots, BotMode.Demo); }
            if (Mode.HasFlag(BotMode.Scanner)) { await LoadBots(Template.Bots, ref bots, BotMode.Scanner).ConfigureAwait(false); }
            //if (Mode.HasFlag(BotMode.Paper)) { await LoadBots(Template.PaperBots, ref paperBots, BotMode.Paper); }

            foreach (var bot in Bots.OfType<IInitializable>().ToArray())
            {
                try
                {
                    var result = await bot.Initialize().ConfigureAwait(false);
                    if (!result)
                    {
                        Debug.WriteLine($"Bot failed to initialize: {bot}{Environment.NewLine}{(bot as AccountParticipant)?.FaultException?.ToString()}");
                    }
                }
                catch
                {
                    // EMPTYCATCH - intended
                }
                //if (!result)
                //{
                //    throw new Exception($"Bot failed to initialize: {bot}");
                //}
            }
            return true;
        }

        #region Load Bots

        private Task LoadBots(IEnumerable<string> botIds, ref ObservableCollection<IBot> target, BotMode mode)
        {
            return LoadBots(botIds.Select(id => new PBot { Id = id }), ref target, mode);
        }
        private Task LoadBots(IEnumerable<IInstantiator> bots, ref ObservableCollection<IBot> target, BotMode mode)
        {
            var target2 = target;
            if (bots == null || !bots.Any()) return Task.CompletedTask;

            return Task.Run(() => LoadBots2(bots, target2, mode));
        }
        private void LoadBots2(IEnumerable<IInstantiator> bots, ObservableCollection<IBot> target, BotMode mode)
        {
            foreach (var pBot in bots.ToArray())
            {
                if (pBot == null)
                {
                    Debug.WriteLine("WARN - LoadBots2: null instantiator in bots parameter");
                    continue;
                }
                var bot = (IBot)pBot.Instantiate();
                // TODO: Use AddBotForModes NEXT
                _AddBotForMode(mode, bot: bot, pBot: pBot as PBot);
            }
        }

        #endregion

        public Task Start()
        {
            symbolsAvailable = null;
            //LiveAccount?.TryAdd(this);
            //DemoAccount?.TryAdd(this);
            //ScannerAccount?.TryAdd(this);
            //PaperAccount?.TryAdd(this);
            return Task.CompletedTask;
        }

        public Task Stop(StopMode mode = StopMode.GracefulShutdown, StopOptions options = StopOptions.StopChildren)
        {
            State = ExecutionStateEx.Stopping;

            State = ExecutionStateEx.Stopped;
            return Task.CompletedTask;
        }

        #endregion

        #region Add Bots

        public void AddDemoBot(BacktestResultHandle handle)
        {
            _AddBot(BotMode.Demo | BotMode.Scanner | BotMode.Paper, account: Workspace.DefaultDemoAccount, backtestResultHandle: handle);
        }

        public void AddLiveBot(BacktestResultHandle handle)
        {
            _AddBot(BotMode.Live | BotMode.Scanner | BotMode.Paper, account: Workspace.DefaultLiveAccount, backtestResultHandle: handle);
        }
        public void AddScanner(BacktestResultHandle handle)
        {
            _AddBot(BotMode.Scanner | BotMode.Paper, account: Workspace.DefaultScannerAccount, backtestResultHandle: handle);
        }

        public IEnumerable<IBot> AddBotForModes(BacktestResult backtestResult, BotMode modes)
        {
            if (modes.HasFlag(BotMode.Live))
            {
                yield return _AddBotForMode(BotMode.Live, backtestResult: backtestResult);
            }
            if (modes.HasFlag(BotMode.Demo))
            {
                yield return _AddBotForMode(BotMode.Demo, backtestResult: backtestResult);
            }
            if (modes.HasFlag(BotMode.Scanner))
            {
                yield return _AddBotForMode(BotMode.Scanner, backtestResult: backtestResult);
            }
            if (modes.HasFlag(BotMode.Paper))
            {
                yield return _AddBotForMode(BotMode.Paper, backtestResult: backtestResult);
            }
        }

        private IBot _AddBotForMode(BotMode mode, BacktestResult backtestResult = null, PBot pBot = null, TBot tBot = null, IBot bot = null)
        {
            IAccount account;
            switch (mode)
            {
                case BotMode.Live:
                    account = LiveAccount;
                    break;
                case BotMode.Demo:
                    account = DemoAccount;
                    break;
                case BotMode.Scanner:
                    account = ScannerAccount;
                    break;
                case BotMode.Paper:
                    account = PaperAccount;
                    break;
                default:
                    throw new ArgumentException(nameof(mode) + " unknown or is set to more than one mode.");
            }
            if (account == null)
            {
                Debug.WriteLine($"WARNING - _AddBotForMode({mode}) assigned no account");
            }
            return _AddBot(mode, account: account, backtestResult: backtestResult, pBot: pBot, tBot: tBot, bot: bot);
        }

        private IBot _AddBot(BacktestResult backtestResult, BotMode mode)
        {
            return _AddBot(mode, backtestResult: backtestResult); // REFACTOR
        }

        private IBot _AddBot(BotMode mode, BacktestResultHandle backtestResultHandle = null, IAccount account = null, BacktestResult backtestResult = null, PBot pBot = null, TBot tBot = null, IBot bot = null)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            if (backtestResult == null) backtestResult = backtestResultHandle?.Object;

            if (bot == null)
            {
                if (pBot != null)
                {
                    bot = (IBot)pBot.Instantiate();
                }
                else
                {
                    if (tBot == null)
                    {
                        tBot = backtestResult.TBot;
                        //if (backtestResult == null) { backtestResult = backtestResultHandle?.Object; }

                        //var templateType = ResolveType(backtestResult.BotConfigType);
                        ////var templateType = Type.GetType(result.BotConfigType);
                        //if (templateType == null)
                        //{
                        //    throw new NotSupportedException($"Bot type not supported: {backtestResult.BotConfigType}");
                        //}

                        //tBot = (TBot)((JObject)backtestResult.Config).ToObject(templateType);
                    }

                    if (tBot == null)
                    {
                        throw new ArgumentNullException(nameof(tBot));
                    }

                    bot = (IBot)tBot.Create();
                }
            }

            //bot.Modes = mode | BotMode.Paper;
            //var botVM = new BotVM() { Bot = bot };
            //botVM.State = SupervisorBotState.Scanner;

            if (bot.Modes.HasFlag(BotMode.Live))
            {
                throw new Exception("Not ready for live yet");
            }
            bot.Account = account;
            //botVM.AddBacktestResult(backtestResult);

            //bool hasPaper = mode.HasFlag(BotMode.Paper);
            //if (mode != BotMode.Paper)
            //{
            //    mode = mode & ~BotMode.Paper;
            //}

            //if (mode.HasFlag(BotMode.Live)) { LiveBots.Add(bot); Template.LiveBots.Add(tBot.Id); }
            //if (mode.HasFlag(BotMode.Demo)) { DemoBots.Add(bot); Template.DemoBots.Add(tBot.Id); }
            //if (mode.HasFlag(BotMode.Scanner)) { Scanners.Add(bot); Template.Scanners.Add(tBot.Id); }
            //if (mode.HasFlag(BotMode.Paper)) { PaperBots.Add(bot); Template.PaperBots.Add(tBot.Id); }

            //if (mode.HasFlag(BotMode.Live)) { if (LiveBots == null) { LiveBots = new ObservableCollection<IBot>(); } LiveBots.Add(bot); }
            //if (mode.HasFlag(BotMode.Demo)) { if (DemoBots == null) { DemoBots = new ObservableCollection<IBot>(); } DemoBots.Add(bot); }

            if (Bots == null)
            {
                Bots = new ObservableCollection<IBot>();
            }

            if (!BotIds.Contains(bot.Template.Id))
            {
                BotIds.Add(bot.Template.Id);
                Bots.Add(bot);
            }

            //if (mode.HasFlag(BotMode.Paper)) { if (PaperBots==null) { PaperBots = new ObservableCollection<IBot>(); } PaperBots.Add(bot); }

            //if (pBot != null)
            //{
            //    bot.DesiredExecutionState = pBot.DesiredExecutionState;
            //}

            //switch (mode)
            //{
            //    case BotMode.Live:
            //        LiveBots.Add(bot);
            //        break;
            //    case BotMode.Demo:
            //        DemoBots.Add(bot);
            //        break;
            //    case BotMode.Scanner:
            //        Scanners.Add(bot);
            //        break;
            //    case BotMode.Paper:

            //        break;
            //    default:
            //        throw new ArgumentException(nameof(mode));
            //}

            //if (mode.HasFlag(BotMode.Scanner))
            //{
            //    Scanners.Add(bot);
            //}
            //if (mode.HasFlag(BotMode.Live))
            //{
            //    LiveBots.Add(bot);
            //}
            //if (mode.HasFlag(BotMode.Demo))
            //{
            //    DemoBots.Add(bot);
            //}
            //if (mode.HasFlag(BotMode.Paper))
            //{
            //    PaperBots.Add(bot);
            //}

            //Task.Factory.StartNew(() => bot.Start());
            return bot;
        }

        #endregion

        #region Misc


        public event Action<object> Changed;

        #region INotifyPropertyChanged Implementation

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            RaiseChanged();

        }
        private void RaiseChanged()
        {
            Changed?.Invoke(this);
            Workspace.RaiseChanged();
        }

        #endregion
        

        public override string ToString()
        {
            // TODO
            string liveBots = "", demoBots = "", paperBots = "";
            //var liveBots = LiveBots.Count == 0 ? "" : $" Live bots: {LiveBots.Count}";
            //var demoBots = LiveBots.Count == 0 ? "" : $" Demo bots: {DemoBots.Count}";
            var scanners = Bots.Count == 0 ? "" : $" Bots: {Bots.Count}";
            //var paperBots = LiveBots.Count == 0 ? "" : $" Paper bots: {PaperBots.Count}";

            return $"{{Session \"{Name}\" ({Mode}) {liveBots}{demoBots}{scanners}{paperBots}}}";
        }

        public void OnSaving(object context = null)
        {
            Template.Bots.Clear();
            foreach (var bot in Bots)
            {
                var inst = bot.ToInstantiator();
                if (inst == null)
                {
                    // TOALERTER
                    Debug.WriteLine("WARN- Session.OnSaving bot.ToInstantiator() returned null;");
                    continue;
                }
                Template.Bots.Add(inst);
            }

        }

        #endregion
    }

}
