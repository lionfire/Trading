using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using LionFire.Trading.Workspaces;
using LionFire.Templating;
using LionFire.Assets;
using LionFire.Structures;

namespace LionFire.Trading.Dash.Wpf
{
    //public class WorkspacesViewModel : Conductor<WorkspaceViewModel>.Collection
    //{

    //}


    public class ShellApp
    {
        public DashSettings Settings = new DashSettings();
    }

    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public ShellApp App { get; set; } = new ShellApp();

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
        private BindableCollection<WorkspaceViewModel> workspaces = new BindableCollection<WorkspaceViewModel>();

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

        public DashSettings Settings { get { return App.Settings; } }

        protected override void OnActivate()
        {
            base.OnActivate();


            // TODO: UI for
            // - Load last workspace option
            // - Load last workspace
            // - Show welcome tab if nothing loaded

            var lastActiveWorkspace = Settings.LastWorkspace;
            if (Settings.LoadLastActiveWorkspaces)
            {
                var tWorkspace = lastActiveWorkspace.Load<TWorkspace>();
                if (tWorkspace != null)
                {
                    workspaces.Add(new WorkspaceViewModel(tWorkspace.Create()));
                }
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

            this.IsAutoSaveEnabled = Settings.IsAutoSaveEnabled;

            ActiveItem = workspaces.FirstOrDefault();
        }

        public bool IsAutoSaveEnabled
        {
            get { return isAutoSaveEnabled; }
            set
            {
                isAutoSaveEnabled = value;
                foreach (var workspace in workspaces)
                {
                    // FUTURE: Move this to a global autosave manager that autosaves all asset types (unless turned off for that type, or globally)
                    workspace.Workspace.Template.EnableAutoSave(isAutoSaveEnabled);
                }
            }
        }
        private bool isAutoSaveEnabled;


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
