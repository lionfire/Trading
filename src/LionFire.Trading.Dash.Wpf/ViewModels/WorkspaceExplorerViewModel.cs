using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Workspaces;

namespace LionFire.Trading.Dash.Wpf
{
    public class WorkspaceExplorerViewModel : Screen
    {
        private WorkspaceViewModel workspaceViewModel;

        public BindableCollection<SessionViewModel> Sessions = new BindableCollection<SessionViewModel>();

        public WorkspaceExplorerViewModel(WorkspaceViewModel workspaceViewModel)
        {
            this.workspaceViewModel = workspaceViewModel;
            
        }


        #region ShowLive

        public bool ShowLive
        {
            get { return showLive; }
            set
            {
                if (showLive == value) return;
                showLive = value;
                NotifyOfPropertyChange(() => ShowLive);
            }
        }
        private bool showLive = true;

        #endregion

        #region ShowDemo

        public bool ShowDemo
        {
            get { return showDemo; }
            set
            {
                if (showDemo == value) return;
                showDemo = value;
                NotifyOfPropertyChange(() => ShowDemo);
            }
        }
        private bool showDemo = true;

        #endregion

        #region ShowScanners

        public bool ShowScanners
        {
            get { return showScanners; }
            set
            {
                if (showScanners == value) return;
                showScanners = value;
                NotifyOfPropertyChange(() => ShowScanners);
            }
        }
        private bool showScanners = true;

        #endregion

        #region ShowPaper

        public bool ShowPaper
        {
            get { return showPaper; }
            set
            {
                if (showPaper == value) return;
                showPaper = value;
                NotifyOfPropertyChange(() => ShowPaper);
            }
        }
        private bool showPaper = true;

        #endregion



        protected override void OnActivate()
        {
            base.OnActivate();



            var sessionVM = new SessionViewModel(workspaceViewModel,
                new Session() { Name = "testSession456" });
            Sessions.Add(sessionVM);

            Sessions.Add(new SessionViewModel(workspaceViewModel,
                new Session() { Name = "testSession789" }));

        }
    }
}
