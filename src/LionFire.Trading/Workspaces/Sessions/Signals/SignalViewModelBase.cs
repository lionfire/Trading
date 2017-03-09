using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LionFire.Trading.Workspaces
{

    public class SignalViewModelBase : INotifyPropertyChanged
    {
        public Session Session { get; set; }

        public SignalViewModelBase()
        {
        }

        #region DisplayName

        public string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName == value) return;
                displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        private string displayName;

        #endregion

        public IEnumerable<string> UnderlyingSymbolCodes { get; private set; }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        ConcurrentDictionary<string, SignalChangeNotifier> dict = new ConcurrentDictionary<string, SignalChangeNotifier>();

        public SignalChangeNotifier GetSignalChangeNotifier(TimeFrame timeFrame)
        {
            return dict.GetOrAdd(timeFrame.Name, tf => new SignalChangeNotifier(this, timeFrame));
        }

        #region Misc

        public override string ToString()
        {
            return DisplayName ?? this.GetType().Name;
        }

        #endregion
    }

}
