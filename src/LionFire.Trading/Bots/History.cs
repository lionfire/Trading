using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
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

    public class _History : History
    {
        List<HistoricalTrade> trades = new List<HistoricalTrade>();

        public HistoricalTrade this[int index] {
            get { return trades[index]; }
        }

        public int Count {
            get {
                return trades.Count;
            }
        }

        public void Add(HistoricalTrade trade)
        {
            trades.Add(trade);
        }

        public HistoricalTrade[] FindAll(string label)
        {
            return trades.Where(t => t.Label == label).ToArray();
        }

        public HistoricalTrade[] FindAll(string label, Symbol symbol)
        {
            return trades.Where(t => t.Label == label && t.SymbolCode == symbol.Code).ToArray();
        }

        public HistoricalTrade[] FindAll(string label, Symbol symbol, TradeType tradeType)
        {
            return trades.Where(t => t.Label == label && t.SymbolCode == symbol.Code && t.TradeType == tradeType).ToArray();
        }

        public HistoricalTrade FindLast(string label)
        {
            return trades.FindLast(t => t.Label == label);
        }

        public HistoricalTrade FindLast(string label, Symbol symbol)
        {
            return trades.FindLast(t => t.Label == label && t.SymbolCode == symbol.Code);
        }

        public HistoricalTrade FindLast(string label, Symbol symbol, TradeType tradeType)
        {
            return trades.FindLast(t => t.Label == label && t.SymbolCode == symbol.Code && t.TradeType == tradeType);
        }

        public IEnumerator<HistoricalTrade> GetEnumerator()
        {
            return trades.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
