using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    
    public class SignalChangeNotifier
    {
        public SignalChangeSettings Settings { get; set; }

        public SignalViewModelBase Parent { get; private set; }

        public TimeFrame Interval { get; private set; }


        public SignalChangeNotifier(SignalViewModelBase parent, TimeFrame interval)
        {
            this.Parent = parent;
            this.Interval = interval;
        }

        #region SignalChangePercent

        // TODO: Wire this up to data sources !!!!!!!!!!!!!!!!!!!!!!!!!!!
        public double SignalChangePercent
        {
            get { return signalChangePercent; }
            set
            {
                if (signalChangePercent == value) return;
                signalChangePercent = value;
                SignalChangePercentChangedForTo?.Invoke(this, value);
            }
        }
        private double signalChangePercent;

        public event Action<SignalChangeNotifier, double> SignalChangePercentChangedForTo;

        #endregion

    }
}
