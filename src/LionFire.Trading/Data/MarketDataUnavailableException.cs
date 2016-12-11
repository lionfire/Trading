using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class MarketDataUnavailableException : Exception
    {
        public MarketDataUnavailableException() { }
        public MarketDataUnavailableException(string message) : base(message) { }
        public MarketDataUnavailableException(string message, Exception inner) : base(message, inner) { }
        
    }
}
