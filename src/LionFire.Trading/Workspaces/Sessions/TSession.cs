using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Instantiating;
using LionFire.ExtensionMethods;
using LionFire.Trading.Bots;
using System.Collections.ObjectModel;
using LionFire.Structures;
using System.ComponentModel;


namespace LionFire.Trading.Workspaces
{

    

    public class TSession : ITemplate<Session>, IChanged
    {
        #region Construction

        public TSession() { }
        public TSession(string name) { this.Name = name; }
        public TSession(BotMode mode, string name = null) { this.Mode = mode; this.Name = name ?? mode.ToString(); }

        #endregion

        #region Description

        #region Name

        public string Name
        {
            get { return name; }
            set
            {
                if (name == value) return;
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string name;

        #endregion

        #region Description

        public string Description
        {
            get { return description; }
            set
            {
                if (description == value) return;
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        private string description;

        #endregion
        
        #endregion

        #region State

        public ExecutionStateEx DesiredExecutionState { get; set; } = ExecutionStateEx.Started;

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

        public ObservableCollection<IInstantiator> Bots
        {
            get { return bots; }
            set
            {
                if (bots != null) bots.CollectionChanged -= CollectionChangedToChanged;
                bots = value;
                if (bots != null) bots.CollectionChanged += CollectionChangedToChanged;
            }
        }
        private ObservableCollection<IInstantiator> bots;

        #endregion

        public HashSet<string> EnabledSymbols { get; set; }
        public HashSet<string> DisabledSymbols { get; set; }

        public override string ToString()
        {
            return this.ToXamlAttribute(nameof(Name), nameof(Mode));
        }

        public string[] BGColors { get; set; }
        public string[] FGColors { get; set; }

        #region Misc

        public event Action<object> Changed;
        private void RaiseChanged() => Changed?.Invoke(this);
        private void CollectionChangedToChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaiseChanged();
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Changed?.Invoke(this);
        }

        #endregion

        #endregion
    }
   
}
