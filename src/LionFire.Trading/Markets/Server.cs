using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class Server
    {
        #region Time

        public DateTime Time {
            get { return time; }
            set {
                time = value;
                LocalDelta = DateTime.UtcNow - value;
            }
        }
        private DateTime time;

        #endregion

        public DateTime ExtrapolatedTime {
            get {
                return DateTime.UtcNow - LocalDelta;
            }
        }

        public TimeSpan LocalDelta {
            get; set;
        }

    }
}
