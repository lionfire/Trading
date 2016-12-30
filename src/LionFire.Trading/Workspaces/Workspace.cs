using LionFire.Assets;
using LionFire.Execution;
using LionFire.Templating;
using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using System.ComponentModel;
using System.IO;
using LionFire.States;
using LionFire.Structures;
using System.Reflection;

namespace LionFire.Trading.Workspaces
{
    
    /// <summary>
    /// Each user will typically work with one workspace.  
    /// FUTURE: Hierarchy of groups and sessions, allowing users to start/stop/view entire groups
    /// </summary>
    public class Workspace : ITemplateInstance<TWorkspace>, IExecutable, IStartable, IInitializable, INotifyPropertyChanged, IChanged
    {

        #region Relationships

        #region Template

        public TWorkspace Template
        {
            get { return template; }
            set
            {
                if (template == value) return;
                if (template != null)
                {
                    template.ControlSwitchChanged -= ControlSwitchChanged;
                }
                template = value;

                if (template != null)
                {
                    template.ControlSwitchChanged += ControlSwitchChanged;
                }
            }
        }
        private TWorkspace template;

        #endregion

        ITemplate ITemplateInstance.Template { get { return Template; } set { Template = (TWorkspace)value; } }


        #endregion

        public WorkspaceInfo Info { get; set; }

        #region Settings
               

        #region Handlers

        private void ControlSwitchChanged()
        {
            foreach (var a in Accounts)
            {
                a.IsTradeApiEnabled = CanConnect;
            }
        }

        public bool CanConnect
        {
            get { return Template.AllowAny && Template.AllowConnect; }
        }

        #endregion

        #endregion

        #region State


        #region StatusText

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }
        private string statusText;

        #endregion


        //public WorkspaceNode Root { get; set; } = new WorkspaceNode(); // FUTURE

        public ObservableCollection<Session> Sessions { get; private set; } = new ObservableCollection<Session>();

        public IBehaviorObservable<ExecutionState> State { get { return state; } }
        private BehaviorObservable<ExecutionState> state = new BehaviorObservable<Execution.ExecutionState>(ExecutionState.Uninitialized);

        public ObservableCollection<IAccount> LiveAccounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> DemoAccounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> Accounts { get; private set; } = new ObservableCollection<IAccount>();


        public ObservableCollection<WorkspaceBot> Bots { get; private set; } = new ObservableCollection<WorkspaceBot>();
        public ObservableCollection<WorkspaceBot> Scanners { get; private set; } = new ObservableCollection<WorkspaceBot>();
        public ObservableCollection<PriceAlert> Alerts { get; private set; } = new ObservableCollection<PriceAlert>();


        #region Derived

        public IAccount DefaultLiveAccount
        {
            get { return Accounts.Where(a => !a.IsDemo).FirstOrDefault(); }
        }
        public IAccount DefaultDemoAccount
        {
            get { return Accounts.Where(a => a.IsDemo).FirstOrDefault(); }
        }
        public IAccount DefaultScannerAccount
        {
            get { return DefaultDemoAccount ?? DefaultLiveAccount; }
        }

        #endregion

        private void ResetState()
        {
            LiveAccounts.Clear();
            DemoAccounts.Clear();
            Accounts.Clear();
            accountsByName.Clear();
            Bots.Clear();
            Alerts.Clear();
            state.OnNext(ExecutionState.Uninitialized);
        }

        #endregion

        #region Accounts

        private Dictionary<string, IAccount> accountsByName = new Dictionary<string, IAccount>();

        private void AddAccount(string accountId, IAccount account)
        {
            Accounts.Add(account);
            accountsByName.Add(accountId, account);
        }
        public IAccount GetAccount(string accountName)
        {
            if (accountName == null) return null;
            IAccount account;
            accountsByName.TryGetValue(accountName, out account);
            return account;
        }

        #endregion


        #region Lifecycle

        public Workspace()
        {
            //this.AttachChangedEventToCollections(() => Changed?.Invoke(this));
        }

        public async Task<bool> Initialize()
        {
            state.OnNext(ExecutionState.Initializing);

            ResetState();
            // TODO: Verify state

            state.OnNext(Execution.ExecutionState.Starting);

            if ((Template.TradingOptions.AccountModes & AccountMode.Live) == AccountMode.Live)
            {
                foreach (var accountId in Template.LiveAccounts)
                {
                    var account = TypeResolver.CreateAccount(accountId);
                    LiveAccounts.Add(account);
                    AddAccount(accountId, account);
                }
            }

            if ((Template.TradingOptions.AccountModes & AccountMode.Demo) == AccountMode.Demo)
            {
                foreach (var accountId in Template.DemoAccounts)
                {
                    var account = TypeResolver.CreateAccount(accountId);
                    DemoAccounts.Add(account);
                    AddAccount(accountId, account);
                }
            }

            foreach (var tSession in Template.Sessions)
            {
                var session = tSession.Create();
                session.Workspace = this;
                this.Sessions.Add(session);
                await session.Initialize().ConfigureAwait(continueOnCapturedContext: false);
            }

            state.OnNext(ExecutionState.Ready);
            return true;
        }

        public ITypeResolver TypeResolver
        {
            get
            {
                return LionFire.Structures.ManualSingleton<ITypeResolver>.Instance;
            }
        }

        public async Task Start()
        {
            await StartAllAccounts();
            await StartAllSessions();
            ControlSwitchChanged();
            state.OnNext(ExecutionState.Started);
        }

        public async Task StartAllSessions(bool forceStart = false)
        {
            foreach (var session in Sessions)
            {
                var startable = session as IStartable;
                if (startable == null) continue;
                if (forceStart || session.Template.DesiredExecutionState == ExecutionState.Started)
                {
                    await session.Start();
                }
            }
        }

        public async Task StartAllAccounts(bool forceStart = false)
        {
            foreach (var account in Accounts)
            {
                var startable = account as IStartable;
                if (startable == null) continue;
                if (forceStart || account.Template.DesiredExecutionState == ExecutionState.Started)
                {
                    await startable.Start();
                }
            }
        }

        public async Task StartAllBots()
        {
            foreach (var bot in Bots)
            {
                await bot.Bot.Start();
            }
        }

        #endregion

        public List<Type> BotTypes { get; set; } = new List<Type>();

        #region Misc

        public event Action<object> Changed;

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Changed?.Invoke(this);
        }

        #endregion

        #endregion
    }


}
