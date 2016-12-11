using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Templating;

namespace LionFire.Trading.Workspaces
{
    public class TSession : ITemplate<Session>
    {
        #region Construction

        public TSession() { }
        public TSession(string name) { this.Name = name; }

        #endregion

        #region Description

        public string Name { get; set; }
        public string Description { get; set; }

        #endregion

        #region State

        public ExecutionState DesiredExecutionState { get; set; } = ExecutionState.Started;

        #endregion

        #region Accounts
                
        public string LiveAccount { get; set; }
        public string DemoAccount { get; set; }
        public string PaperAccount { get; set; }
        public string ScanAccount { get; set; }

        #endregion

        #region Participants

        public List<string> LiveBots { get; set; } 
        public List<string> DemoBots { get; set; }
        public List<string> PaperBots { get; set; }
        public List<string> Scanners { get; set; }

        #endregion

    }
}
