using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using LionFire.Trading.Workspaces;
using LionFire.Templating;
using Newtonsoft.Json.Linq;
using LionFire.Trading.Bots;
using System.Windows;
using LionFire.Parsing.String;
using LionFire.Avalon;
using LionFire.Structures;
using IEventAggregator = Caliburn.Micro.IEventAggregator;
using System.Collections.Specialized;

namespace LionFire.Trading.Dash.Wpf
{
    public class BotViewModelProvider : IViewModelProvider
    {
        public static BotViewModelProvider Instance { get { return Singleton<BotViewModelProvider>.Instance; } }
        public T GetFor<T>(object model, object context)
        {
            return (T) (object)new BotViewModel((IBot)model);
        }
    }
    public class SessionViewModel : Screen, IViewModel<Session>
    {
        object IViewModel.Model { get { return Session; } set { Session = (Session)value; } }
        public Session Model { get { return Session; } set { Session = value; } }
        bool IViewModel<Session>.IsViewModelOf(object obj) { return obj as Session != null; }

        #region Construction

        public SessionViewModel()
        {
        }
        public SessionViewModel(WorkspaceViewModel parent, Session session)
        {
            //this.WorkspaceVM = parent;
            this.Session = session;

            //Screens.Add(new SymbolsViewModel());
        }

        #endregion

        #region IsNewTarget

        public bool IsNewTarget
        {
            get { return isNewTarget; }
            set
            {
                if (isNewTarget == value) return;
                isNewTarget = value;
                NotifyOfPropertyChange(() => IsNewTarget);
            }
        }
        private bool isNewTarget;

        #endregion

        public string DisplayType
        {
            get
            {
                return Session?.Template?.Mode.ToString();
            }
        }

        public override string DisplayName
        {
            get
            {
                return Session.Name;
            }
            set
            {
                Session.Name = value;
            }
        }

        //public BindableCollection<object> Screens { get; set; } = new BindableCollection<object>();

        public WorkspaceViewModel WorkspaceVM { get { return Parent as WorkspaceViewModel; } }
        public Workspace Workspace { get { return WorkspaceVM?.Workspace; } }

        #region Session

        public Session Session
        {
            get { return session; }
            set
            {
                if (session == value) { return; }

                session = value;
                LiveBots = (Session?.LiveBots == null) ? null : new VmCollection<BotViewModel, IBot>(Session.LiveBots, BotViewModelProvider.Instance);
                DemoBots = (Session?.DemoBots == null) ? null : new VmCollection<BotViewModel, IBot>(Session.DemoBots, BotViewModelProvider.Instance);
                Scanners = (Session?.Scanners == null) ? null : new VmCollection<BotViewModel, IBot>(Session.Scanners, BotViewModelProvider.Instance);
                PaperBots = (Session?.PaperBots == null) ? null : new VmCollection<BotViewModel, IBot>(Session.PaperBots, BotViewModelProvider.Instance);

                foreach (var incc in NotifyingCollections)
                {
                    incc.CollectionChanged += NotifyingCollectionChanged;
                }
            }
        }

        public IEnumerable<INotifyCollectionChanged> NotifyingCollections
        {
            get {
                if (LiveBots != null) yield return LiveBots;
                if (DemoBots != null) yield return DemoBots;
                if (Scanners != null) yield return Scanners;
                if (PaperBots != null) yield return PaperBots;
            }
        }

        private void NotifyingCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ManualSingleton<IEventAggregator>.Instance.Publish(new WorkspaceDataChanged(), a=>a());
        }

        private Session session;

        #endregion


        

        #region Lifecycle

        protected override void OnActivate()
        {
            base.OnActivate();
        }


        #endregion

        public VmCollection<BotViewModel, IBot> LiveBots { get; set; }
        public VmCollection<BotViewModel, IBot> DemoBots { get; set; }
        public VmCollection<BotViewModel, IBot> Scanners { get; set; }
        public VmCollection<BotViewModel, IBot> PaperBots { get; set; }

        #region Symbols

        public ObservableCollection<SymbolViewModel> Symbols
        {
            get { return symbols; }
            set
            {
                if (symbols == value) return;
                symbols = value;
                NotifyOfPropertyChange(() => Symbols);
            }
        }

        private ObservableCollection<SymbolViewModel> symbols;

        #endregion


        public void IsNewTargetClicked(object item)
        {
            var clickedSession = item as SessionViewModel;
            if (clickedSession == null) return;
            //foreach (var mi in ((Menu)menuItem.Parent).Items.OfType<MenuItem>())
            //{
            //    mi.IsChecked = false;
            //}

            foreach (var sessionVM in clickedSession.WorkspaceVM.Sessions)
            {
                if (sessionVM != clickedSession)
                {
                    sessionVM.IsNewTarget = false;
                }
                else
                {
                    sessionVM.IsNewTarget = true;
                }
            }

            //Workspace.UpdateSessionNewTarget();


            //var menuItem = item as MenuItem;
            //if (menuItem != null)
            //{
            //    menuItem.IsChecked = true;
            //}
            //var obj = Activator.CreateInstance(Type) as IScreen;
            //if (obj != null)
            //{
            //    Parent.ActivateItem(obj);
            //}
        }

    }
}
