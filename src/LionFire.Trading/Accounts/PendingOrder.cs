using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface PendingOrder
    {
        string Comment { get; }
        DateTime? ExpirationTime { get; }
        int Id { get; }
        string Label { get; }
        PendingOrderType OrderType { get; }
        double Quantity { get; }
        double? StopLoss { get; }
        double? StopLossPips { get; }
        string SymbolCode { get; }
        double? TakeProfit { get; }
        double? TakeProfitPips { get; }
        double TargetPrice { get; }
        TradeType TradeType { get; }
        long Volume { get; }
    }
}
