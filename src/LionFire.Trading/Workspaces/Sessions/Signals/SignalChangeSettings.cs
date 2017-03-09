using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LionFire.Trading.Workspaces
{
    public enum SessionTimePeriodKind
    {
        Unspecified = 0,
        /// <summary>
        /// E.g. show the last 24 hours
        /// </summary>
        TimeSpan = 1<<0,

        /// <summary>
        /// E.g. Show since 1hr before Euro session open, or US session
        /// </summary>
        PreSessionStart = 1 << 1,

        /// <summary>
        /// Show since major session open (Syd, Tok, Lon, NY)
        /// </summary>
        SessionStart = 1 << 1,

    }

    public class SignalChangeSettings : System.ComponentModel.INotifyPropertyChanged
    {

        #region SessionTimePeriodKind

        public SessionTimePeriodKind SessionTimePeriodKind
        {
            get { return sessionTimePeriodKind; }
            set
            {
                if (sessionTimePeriodKind == value) return;
                sessionTimePeriodKind = value;
                OnPropertyChanged(nameof(SessionTimePeriodKind));
            }
        }
        private SessionTimePeriodKind sessionTimePeriodKind;

        #endregion

        #region TimeFrames

        public ObservableCollection<TimeFrame> TimeFrames
        {
            get { return timeFrames; }
            set
            {
                if (timeFrames == value) return;
                timeFrames = value;
                OnPropertyChanged(nameof(TimeFrames));
            }
        }
        private ObservableCollection<TimeFrame> timeFrames;

        #endregion
        
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
