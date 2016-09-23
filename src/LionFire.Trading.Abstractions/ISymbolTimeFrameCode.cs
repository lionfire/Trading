using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ISymbolTimeFrameCode
    {
        string SymbolCode { get; set; }
        string TimeFrameCode { get; set; }
    }

}
