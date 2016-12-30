using Caliburn.Micro;
using LionFire.Trading.Workspaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LionFire.Caliburn.Micro;
using System.Windows;
using System.Globalization;
using LionFire.Avalon;
using LionFire.Assets;
using System.Windows.Controls;
using LionFire.Structures;
using LionFire.States;
using System.Threading;

namespace LionFire.Trading.Dash.Wpf
{

    public class WorkspaceState
    {
        public string SessionNewItemTarget { get; set; }
    }


    // Primary child: Session
    public class WorkspaceViewModel : Conductor<IScreen>.Collection.AllActive, IHasStateType, IHandle<WorkspaceDataChanged>
    {
        public void Handle(WorkspaceDataChanged dc)
        {
            Workspace?.Template?.QueueAutoSave();
        }

        #region Children

        public WorkspaceExplorerViewModel WorkspaceExplorerViewModel { get; set; }

        public VmCollection<SessionViewModel, Session> Sessions { get; set; }

        #region NumberOfLiveBots

        public int NumberOfLiveBots
        {
            get { return numberOfLiveBots; }
            set
            {
                if (numberOfLiveBots == value) return;
                numberOfLiveBots = value;
                NotifyOfPropertyChange(() => NumberOfLiveBots);
            }
        }
        private int numberOfLiveBots;


        #endregion

        public HistoricalDataViewModel HistoricalData { get; private set; }

        #endregion

        public CancellationTokenSource deactivating = new CancellationTokenSource();

        #region State

        Type IHasStateType.StateType => typeof(WorkspaceState);


        #region SessionNewItemTarget

        [State]
        public string SessionNewItemTarget
        {
            get { return sessionNewItemTarget; }
            set
            {
                if (sessionNewItemTarget == value) return;
                sessionNewItemTarget = value;
                NotifyOfPropertyChange(() => SessionNewItemTarget);
            }
        }
        private string sessionNewItemTarget;

        #endregion

        #endregion

        #region Lifecycle

        public WorkspaceViewModel()
        {
            //ManualSingleton<IEventAggregator>.Instance.Subscribe(this);
        }

        public WorkspaceViewModel(Workspace workspace) : base()
        {
            this.Workspace = workspace;
            //ManualSingleton<IEventAggregator>.Instance.Subscribe(this);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ManualSingleton<IEventAggregator>.Instance.Subscribe(this);


            Task.Run(async () =>
            {
                await InitWorkspace();

                //ActivateItem(HistoricalData = new HistoricalDataViewModel() { Parent = this });

                IScreen selectedItem = null;

                foreach (var tItem in Workspace.Template.Items)
                {
                    var session = Sessions.Where(s => s.Session.Name == tItem.Session).FirstOrDefault();
                    if (session == null)
                    {
                        throw new Exception($"Error loading from configuration: workspace item referenced session with name {tItem.Session}, which was not found.");
                    }
                    var type = ViewModelResolver.GetViewModelType(tItem.View);
                    var item = (IWorkspaceViewModel)Activator.CreateInstance(type);
                    item.SessionViewModel = session;
                    item.WorkspaceViewModel = this;

                    (item as IHasStateType)?.SetState(tItem.State);

                    if (tItem.IsSelected && selectedItem == null)
                    {
                        // Save activation for last
                        selectedItem = item;
                    }
                    else
                    {
                        ActivateItem(item);
                    }
                }
                if (selectedItem != null)
                {
                    ActivateItem(selectedItem);
                }

                //if (WorkspaceExplorerViewModel == null)
                //{
                //    WorkspaceExplorerViewModel = new WorkspaceExplorerViewModel(this);
                //    Items.Add(WorkspaceExplorerViewModel);
                //    ActivateItem(WorkspaceExplorerViewModel);
                //}

                deactivating = new CancellationTokenSource();
                var timerTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        UpdateStats();
                        await Task.Delay(1000, deactivating.Token);
                    }
                }, deactivating.Token);

            });
        }

        private void UpdateStats()
        {
            if (StartTime == default(DateTime))
            {
                StartTime = DateTime.UtcNow;
            }
            TotalTicks = Sessions.Select(s => s.Session.Account.AccountStats.Totals.Ticks).Sum();
            TotalOther = Sessions.Select(s => s.Session.Account.AccountStats.Totals.Other).Sum();

            TicksPerMinute = (int)(TotalTicks / (DateTime.UtcNow - StartTime).TotalMinutes);
            OtherPerMinute = (int)(TotalOther / (DateTime.UtcNow - StartTime).TotalMinutes);

        }
        public DateTime StartTime { get; set; }


        #region TicksPerMinute

        public int TicksPerMinute
        {
            get { return ticksPerMinute; }
            set
            {
                if (ticksPerMinute == value) return;
                ticksPerMinute = value;
                NotifyOfPropertyChange(() => TicksPerMinute);
            }
        }
        private int ticksPerMinute;

        #endregion


        #region OtherPerMinute

        public int OtherPerMinute
        {
            get { return otherPerMinute; }
            set
            {
                if (otherPerMinute == value) return;
                otherPerMinute = value;
                NotifyOfPropertyChange(() => OtherPerMinute);
            }
        }
        private int otherPerMinute;

        #endregion





        #region TotalTicks

        public long TotalTicks
        {
            get { return totalTicks; }
            set
            {
                if (totalTicks == value) return;
                totalTicks = value;
                NotifyOfPropertyChange(() => TotalTicks);
            }
        }
        private long totalTicks;

        #endregion

        #region TotalOther

        public long TotalOther
        {
            get { return totalOther; }
            set
            {
                if (totalOther == value) return;
                totalOther = value;
                NotifyOfPropertyChange(() => TotalOther);
            }
        }
        private long totalOther;

        #endregion



        private async Task InitWorkspace()
        {
            // REVIEW - manage all this state better
            Workspace.StatusText = "Starting workspace";
            await Workspace.Initialize().ConfigureAwait(continueOnCapturedContext: false);
            await Workspace.Start().ConfigureAwait(continueOnCapturedContext: false);

            Sessions = new VmCollection<SessionViewModel, Session>(workspace.Sessions, new VMP(this));

            Workspace.StatusText = "Started workspace";

            var account = DefaultCTraderAccount;

            if (account != null)
            {
                account.StatusTextChanged += OnAccountStatusTextChanged;
                Workspace.StatusText = account.StatusText;
            }
            else
            {
                Workspace.StatusText = "No account configured";
            }

            OnAccountStatusTextChanged();
        }

        #endregion


        protected IAccount DefaultCTraderAccount
        {
            get { return Workspace.Accounts.OfType<Spotware.Connect.CTraderAccount>().FirstOrDefault(); }
        }
        void OnAccountStatusTextChanged()
        {
            var account = DefaultCTraderAccount;
            if (account != null)
            {
                Workspace.StatusText = account.StatusText;
            }
            else
            {
                Workspace.StatusText = "No account configured";
            }
        }


        #region Workspace

        public class VMP : IViewModelProvider
        {
            WorkspaceViewModel parent;
            public VMP(WorkspaceViewModel parent)
            { this.parent = parent; }

            public T GetFor<T>(object model, object context)
            {
                return (T)(object)new SessionViewModel(parent, (Session)model);
            }
        }

        [SetOnce]
        public Workspace Workspace
        {
            get { return workspace; }
            set
            {
                if (workspace == value) return;
                workspace = value;

                NotifyOfPropertyChange(() => Workspace);

                //Sessions.Clear();
                //foreach (var session in Workspace.Sessions)
                //{
                //    Sessions.Add(new SessionViewModel(this, session));
                //}
                //foreach (var session in Sessions)
                //{
                //    this.ActivateItem(session);
                //}
            }
        }

        private Workspace workspace;

        public override string DisplayName
        {
            get
            {
                return Workspace.Template.Name;
            }

            set
            {
                Workspace.Template.Name = value;
            }
        }

        #endregion

        #region New Workspace Item

        public IEnumerable<Type> _NewItemTypes
        {
            get
            {
                yield return typeof(HistoricalDataViewModel);
                yield return typeof(SymbolsViewModel);
                yield return typeof(SeriesDataViewModel);

            }
        }

        //VmCollection<SessionViewModel, Session> sessions = new VmCollection<SessionViewModel, Session>();

        private bool TryEnsureSessionNewTarget()
        {
            bool gotOne = false;
            foreach (var s in Sessions)
            {
                if (gotOne)
                {
                    s.IsNewTarget = false;
                }
                else if (s.IsNewTarget)
                {
                    gotOne = true;
                }

            }
            if (!gotOne)
            {
                foreach (var s in Sessions) { s.IsNewTarget = true; return true; }
                return false;
            }
            return true;
        }

        public IEnumerable<object> NewItemTypes
        {
            get
            {
                TryEnsureSessionNewTarget();
                foreach (var session in Sessions)
                {
                    yield return session;
                    //yield return new SessionSelectViewModel
                    //{
                    //    Parent = this,
                    //    DisplayName = "Session: " + session.Session.Name,
                    //    Session = session,
                    //};
                }

                yield return null;

                foreach (var item in _NewItemTypes)
                {
                    yield return new AddNewViewModelMenuItemViewModel
                    {
                        Parent = this,
                        DisplayName = TypeToDisplayName.ToCamelCase(item.Name.Replace("ViewModel", "")),
                        Type = item,
                        Initializer = o => InitializeNewScreen(o as WorkspaceScreen),
                    };
                }
            }
        }
        private void InitializeNewScreen(WorkspaceScreen workspaceScreen)
        {
            if (workspaceScreen == null) return;
            TryEnsureSessionNewTarget();
            workspaceScreen.SessionViewModel = Sessions.Where(s => s.IsNewTarget).FirstOrDefault();

            // TODO: http://stackoverflow.com/questions/39396035/how-to-select-tabs-with-avalondock
            //WorkspaceView a;
            //a.LayoutDocumentPane.SelectedContent
        }

        #endregion

        #region New Session

        private int NewSessionCounter = 1;
        public void NewSession()
        {
            var session = new Session();
            session.Name = "Session " + NewSessionCounter++;
            ActivateItem(new SessionViewModel(this, session));
        }

        #endregion

        #region Save/Load

        public void Save()
        {
            if (Workspace.Template.Name == null)
            {
                throw new Exception("Can't save when name is null");
            }

            foreach (var item in Items)
            {
            }

            Workspace.Template.Save();
        }

        #endregion


        #region Misc

        public override string ToString()
        {
            return Workspace?.Template.Name;
        }


        #endregion

    }

    //public class SessionSelectViewModel : PropertyChangedBase
    //{
    //    public string DisplayName { get; set; }
    //    public override string ToString()
    //    {
    //        return DisplayName ?? "(Unnamed SessionSelectViewModel)";
    //    }
    //    public WorkspaceViewModel Parent { get; set; }

    //    public SessionViewModel Session { get; set; }


    //    #region IsNewTarget

    //    public bool IsNewTarget // TODO - 
    //    {
    //        get { return isNewTarget; }
    //        set
    //        {
    //            if (isNewTarget == value) return;
    //            isNewTarget = value;
    //            NotifyOfPropertyChange(() => IsNewTarget);
    //        }
    //    }
    //    private bool isNewTarget;

    //    #endregion

    //}



    //public interface IWorkspaceViewModel : IScreen
    //{
    //    TWorkspaceItem Template { get; set; }
    //}

}

