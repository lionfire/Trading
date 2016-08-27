using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{

    public interface History : IEnumerable<HistoricalTrade>, IEnumerable
    {
        HistoricalTrade this[int index] { get; }

        int Count { get; }

        HistoricalTrade[] FindAll(string label);
        HistoricalTrade[] FindAll(string label, Symbol symbol);
        HistoricalTrade[] FindAll(string label, Symbol symbol, TradeType tradeType);
        HistoricalTrade FindLast(string label);
        HistoricalTrade FindLast(string label, Symbol symbol);
        HistoricalTrade FindLast(string label, Symbol symbol, TradeType tradeType);
    }
}
