using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{
    public class TSimulatedAccountBase : TMarketAccount
    {
        
        public double StartingBalance { get; set; } = 1000.0;
    }

}
