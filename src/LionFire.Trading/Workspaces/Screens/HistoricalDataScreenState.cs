using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces.Screens
{
    public class HistoricalDataScreenState
    {
        public bool CacheMode { get; set; }
        public DateTime From { get; set; }
        public string SelectedSymbolCode { get; set; }
        public string SelectedTimeFrame { get; set; }
    }
}
