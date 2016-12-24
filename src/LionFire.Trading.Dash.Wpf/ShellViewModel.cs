using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using LionFire.Trading.Workspaces;
using LionFire.Templating;

namespace LionFire.Trading.Dash.Wpf
{
    //public class WorkspacesViewModel : Conductor<WorkspaceViewModel>.Collection
    //{

    //}

    
    
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {

        #region Workspaces

        public BindableCollection<WorkspaceViewModel> Workspaces
        {
            get { return workspaces; }
            set
            {
                if (workspaces == value) return;
                workspaces = value;
                NotifyOfPropertyChange(() => Workspaces);
            }
        }
        private BindableCollection< WorkspaceViewModel> workspaces = new BindableCollection<WorkspaceViewModel>();

        #endregion


        #region Test

        public string Test
        {
            get { return test; }
            set
            {
                if (test == value) return;
                test = value;
                NotifyOfPropertyChange(() => Test);
            }
        }
        private string test;

        #endregion

        #region IsConnectedDesired

        public bool IsConnectedDesired
        {
            get { return isConnectedDesired; }
            set
            {
                if (isConnectedDesired == value) return;
                isConnectedDesired = value;
                NotifyOfPropertyChange(() => IsConnectedDesired);
            }
        }
        private bool isConnectedDesired;

        #endregion

        public ShellViewModel()
        {
            
        }

        DashSettings Settings = new DashSettings();

        protected override void OnActivate()
        {
            base.OnActivate();

            // TODO: 
            // - Load last workspace option
            // - Load last workspace
            // - Show welcome tab if nothing loaded

            if (Settings.LoadLastActiveWorkspaces)
            {
                // TODO
            }

            if (workspaces.Count == 0)
            {
                if (Settings.CreateDefaultWorkspaceIfNoneExists)
                {
                    workspaces.Add(new WorkspaceViewModel(TWorkspace.Default.Create()));
                }
                else
                {
                    // Show Welcome tab
                }
            }

            ActiveItem = workspaces.FirstOrDefault();
            
        }

        public void Exit()
        {
            this.TryClose(null);
        }
        public void Save()
        {
            foreach (var workspace in Workspaces)
            {
                workspace.Save();
            }
        }
    }

    

}
