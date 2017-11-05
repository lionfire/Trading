#if cAlgo
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public static class SymbolExtensions
    {
        public static double CurrentSpread(this Symbol symbol)
        {
            return symbol.Ask - symbol.Bid;
        }
        public static double PointValue(this Symbol symbol)
        {
            return symbol.PipValue / symbol.PipSize;
        }
    }
}
