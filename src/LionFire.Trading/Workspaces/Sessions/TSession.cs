using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Templating;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Workspaces
{
    public class TSession : ITemplate<Session>
    {
        #region Construction

        public TSession() { }
        public TSession(string name) { this.Name = name; }
        public TSession(BotMode mode, string name = null) { this.Mode = mode; this.Name = name ?? mode.ToString(); }

        #endregion

        #region Description

        public string Name { get; set; }
        public string Description { get; set; }

        #endregion

        #region State

        public ExecutionState DesiredExecutionState { get; set; } = ExecutionState.Started;

        /// <summary>
        /// Safety switch for live bots
        /// </summary>
        public bool AllowLiveBots { get; set; }

        //public string ModeName { get {
        //        if (AllowLiveBots) return "Live";
        //        else if (AllowDemoBots) return "Demo";
        //        else return "View";
        //    } }

        /// <summary>
        /// Valid modes for a session:
        ///  - zero or oneof: Live/Demo,
        ///  - Paper, Scanner optional
        /// </summary>
        public BotMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                var testVal = value & ~BotMode.Paper;
                testVal = testVal & ~BotMode.Scanner;
                switch (testVal)
                {
                    case BotMode.None:
                    case BotMode.Live:
                    case BotMode.Demo:
                        break;
                    default:
                        throw new ArgumentException("Valid modes for a session: zero or one of: Live/Demo, Scanner, Paper optional");
                }
                this.mode = value;
            }
        }
        private BotMode mode;

        public bool IsPaperMode
        {
            get { return Mode.HasFlag(BotMode.Paper); }
        }

        #endregion

        #region Accounts

        public string LiveAccount { get; set; }
        public string DemoAccount { get; set; }

        public AccountMode PaperAccountMode { get; set; } = AccountMode.Any;
        public AccountMode ScannerAccountMode { get; set; } = AccountMode.Any;

        #endregion

        #region Participants

        public List<string> LiveBots { get; set; }
        public List<string> DemoBots { get; set; }
        public List<string> PaperBots { get; set; }
        public List<string> Scanners { get; set; }

        #endregion

        public HashSet<string> EnabledSymbols { get; set; }
        public HashSet<string> DisabledSymbols { get; set; }

        public override string ToString()
        {
            return $"{{TSession \"{Name}\" {Mode}}}";
        }
    }
}
