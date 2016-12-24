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

namespace LionFire.Trading.Dash.Wpf
{
    public class SessionViewModel : Screen, IViewModel<Session>
    {
        object IViewModel.Model { get { return Session; } set { Session = (Session)value; } }
        public Session Model { get { return Session; } set { Session = value; } }
        bool IViewModel<Session>.IsViewModelOf(object obj) { return obj as Session != null; }

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
        public Session Session { get; set; }
        public SessionViewModel()
        {
        }
        public SessionViewModel(WorkspaceViewModel parent, Session session)
        {
            //this.WorkspaceVM = parent;
            this.Session = session;
            //Screens.Add(new SymbolsViewModel());
        }

        #region Lifecycle

        protected override void OnActivate()
        {
            base.OnActivate();
        }


        #endregion

        public ObservableCollection<BotViewModel> LiveBots { get; set; } = new ObservableCollection<BotViewModel>();
        public ObservableCollection<BotViewModel> DemoBots { get; set; } = new ObservableCollection<BotViewModel>();
        public ObservableCollection<BotViewModel> Scanners { get; set; } = new ObservableCollection<BotViewModel>();


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
