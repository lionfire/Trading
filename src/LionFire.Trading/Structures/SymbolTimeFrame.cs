using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    

    public class SymbolTimeFrame : ISymbolTimeFrame
    {
        public ISymbol Symbol { get; set; }
        public ITimeFrame TimeFrame { get; set; }
    }
}
