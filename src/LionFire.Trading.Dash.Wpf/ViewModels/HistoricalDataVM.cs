using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash.Wpf
{

    public class HistoricalDataVM : PropertyChangedBase
    {
        public WorkspaceVM Parent { get; set; }

        #region Parameters

        #region From

        public DateTime From
        {
            get { return from; }
            set
            {
                if (from == value) return;
                from = value;
                NotifyOfPropertyChange(() => From);
            }
        }
        private DateTime from;

        #endregion

        #region To

        public DateTime To
        {
            get { return to; }
            set
            {
                if (to == value) return;
                to = value;
                NotifyOfPropertyChange(() => To);
            }
        }
        private DateTime to;

        #endregion

        #endregion

        #region Lookup Data
        

        public IEnumerable<string> SymbolsAvailable
        {
            get
            {
                return Parent.Workspace.Accounts.First().SymbolsAvailable;
                //yield return "XAUUSD";
                //yield return "EURUSD";
                //yield return "US500";
            }
        }

        public IEnumerable<string> TimeFramesAvailable
        {
            get
            {
                yield return "t1";
                yield return "m1";
                yield return "h1";
            }
        }

        #endregion

        #region Selected Item


        #region SelectedTimeFrame

        public string SelectedTimeFrame
        {
            get { return selectedTimeFrame; }
            set
            {
                if (selectedTimeFrame == value) return;
                selectedTimeFrame = value;
                NotifyOfPropertyChange(() => SelectedTimeFrame);
                NotifyOfPropertyChange(() => T1);
                NotifyOfPropertyChange(() => M1);
                NotifyOfPropertyChange(() => H1);

            }
        }
        private string selectedTimeFrame;

        #region Derived

        #region T1

        public bool T1
        {
            get { return SelectedTimeFrame == "t1"; }
            set
            {
                if (value) { SelectedTimeFrame = "t1"; }
            }
        }
        private bool t1;

        #endregion

        #region M1

        public bool M1
        {
            get { return SelectedTimeFrame == "m1"; }
            set
            {
                if (value) { SelectedTimeFrame = "m1"; }
            }
        }
        private bool m1;

        #endregion

        #region H1

        public bool H1
        {
            get { return SelectedTimeFrame == "h1"; }
            set
            {
                if (value) { SelectedTimeFrame = "h1"; }
            }
        }
        private bool h1;

        #endregion

        #endregion

        #endregion

        #region SelectedSymbol

        public string SelectedSymbol
        {
            get { return selectedSymbol; }
            set
            {
                if (selectedSymbol == value) return;
                selectedSymbol = value;
                NotifyOfPropertyChange(() => SelectedSymbol);
            }
        }
        private string selectedSymbol;

        #endregion


        #endregion

        public BindableCollection<string> MissingData { get; private set; } = new BindableCollection<string>();


        public bool CanLoad()
        {
            return true;
        }
        public void Load()
        {
            throw new Exception("Load!!");
        }

        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }

}
