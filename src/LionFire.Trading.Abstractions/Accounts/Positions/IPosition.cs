using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading;

public interface IPosition
{
    string Comment { get; }
    decimal Commissions { get; }
    decimal EntryPrice { get; }
    DateTime EntryTime { get; }
    decimal GrossProfit { get; }
    int Id { get; }
    string Label { get; }
    decimal NetProfit { get; }
    decimal Pips { get; }
    decimal Quantity { get; }
    decimal? StopLoss { get; }
    string? StopLossWorkingType { get; set; }
    decimal Swap { get; }

    SymbolId SymbolId { get; }
    string SymbolCode { get; }
    decimal? TakeProfit { get; }
    TradeKind TradeType { get; }
    long Volume { get; }



    decimal? LastPrice { get; set; }
    decimal? LiqPrice { get; set; }
    decimal? MarkPrice { get; set; }
    decimal? UsdEquivalentQuantity { get; set; }

}
