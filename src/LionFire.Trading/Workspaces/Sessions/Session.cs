using LionFire.Execution;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Templating;
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

namespace LionFire.Trading.Workspaces
{



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
    public class Session : TemplateInstanceBase<TSession>, IInitializable, IStartable, IStoppable, IExecutable, INotifyPropertyChanged, IChanged
    {

        #region Properties

        #region Pass-through

        public string Name { get { return Template?.Name; } set { Template.Name = value; } }
        public string Description { get { return Template?.Description; } set { Template.Description = value; } }

        #endregion

        public Workspace Workspace { get; set; }

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

        public IEnumerable<IBot> AllBots
        {
            get
            {
                if (LiveBots != null) foreach (var bot in LiveBots)
                    {
                        yield return bot;
                    }
                if (DemoBots != null) foreach (var bot in DemoBots)
                    {
                        yield return bot;
                    }
                if (Scanners != null) foreach (var bot in Scanners)
                    {
                        yield return bot;
                    }
                if (PaperBots != null) foreach (var bot in PaperBots)
                    {
                        yield return bot;
                    }
            }
        }
        #region LiveBots

        public ObservableCollection<IBot> LiveBots
        {
            get { return liveBots; }
            set {
                liveBots = value;
            }
        }
        private ObservableCollection<IBot> liveBots;

        #endregion

        #region DemoBots

        public ObservableCollection<IBot> DemoBots
        {
            get { return demoBots; }
            set {
                demoBots = value;
            }
        }

        

        private ObservableCollection<IBot> demoBots;

        #endregion

        #region Scanners

        public ObservableCollection<IBot> Scanners
        {
            get { return scanners; }
            set {
                scanners = value;
            }
        }
        private ObservableCollection<IBot> scanners;

        #endregion

        #region PaperBots

        public ObservableCollection<IBot> PaperBots
        {
            get { return paperBots; }
            set {
                paperBots = value;
            }
        }
        private ObservableCollection<IBot> paperBots;

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
                if (state.Value == ExecutionState.Started)
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

        #region Lifecycle

        public IBehaviorObservable<ExecutionState> State { get { return state; } }
        BehaviorObservable<ExecutionState> state = new BehaviorObservable<ExecutionState>(ExecutionState.Uninitialized);

        public async Task<bool> Initialize()
        {
            this.Validate().PropertyNonDefault(nameof(Workspace), Workspace).EnsureValid();

            state.OnNext(ExecutionState.Initializing);

            LiveAccount = Workspace.GetAccount(Template.LiveAccount);
            DemoAccount = Workspace.GetAccount(Template.DemoAccount);

            await InitializeBots().ConfigureAwait(false);

            state.OnNext(ExecutionState.Ready);

            return true;
        }

        //[Idempotent] -- TODO make this idempotent by checking for existing bots
        private async Task<bool> InitializeBots()
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


            if (Mode.HasFlag(BotMode.Live)) { await LoadBots(Template.LiveBots, ref liveBots, BotMode.Live); }
            if (Mode.HasFlag(BotMode.Demo)) { await LoadBots(Template.DemoBots, ref demoBots, BotMode.Demo); }
            if (Mode.HasFlag(BotMode.Scanner)) { await LoadBots(Template.Scanners, ref scanners, BotMode.Scanner); }
            if (Mode.HasFlag(BotMode.Paper)) { await LoadBots(Template.PaperBots, ref paperBots, BotMode.Paper); }

            foreach (var bot in AllBots.OfType<IInitializable>().ToArray())
            {
                var result = await bot.Initialize();
                if (!result)
                {
                    throw new Exception($"Bot failed to initialize: {bot}");
                }
            }
            return true;
        }

        #region Load Bots

        private Task LoadBots(IEnumerable<string> botIds, ref ObservableCollection<IBot> target, BotMode mode)
        {
            if (target == null) target = new ObservableCollection<IBot>();
            target.CollectionChanged += (s, e) => Changed?.Invoke(this);

            var target2 = target;
            if (botIds == null) return Task.CompletedTask;
            return Task.Run(() => LoadBots2(botIds, target2, mode));
        }
        private void LoadBots2(IEnumerable<string> botIds, ObservableCollection<IBot> target, BotMode mode)
        {
            foreach (var botId in botIds.ToArray())
            {
                foreach (var assetName in $"*id={botId}*".Find<BacktestResult>())
                {
                    var br = assetName.Load<BacktestResult>();
                    var bot = _AddBotForMode(br, mode);
                }
            }
        }

        #endregion

        public Task Start()
        {
            symbolsAvailable = null;
            LiveAccount?.TryAdd(this);
            DemoAccount?.TryAdd(this);
            ScannerAccount?.TryAdd(this);
            PaperAccount?.TryAdd(this);
            return Task.CompletedTask;
        }

        public Task Stop(StopMode mode = StopMode.GracefulShutdown, StopOptions options = StopOptions.StopChildren)
        {
            this.state.OnNext(ExecutionState.Stopping);

            this.state.OnNext(ExecutionState.Stopped);
            return Task.CompletedTask;
        }

        #endregion

        #region Add Bots

        public void AddDemoBot(BacktestResultHandle handle)
        {
            _AddBot(handle, BotMode.Demo | BotMode.Scanner | BotMode.Paper, Workspace.DefaultDemoAccount);
        }

        public void AddLiveBot(BacktestResultHandle handle)
        {
            _AddBot(handle, BotMode.Live | BotMode.Scanner | BotMode.Paper, Workspace.DefaultLiveAccount);
        }
        public void AddScanner(BacktestResultHandle handle)
        {
            _AddBot(handle, BotMode.Scanner | BotMode.Paper, Workspace.DefaultScannerAccount);
        }

        public IEnumerable<IBot> AddBotForModes(BacktestResult backtestResult, BotMode modes)
        {
            if (modes.HasFlag(BotMode.Live))
            {
                yield return _AddBotForMode(backtestResult, BotMode.Live);
            }
            if (modes.HasFlag(BotMode.Demo))
            {
                yield return _AddBotForMode(backtestResult, BotMode.Demo);
            }
            if (modes.HasFlag(BotMode.Scanner))
            {
                yield return _AddBotForMode(backtestResult, BotMode.Scanner);
            }
            if (modes.HasFlag(BotMode.Paper))
            {
                yield return _AddBotForMode(backtestResult, BotMode.Paper);
            }
        }

        private IBot _AddBotForMode(BacktestResult backtestResult, BotMode mode)
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
            return _AddBot(null, mode, account, backtestResult);
        }

        private IBot _AddBot(BacktestResult backtestResult, BotMode mode)
        {
            return _AddBot(null, mode, null, backtestResult); // REFACTOR
        }
        private IBot _AddBot(BacktestResultHandle handle, BotMode mode, IAccount account = null, BacktestResult backtestResult = null)
        {
            //var sessionVM = this;

            if (account == null) throw new ArgumentNullException(nameof(account));

            if (backtestResult == null) { backtestResult = handle.Object; }

            var templateType = ResolveType(backtestResult.BotConfigType);
            //var templateType = Type.GetType(result.BotConfigType);
            if (templateType == null)
            {
                throw new NotSupportedException($"Bot type not supported: {backtestResult.BotConfigType}");
            }

            var tBot = (TBot)((JObject)backtestResult.Config).ToObject(templateType);

            var bot = (IBot)tBot.Create();
            bot.Mode = BotMode.Scanner | BotMode.Paper;
            //var botVM = new BotVM() { Bot = bot };
            //botVM.State = SupervisorBotState.Scanner;

            if (bot.Mode.HasFlag(BotMode.Live))
            {
                throw new Exception("Not ready for live yet");
            }
            bot.Account = account;
            //botVM.AddBacktestResult(backtestResult);

            bool hasPaper = mode.HasFlag(BotMode.Paper);
            mode = mode & ~BotMode.Paper;
            if (mode.HasFlag(BotMode.Live)) { LiveBots.Add(bot); Template.LiveBots.Add(tBot.Id); }
            if (mode.HasFlag(BotMode.Demo)) { DemoBots.Add(bot); Template.DemoBots.Add(tBot.Id); }
            if (mode.HasFlag(BotMode.Scanner)) { Scanners.Add(bot); Template.Scanners.Add(tBot.Id); }
            if (mode.HasFlag(BotMode.Paper)) { PaperBots.Add(bot); Template.PaperBots.Add(tBot.Id); }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Changed?.Invoke(this);
        }

        #endregion


        public Type ResolveType(string typeName)
        {

            var type = Type.GetType(typeName);
            if (type != null) return type;
            typeName = typeName.Replace("LionFire.Trading.cTrader", "LionFire.Trading.Proprietary");
            type = Type.GetType(typeName);
            return type;
        }


        public override string ToString()
        {
            var liveBots = LiveBots.Count == 0 ? "" : $" Live bots: {LiveBots.Count}";
            var demoBots = LiveBots.Count == 0 ? "" : $" Demo bots: {DemoBots.Count}";
            var scanners = LiveBots.Count == 0 ? "" : $" Scanners: {Scanners.Count}";
            var paperBots = LiveBots.Count == 0 ? "" : $" Paper bots: {PaperBots.Count}";

            return $"{{Session \"{Name}\" ({Mode}) {liveBots}{demoBots}{scanners}{paperBots}}}";
        }

        #endregion
    }
    
}
