using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum TradeType
    {
        Buy = 0,
        Sell = 1,
        Either = 2,

    }

    public static class TradeTypeExtensions
    {
        public static string ToLongShort(this TradeType tradeType)
        {
            return tradeType switch
            {
                TradeType.Buy => "Long",
                TradeType.Sell => "Short",
                TradeType.Either => "Long or Short",
                _ => "?",
            };
        }
    }
}
