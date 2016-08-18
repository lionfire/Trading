using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class MarketDataSubscriptions
    {
        public string Symbol { get; set; }
        public TimeFrame TimeFrame { get; set; }
        public List<Actor> Subscribers { get; set; } = new List<Actor>();

        
    }

    
}
