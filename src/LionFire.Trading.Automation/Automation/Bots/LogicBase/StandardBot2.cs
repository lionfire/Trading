namespace LionFire.Trading.Automation;

public abstract class PStandardBot2<TParameters> : PBot2<TParameters>
    where TParameters : PBot2<TParameters>
{
    public LongAndShort Direction { get; set; }

    public double PositionSize { get; set; } = 1;

    public int MaxPositions { get; set; } = 1;

    public bool CloseAllAtOnce { get; set; }
}

public class BasicBot2<TParameters> : Bot2<TParameters>
      where TParameters : PBot2<TParameters>
{

    public BasicBot2()
    {
    }

    public override void Init()
    {
        base.Init();
        if (Controller == null) throw new ArgumentNullException(nameof(Controller));

        //if(Parameters is IPTimeFrameMarketProcessor tf)
        //{
        //}
        if (PrimaryExchangeSymbol == null && Parameters is IPSymbolBarsBot2 s)
        {
            PrimaryExchangeSymbol = s.ExchangeSymbol;
        }

        if (PrimaryAccount == null && PrimaryExchangeSymbol != null)
        {
            PrimaryAccount = Controller.GetAccount(PrimaryExchangeSymbol);
        }

        if (PrimaryExchangeSymbol == null) throw new InvalidOperationException($"Failed to resolve {nameof(PrimaryExchangeSymbol)}");
        if (PrimaryAccount == null) throw new InvalidOperationException($"Failed to resolve {nameof(PrimaryAccount)}");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
    }

    public ExchangeSymbol PrimaryExchangeSymbol { get; set; } = default!;
    public IAccount2 PrimaryAccount { get; set; } = default!;
}

public abstract class StandardBot2<TParameters> : BasicBot2<TParameters>
      where TParameters : PStandardBot2<TParameters>
{
    public string Symbol { get; protected set; } = default!;

    public override void Init()
    {
        base.Init();
        Symbol = PrimaryExchangeSymbol.Symbol ?? throw new InvalidOperationException($"Failed to resolve {nameof(Symbol)}"); ;
    }

    public virtual ValueTask<IOrderResult> OpenPositionPortion(double? amount = null)
    {
        return PrimaryAccount.ExecuteMarketOrder(Symbol, Parameters.Direction, amount == null ? Parameters.PositionSize : Parameters.PositionSize * amount.Value);
    }

    public virtual ValueTask<bool> ClosePositionPortion(double? amount = null)
    {
        if (Parameters.CloseAllAtOnce)
        {
            PrimaryAccount.ClosePositionsForSymbol(Symbol, )
            throw new NotImplementedException();

        }
        else
        {
            throw new NotImplementedException();

        }
    }
}
