using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public interface HistoricalTrade
    {
        double Balance { get; }
        int ClosingDealId { get; }
        double ClosingPrice { get; }
        DateTime ClosingTime { get; }
        string Comment { get; }
        double Commissions { get; }
        double EntryPrice { get; }
        DateTime EntryTime { get; }
        double GrossProfit { get; }
        string Label { get; }
        double NetProfit { get; }
        double Pips { get; }
        int PositionId { get; }
        double Quantity { get; }
        double Swap { get; }
        string SymbolCode { get; }
        TradeType TradeType { get; }
        long Volume { get; }
    }
}
