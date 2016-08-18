using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Feeds
{
    ///// <summary>
    ///// Six character symbols representing one currency or commodity priced in another
    ///// </summary>
    //public class ForexSymbol
    //{
    //    public 
    //}
    //public static class ForexSymbolExtensions
    //{

    //    public static ForexSymbol ToForexSymbol(this string symbol)
    //    {
    //        if (symbol.Length == 6)
    //        {
    //        }

    //    }
    //}

    public class TickQuote
    {
        public string Symbol { get; set; }

        public decimal Bid { get; set; }

        public decimal Ask { get; set; }

        public DateTime Date { get; set; }

    }
}
