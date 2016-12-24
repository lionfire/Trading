using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using LionFire.Avalon.CaliburnMicro;

namespace LionFire.Trading.Dash.Wpf
{
    public class WorkspaceScreen : LionFireScreen, IWorkspaceViewModel
    {

        public WorkspaceViewModel WorkspaceViewModel { get; set; }

        #region Session

        public SessionViewModel Session
        {
            get { return session; }
            set
            {
                if (session == value) return;
                session = value;
                NotifyOfPropertyChange(() => Session);
                NotifyOfPropertyChange(() => SelectedSessionName);
            }
        }
        private SessionViewModel session;

        #endregion


        #region SelectedSessionName

        public string SelectedSessionName
        {
            get { return session?.Session.Name; }
            set
            {
                Session = WorkspaceViewModel?.Sessions?.Where(s => s.Session.Name == value).FirstOrDefault();
            }
        }

        #endregion

        public IEnumerable<string> SessionsAvailable
        {
            get { return Session?.Workspace?.Sessions?.Select(s => s.Name); }
        }
    }
}
