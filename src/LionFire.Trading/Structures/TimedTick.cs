using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public struct TimedTick
    {
        public DateTime Time;
        public double Bid;
        public double Ask;

        public bool HasBid { get { return !double.IsNaN(Bid); } }
        public bool HasAsk { get { return !double.IsNaN(Ask); } }
    }

}
