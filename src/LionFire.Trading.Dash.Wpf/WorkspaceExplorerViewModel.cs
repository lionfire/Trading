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
