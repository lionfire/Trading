using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Instruments
{
    public class Commodities
    {
        // FUTURE: Set these symbols up per broker. 
        public static List<string> Symbols = new List<string>()
        {
            "XAU",
            "XAG",
            "WTI",
        };
    }
}
