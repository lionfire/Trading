using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading;

public interface IPosition
{
    int Id { get; }
    string? Label { get; }
    string? Comment { get; }
    string? StopLossWorkingType { get; set; }
    SymbolId SymbolId { get; }
    string Symbol { get; }
    long Volume { get; }


}

public interface IPosition<TPrecision> : IPosition
    where TPrecision : struct, INumber<TPrecision>
{
    TPrecision Commissions { get; }
    TPrecision? EntryAverage { get; }
    TPrecision RealizedGrossProfit { get; }
    DateTime EntryTime { get; }
    TPrecision GrossProfit { get; }
    TPrecision NetProfit { get; }
    TPrecision Pips { get; }
    TPrecision Quantity { get; }
    LongAndShort LongOrShort { get; }

    TPrecision? StopLoss { get; }
    TPrecision Swap { get; }

    TPrecision? TakeProfit { get; }
    TradeKind TradeType { get; }

    TPrecision? LastPrice { get; set; }
    TPrecision? LiqPrice { get; set; }
    TPrecision? MarkPrice { get; set; }
    TPrecision? UsdEquivalentQuantity { get; set; }
    IAccount2<TPrecision> Account { get; }
}
