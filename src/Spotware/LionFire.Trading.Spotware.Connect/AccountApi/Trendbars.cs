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
    public class SpotwareTrendbarsResult
    {
        public SpotwareTrendbar[] data { get; set; }
    }
    public class SpotwareErrorContainer
    {
        public SpotwareError error { get; set; }
    }
    public class SpotwareError
    {
        public string errorCode { get; set; }
        public string description { get; set; }
    }
}
