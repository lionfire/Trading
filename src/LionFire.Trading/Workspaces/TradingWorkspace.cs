using LionFire.Assets;
using LionFire.Execution;
using LionFire.Instantiating;
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
using LionFire.Persistence;
using LionFire.ExtensionMethods;
using LionFire.Instantiating.Templating;
using System.Diagnostics;
using LionFire.DependencyInjection;
using LionFire.UI.Workspaces;
using LionFire.Validation;

namespace LionFire.Trading.Workspaces
{


    /// <summary>
    /// Each user will typically work with one workspace.  
    /// FUTURE: Hierarchy of groups and sessions, allowing users to start/stop/view entire groups
    /// </summary>
    [AssetPath("Workspaces")]
    [HasDependencies]
    [State]
    public class TradingWorkspace : Workspace<TTradingWorkspace, WorkspaceItem>, IExecutableEx, IStartable, IInitializable2, INotifyPropertyChanged, IChanged, INotifyOnSaving, IAsset, INotifyOnInstantiated, IKeyed<string> 
    {
        public Type Type => typeof(TradingWorkspace);

        // REVIEW - why is this needed -- does it make sense?
        public string Key
        {
            get { return Template?.Name; }
        }

        // FUTURE: Inject this after loading asset if it is not set
        // TEMP - TODO: Move subpath to be stored here, and inject it on load
        public string AssetSubPath { get { return Template?.Name; } set { Template.Name = value; } }

        //public IEnumerable<IFeed> NotificationFeeds { get; } // Feed(s) used for Price notfications??

        public IEnumerable<IFeed> GetAccounts(AccountMode accountModes)
        {
            if (accountModes.HasFlag(AccountMode.Live) || accountModes.HasFlag(AccountMode.Any))
            {
                foreach (var a in LiveAccounts) yield return a;
            }
            if (accountModes.HasFlag(AccountMode.Demo) || accountModes.HasFlag(AccountMode.Any))
            {
                foreach (var a in DemoAccounts) yield return a;
            }
        }

        #region Relationships

        #region Template
      
        protected override void OnTemplateChanged(TTradingWorkspace oldValue, TTradingWorkspace template)
        {
            if (oldValue != null)
            {
                throw new NotImplementedException("Obsolete");
                //oldValue.ControlSwitchChanged -= ControlSwitchChanged;
                //oldValue.IsAutoSaveEnabledChanged -= Template_IsAutoSaveEnabledChanged;
                //oldValue.PropertyChanged -= Template_PropertyChanged;
            }
            if (template != null)
            {
                template.ControlSwitchChanged += ControlSwitchChanged;
                template.IsAutoSaveEnabledChanged += Template_IsAutoSaveEnabledChanged;
                this.EnableAutoSave(Template.IsAutoSaveEnabled);
                ControlSwitchChanged();
                template.PropertyChanged += Template_PropertyChanged;
            }
        }

        private void Template_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Template.SelectedWorkspaceItemId))
            {
                OnPropertyChanged(nameof(SelectedWorkspaceItem));
            }
        }

        private void Template_IsAutoSaveEnabledChanged()
        {
            this.EnableAutoSave(Template.IsAutoSaveEnabled);
        }

        #endregion

        public WorkspaceItem SelectedWorkspaceItem
        {
            get
            {
                return ItemsById.TryGetValue(Template.SelectedWorkspaceItemId);
            }
            set
            {
                Template.SelectedWorkspaceItemId = value?.Template?.Id;
            }
        }

        #region Items

        public Dictionary<string, WorkspaceItem> ItemsById
        {
            get; private set;
        } = new Dictionary<string, WorkspaceItem>();

        public IEnumerable<WorkspaceItem> Items => base.Children.OfType<WorkspaceItem>();

        internal void Add(WorkspaceItem item)
        {
            if (ItemsById.ContainsKey(item.Template.Id)) return;
            item.Workspace = this;
            ItemsById.AddOrUpdate(item.Template.Id, item);
            if (!Items.Contains(item)) Children.Add(item);
        }
        internal void Remove(WorkspaceItem item)
        {
            if (!ItemsById.ContainsKey(item.Template.Id)) return;
            ItemsById.Remove(item.Template.Id);
            Children.Remove(item);
            item.Workspace = null;
        }

        #endregion

        #endregion

        public void LoadWorkspaceItems()
        {
            foreach (var tItem in Template.Items)
            {
                var workspaceItem = tItem.Create();
                if (tItem.Id == null) { tItem.Id = Guid.NewGuid().ToString(); }
                Add(workspaceItem);
            }
        }

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


        public ObservableCollection<IAccount> LiveAccounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> DemoAccounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> Accounts { get; private set; } = new ObservableCollection<IAccount>();
        public ObservableCollection<IFeed> Feeds { get; private set; } = new ObservableCollection<IFeed>();


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
            Sessions.Clear();
            State = ExecutionStateEx.Uninitialized;
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

        public TradingWorkspace()
        {
            this.AttachChangedEventToCollections(() => Changed?.Invoke(this));

            Accounts.CollectionChanged += Accounts_CollectionChanged;

        }

        private void Accounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                ControlSwitchChanged();
            }
        }

        protected override  void OnInitializing(ref ValidationContext validationContext)
        {
            base.OnInitializing(ref validationContext); // Injects dependencies
  
            if (Sessions.Count > 0) throw new Exception("Sessions already populated");

            ResetState();
            // TODO: Verify state

            State = Execution.ExecutionStateEx.Starting;

            if ((Template.TradingOptions.AccountModes & AccountMode.Live) == AccountMode.Live)
            {
                foreach (var accountId in Template.LiveAccounts)
                {
                    var feed = TradingTypeResolver.CreateAccount(accountId);
                    if (feed is IAccount account)
                    {
                        LiveAccounts.Add(account);
                        AddAccount(accountId, account);
                    }
                    Feeds.Add(feed);
                }
            }

            if ((Template.TradingOptions.AccountModes & AccountMode.Demo) == AccountMode.Demo)
            {
                foreach (var accountId in Template.DemoAccounts)
                {
                    var feed = TradingTypeResolver.CreateAccount(accountId);
                    if (feed is IAccount account)
                    {
                        DemoAccounts.Add(account);
                        AddAccount(accountId, account);
                    }
                    Feeds.Add(feed);
                }
            }

            if ((TradingOptions.Features &
                 (
                   TradingFeatures.Bots | TradingFeatures.Scanners
                 )) != TradingFeatures.None)
            {
                foreach (var tSession in Template.Sessions)
                {
                    var session = tSession.Create();
                    session.Workspace = this;
                    //session.Initialize().ConfigureAwait(continueOnCapturedContext: false); // Loads child collections
                    session.Initialize().Wait(); // Loads child collections;
                    this.Sessions.Add(session);
                }
            }

            if (TradingOptions.Features.HasFlag(TradingFeatures.WorkspaceInterface))
            {
                LoadWorkspaceItems();
            }
            State = ExecutionStateEx.Ready;
            //return true;
        }

        public TradingOptions TradingOptions => InjectionContext.Current.GetService<TradingOptions>();

        [Dependency]
        public ITradingTypeResolver TradingTypeResolver { get; set; }
        //public ITradingTypeResolver TradingTypeResolver => InjectionContext.Current.GetService<ITradingTypeResolver>();


        public async Task Start()
        {
            await StartAllAccounts().ConfigureAwait(false);
            await StartAllSessions().ConfigureAwait(false);
            ControlSwitchChanged();

            foreach (var workspaceItem in Items.OfType<IStartable>())
            {
                var ce = workspaceItem as IControllableExecutable;
                if (ce == null || ce.DesiredExecutionState == ExecutionStateEx.Started)
                {
                    await workspaceItem.Start().ConfigureAwait(false);
                }
            }

            State = ExecutionStateEx.Started;
        }

        public async Task StartAllSessions(bool forceStart = false)
        {
            foreach (var session in Sessions)
            {
                var startable = session as IStartable;
                if (startable == null) continue;
                if (forceStart || session.Template.DesiredExecutionState == ExecutionStateEx.Started)
                {
                    await session.Start().ConfigureAwait(false);
                }
            }
        }

        public async Task StartAllAccounts(bool forceStart = false)
        {
            foreach (var account in Accounts)
            {
                var startable = account as IStartable;
                if (startable == null) continue;
                if (forceStart || account.Template.DesiredExecutionState == ExecutionStateEx.Started)
                {
                    await startable.Start().ConfigureAwait(false);
                }
            }
        }

        public async Task StartAllBots()
        {
            foreach (var bot in Bots)
            {
                await bot.Bot.Start().ConfigureAwait(false);
            }
        }

        #endregion


        #region View State

        [SerializeIgnore]
        public List<object> MainWindow { get; private set; } = new List<object>();
        public List<object> Windows { get; private set; } = new List<object>();

        #endregion


        #region DownloadStatusText

        public string DownloadStatusText
        {
            get { return downloadStatusText; }
            set
            {
                if (downloadStatusText == value) return;
                downloadStatusText = value;
                OnPropertyChanged(nameof(DownloadStatusText));
            }
        }
        private string downloadStatusText;

        #endregion

        public List<Type> BotTypes { get; set; } = new List<Type>();

        

        #region Misc

        public event Action<object> Changed;

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            Changed?.Invoke(this);
        }

        #endregion

        //public Task Save(object context = null)
        //{
        //    if (Template.Name == null)
        //    {
        //        throw new Exception("Can't save when name is null");
        //    }

        //    OnSaving(context);
        //    Template.Save();

        //    return Task.CompletedTask;
        //}

        public void OnSaving(object context = null)
        {
            //foreach (var item in Template.Items)
            //{
            //}

            if (Sessions != null)
            {
                foreach (var s in Sessions)
                {
                    s.OnSaving(context);
                }
            }
        }

        internal void RaiseChanged()
        {
            this.Changed?.Invoke(this);
        }

        public void OnInstantiated(object instantiationContext = null)
        {
            Debug.WriteLine($"Workspace '{this.Template.Name}' instantiated");
            foreach (var s in this.Template.Sessions)
            {
                Debug.WriteLine($" - session '{s.Name}': {s.Bots.Count} bots");
            }
        }

   
    }

}
