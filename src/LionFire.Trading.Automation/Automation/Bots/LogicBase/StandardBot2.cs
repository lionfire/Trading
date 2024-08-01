using DynamicData;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;

public abstract class PStandardBot2<TConcrete, TValue> : PBarsBot2<TConcrete, TValue>
    where TConcrete : PStandardBot2<TConcrete, TValue>
{
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
{

    #region Lifecycle

    public override void Init()
    {
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
        return Account.ExecuteMarketOrder(Symbol, Parameters.Direction, amount == null ? Parameters.PositionSize : Parameters.PositionSize * amount.Value);
    }

    public virtual ValueTask<IOrderResult> ClosePositionPortion(double? amount = null)
    {
        if (Positions.Count == 0) { return ValueTask.FromResult(OrderResult.NoopSuccess); }
        double amountToClose = amount == null ? Parameters.PositionSize : Parameters.PositionSize * amount.Value;


        foreach (var p in Positions.KeyValues)
        {
            if (p.Value.Volume == amountToClose)
            {
                Account.ClosePosition(p.Value);
                return ValueTask.FromResult(OrderResult.Success);
            }
        }

        foreach (var p in Positions.KeyValues)
        {

        }

        if (Parameters.CloseAllAtOnce)
        {
            //PrimaryAccount.ClosePositionsForSymbol(Symbol, )
            throw new NotImplementedException();

        }
        else
        {

            throw new NotImplementedException();

        }
    }

    #endregion
}
