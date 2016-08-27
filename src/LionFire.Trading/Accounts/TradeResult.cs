using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    
    public class TradeResult
    {
        public ErrorCode? Error { get; set; }
        public bool IsSuccessful { get; set; }
        public PendingOrder PendingOrder { get; set; }
        public Position Position { get; set; }

        //public override string ToString()
        //{
        //    return 
        //}
    }
}
