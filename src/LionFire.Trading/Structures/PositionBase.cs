using LionFire.Trading;
using System;
using System.CommandLine;
using System.Numerics;

namespace LionFire.Trading;


public interface IFuturesPosition
{
    int Leverage { get; set; }
    bool Isolated { get; set; }
}
public abstract class FuturesPositionBase<TPrecision> : PositionBase<TPrecision>, IFuturesPosition
    where TPrecision : struct, INumber<TPrecision>
{
    public FuturesPositionBase(IAccount2<TPrecision> account, string symbol) : base(account, symbol)
    {
    }

    public int Leverage { get; set; }
    public bool Isolated { get; set; }
}

public class SimulatedPosition<TPrecision> : PositionBase<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{

    #region Lifecycle

    public SimulatedPosition(IAccount2<TPrecision> account, string symbol) : base(account, symbol)
    {
    }

    #endregion

    public override ValueTask<IOrderResult> SetStopLoss(TPrecision price)
    {
        StopLoss = price;
        return ValueTask.FromResult<IOrderResult>(OrderResult.Success);
    }
    public override ValueTask<IOrderResult> SetTakeProfit(TPrecision price)
    {
        TakeProfit = price;
        return ValueTask.FromResult<IOrderResult>(OrderResult.Success);
    }

}

public abstract class PositionBase<TPrecision> : IPosition<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity / Immutable

    public int Id { get; set; }
    public DateTime EntryTime { get; set; }


    public string Symbol { get; set; }
    //{
    //    get => SymbolId.Symbol;
    //    set => SymbolId.Symbol = new SymbolId { Symbol = value };
    //}
    public SymbolId SymbolId { get => new() { Symbol = Symbol }; set => throw new NotImplementedException(); }

    #endregion

    #region Relationships

    public IAccount2<TPrecision> Account { get; set; }

    #endregion

    #region Lifecycle

    public PositionBase(IAccount2<TPrecision> account, string symbol)
    {
        Account = account;
        Symbol = symbol;
        //SymbolId = new SymbolId { Symbol = symbol }; // REVIEW
    }

    #endregion

    #region Properties

    public string? Label { get; set; }
    public string? Comment { get; set; }

    #endregion

    #region State

    #region Owned

    private const bool CanSwitchPositionDirection = false;

    public TPrecision Quantity
    {
        get => quantity;
        set
        {
            switch (value)
            {
                case TPrecision n when (n > TPrecision.Zero):
                    longOrShort = LongAndShort.Long;
                    break;
                case TPrecision n when (n < TPrecision.Zero):
                    longOrShort = LongAndShort.Short;
                    break;
                default:
                    break;
            }
            quantity = value;
        }
    }
    private TPrecision quantity;

    public long Volume { get; set; } // TODO: Reconcile with Quantity
    public TPrecision EntryAverage { get; set; }
    public TPrecision RealizedGrossProfit { get; set; }

    public LongAndShort LongOrShort
    {
        get => longOrShort;
        set
        {
            if (longOrShort == value) return;

            if (longOrShort != LongAndShort.Unspecified)
            {
                if (!CanSwitchPositionDirection) { throw new InvalidOperationException("Cannot switch position direction"); }
                else { throw new NotImplementedException(); }
            }
            longOrShort = value;
        }
    }
    private LongAndShort longOrShort;
    public void ResetDirection()
    {
        if(Quantity != TPrecision.Zero) throw new InvalidOperationException("Cannot reset direction while position is open");
        longOrShort = LongAndShort.Unspecified;
    }

    #region Derived

    // REVIEW: redundant to LongOrShort
    public TradeKind TradeType
    {
        get => LongOrShort switch
        {
            LongAndShort.Long => TradeKind.Buy,
            LongAndShort.Short => TradeKind.Sell,
            _ => throw new NotImplementedException(),
        };
        set => LongOrShort = value switch
        {
            TradeKind.Buy => LongAndShort.Long,
            TradeKind.Sell => LongAndShort.Short,
            _ => throw new NotImplementedException(),
        };
    }

    public TPrecision? UsdEquivalentQuantity { get; set; }

    #endregion

    #endregion

    #region Injected

    public TPrecision? LastPrice { get; set; }
    public TPrecision? MarkPrice { get; set; }
    public TPrecision Commissions { get; set; }
    public TPrecision Swap { get; set; }

    #region Derived

    public TPrecision? LiqPrice { get; set; }
    public TPrecision GrossProfit { get; set; }
    public TPrecision NetProfit { get; set; }

    // TODO: Deprecate?
    public TPrecision Pips { get; set; }

    #endregion

    #endregion

    #endregion

    #region SL/TP


    public Nullable<TPrecision> StopLoss { get; set; }
    public abstract ValueTask<IOrderResult> SetStopLoss(TPrecision price);
    public abstract ValueTask<IOrderResult> SetTakeProfit(TPrecision price);
    public string? StopLossWorkingType { get; set; }
    public TPrecision? TakeProfit { get; set; }

    #endregion

    #region Misc

    public override string ToString() => $"{TradeType} {Symbol}: {GrossProfit}";


    #endregion
}
