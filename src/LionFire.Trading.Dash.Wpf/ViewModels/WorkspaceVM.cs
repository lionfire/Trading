using Caliburn.Micro;
using LionFire.Trading.Workspaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LionFire.Trading.Dash.Wpf
{
    public class SessionVM
    {
        public WorkspaceVM Parent { get; set; }

        public SessionVM(WorkspaceVM parent) { this.Parent = parent; }


        public ObservableCollection<BotVM> LiveBots { get; set; } = new ObservableCollection<BotVM>();
        public ObservableCollection<BotVM> DemoBots { get; set; } = new ObservableCollection<BotVM>();
        public ObservableCollection<BotVM> Scanners { get; set; } = new ObservableCollection<BotVM>();


        #region Symbols

        public ObservableCollection<SymbolVM> Symbols
        {
            get { return symbols; }
            set
            {
                if (symbols == value) return;
                symbols = value;
                OnPropertyChanged(nameof(Symbols));
            }
        }
        private ObservableCollection<SymbolVM> symbols;

        #endregion

    }

    public class WorkspaceVM : PropertyChangedBase
    {
        #region Children

        BindableCollection<SessionVM> Sessions { get; set; }

        public HistoricalDataVM HistoricalData { get; private set; }

        #endregion

        public WorkspaceVM()
        {
            HistoricalData = new HistoricalDataVM() { Parent = this };
            Sessions.Add(new SessionVM(this) { });
        
        }

        public Workspace Workspace { get; set; }

    }

}
