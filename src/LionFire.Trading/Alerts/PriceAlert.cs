using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class PriceAlert
    {
        public string SymbolCode { get; set; }
        public string Operator { get; set; }
        public double Price { get; set; }
    }
}
