using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect.AccountApi
{
    public class SpotwareTick
    {
        public long timestamp { get; set; }
        public double bid { get; set; }
        public double ask { get; set; }
    }

    public class SpotwareTicksResult : ISpotwareItemsResult
    {
        public SpotwareTick[] data { get; set; }
        public int Count { get { return data.Length; } }
    }
}
