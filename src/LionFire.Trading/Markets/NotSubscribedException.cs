using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class NotSubscribedException : InvalidOperationException
    {
        public NotSubscribedException() : base("First invoke IAccount.Subscribe() before attempting to access market data for this symbol and timeframe.") { }
        public NotSubscribedException(string message) : base(message) { }
        public NotSubscribedException(string message, Exception inner) : base(message, inner) { }
        
    }
}
