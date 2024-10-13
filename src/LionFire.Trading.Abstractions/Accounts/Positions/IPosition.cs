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

// Optimized: avoid async/await
public interface ISimulatedPosition<TPrecision>
{
    //new TPrecision? StopLoss { get; set; }
}

public interface IPosition<TPrecision> : IPosition
    where TPrecision : struct, INumber<TPrecision>
{
    TPrecision Commissions { get; }
    TPrecision EntryAverage { get; }
    TPrecision RealizedGrossProfit { get; }
    DateTime EntryTime { get; }
    TPrecision GrossProfit { get; }
    TPrecision NetProfit { get; }
    TPrecision Pips { get; }
    TPrecision Quantity { get; }

    /// RENAME: Direction
    LongAndShort LongOrShort { get; }

    TPrecision Swap { get; }

    TradeKind TradeType { get; }

    TPrecision? LastPrice { get; set; }
    TPrecision? LiqPrice { get; set; }
    TPrecision? MarkPrice { get; set; }
    TPrecision? UsdEquivalentQuantity { get; set; }
    IAccount2<TPrecision> Account { get; }

    #region Stop Loss / Take Profit

    TPrecision? StopLoss { get; }
    TPrecision? TakeProfit { get; }

    ValueTask<IOrderResult> SetStopLoss(TPrecision price);
    ValueTask<IOrderResult> SetTakeProfit(TPrecision price);

    #endregion
}

public static class IPositionX
{
    public static TPrecision ProfitAtPrice<TPrecision>(this IPosition<TPrecision> position, TPrecision price)
          where TPrecision : struct, INumber<TPrecision>
    {
        return (price - position.EntryAverage) * position.Quantity;
    }
}
