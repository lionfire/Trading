using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public struct SymbolTick
    {
        public string Symbol;

        public DateTime Time;

        public double Bid;
        public bool HasBid { get { return !double.IsNaN(Bid); } }

        public double Ask;
        public bool HasAsk { get { return !double.IsNaN(Ask); } }
       

        public override string ToString()
        {
            var bid = double.IsNaN(Bid) ? "" : Bid.ToString();
            var ask = double.IsNaN(Ask) ? "" : Ask.ToString();
            return $"{Time} {Symbol} b:{bid} a:{ask}";
        }
    }
}
