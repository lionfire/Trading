using LionFire.Trading;
using System;
using System.Numerics;

namespace LionFire.Trading;


public interface IFuturesPosition
{
    int Leverage { get; set; }
    bool Isolated { get; set; }
}
public class FuturesPositionBase<TPrecision> : PositionBase<TPrecision>, IFuturesPosition
    where TPrecision : struct, INumber<TPrecision>
{
    public FuturesPositionBase(IAccount2<TPrecision> account, string symbol) : base(account, symbol)
    {
    }

    public int Leverage { get; set; }
    public bool Isolated { get; set; }
}

//public class PositionBase : PositionBase<TPrecision>
//{
//    public PositionBase(IAccount2<TPrecision> account, string symbol) : base(account, symbol) { }
//}

public class PositionBase<TPrecision> : IPosition<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    public PositionBase(IAccount2<TPrecision> account, string symbol)
    {
        Account = account;
        Symbol = symbol;
    }

    public IAccount2<TPrecision> Account { get; set; }
    public string? Comment { get; set; }

    public TPrecision Commissions { get; set; }

    public TPrecision EntryPrice { get; set; }
    public TPrecision? LastPrice { get; set; }
    public TPrecision? MarkPrice { get; set; }
    public TPrecision? LiqPrice { get; set; }

    public DateTime EntryTime { get; set; }

    public TPrecision GrossProfit { get; set; }

    public int Id { get; set; }

    public string? Label { get; set; }

    public TPrecision NetProfit { get; set; }

    public TPrecision Pips { get; set; }

    public TPrecision Quantity { get; set; }

    public Nullable<TPrecision> StopLoss { get; set; }
    public string? StopLossWorkingType { get; set; }

    public TPrecision Swap { get; set; }

    public string Symbol { get; set; }
    //{
    //    get => SymbolId.Symbol;
    //    set => SymbolId.Symbol = new SymbolId { Symbol = value };
    //}
    public SymbolId SymbolId { get; set; }

    public TPrecision? TakeProfit { get; set; }

    public TradeKind TradeType { get; set; }

    public long Volume { get; set; }

    public TPrecision? UsdEquivalentQuantity { get; set; }

    public override string ToString() => $"{TradeType} {Symbol}: {GrossProfit}";
}
