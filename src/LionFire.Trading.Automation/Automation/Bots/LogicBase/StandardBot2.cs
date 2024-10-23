using DynamicData;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;

public abstract class PStandardBot2<TConcrete, TValue> : PBarsBot2<TConcrete, TValue>
    where TConcrete : PStandardBot2<TConcrete, TValue>
{
    protected PStandardBot2() 
    {
    }
    protected PStandardBot2(ExchangeSymbolTimeFrame e) : base(e)
    {
    }

    public LongAndShort Direction { get; set; }

    public double PositionSize { get; set; } = 1;

    public int MaxPositions { get; set; } = 1;

    public bool CloseAllAtOnce { get; set; }
}

public abstract class StandardBot2<TParameters, TValue> : BarsBot2<TParameters, TValue>
    where TParameters : PStandardBot2<TParameters, TValue>
    where TValue : struct, INumber<TValue>
{

    public LongAndShort Direction { get; protected set; }

    #region Lifecycle

    public override void Init()
    {
        Direction = Parameters.Direction switch
        {
            LongAndShort.Long => LongAndShort.Long,
            LongAndShort.Short => LongAndShort.Short,
            _ => Parameters.PositionSize >= 0 ? LongAndShort.Long : LongAndShort.Short
        };
        base.Init();
    }

    #endregion

    #region Methods

    public virtual void TryOpen(double? amount = null)
    {
        // TODO: Don't open if already too many positions open
        OpenPositionPortion(amount);
    }
    public virtual void TryClose(double? amount = null)
    {
        ClosePositionPortion(amount);
    }
    public virtual ValueTask<IOrderResult> OpenPositionPortion(double? amount = null)
    {
        return DoubleAccount.ExecuteMarketOrder(Symbol, Parameters.Direction, amount == null ? Parameters.PositionSize : Parameters.PositionSize * amount.Value, PositionOperationFlags.Default | PositionOperationFlags.AllowCloseAndOpenAtOnce | PositionOperationFlags.Open);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount">Defaults to Parameters.PositionSize</param>
    /// <returns></returns>
    public virtual ValueTask<IOrderResult> ClosePositionPortion(double? amount = null)
    {
        amount = amount == null ? Parameters.PositionSize : Parameters.PositionSize * amount.Value;
        if (Parameters.Direction != LongAndShort.Short) { amount = -amount.Value; }

        return DoubleAccount.ExecuteMarketOrder(Symbol, Parameters.Direction, amount.Value, PositionOperationFlags.Default | PositionOperationFlags.Close | PositionOperationFlags.CloseOnly);

        //if (DoubleAccount.Positions.Count == 0) { return ValueTask.FromResult(OrderResult.NoopSuccess); }
        //double amountToClose = amount == null ? Parameters.PositionSize : Parameters.PositionSize * amount.Value;


        //foreach (var p in DoubleAccount.Positions.KeyValues)
        //{
        //    if (((IPosition<double>)p.Value).Quantity == amountToClose)
        //    {
        //        Account.Close(p.Value);
        //        return ValueTask.FromResult(OrderResult.Success);
        //    }
        //}

        //foreach (var p in Positions.KeyValues)
        //{

        //}

        //if (Parameters.CloseAllAtOnce)
        //{
        //    //PrimaryAccount.ClosePositionsForSymbol(Symbol, )
        //    throw new NotImplementedException();

        //}
        //else
        //{

        //    throw new NotImplementedException();

        //}
    }

    #endregion
}
