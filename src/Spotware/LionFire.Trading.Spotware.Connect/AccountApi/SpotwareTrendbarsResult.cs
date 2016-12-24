using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect.AccountApi
{
    public class SpotwareTrendbar
    {
        public long timestamp { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public long volume { get; set; }
    }
    public class SpotwareTrendbarsResult : ISpotwareItemsResult
    {
        public SpotwareTrendbar[] data { get; set; }
        public int Count { get { return data.Length; } }
    }
}
