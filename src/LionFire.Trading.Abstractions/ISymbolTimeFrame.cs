using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    

    public interface ISymbolTimeFrame
    {
        ISymbol Symbol { get; }
        ITimeFrame TimeFrame { get; }
    }
}
