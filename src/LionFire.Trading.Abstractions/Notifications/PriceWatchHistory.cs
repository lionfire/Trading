using System;
using System.Collections.Generic;

namespace LionFire.Trading.Notifications
{
    public class PriceWatchHistory
    {
        public List<KeyValuePair<DateTime , decimal >> PastNotifyTimes { get; set; }
    }
}
