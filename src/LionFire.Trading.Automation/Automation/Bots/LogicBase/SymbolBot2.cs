namespace LionFire.Trading.Automation;

public abstract class PSymbolBot2<TConcrete> : PBot2<TConcrete>, IPSymbolBarsBot2
    where TConcrete : PBot2<TConcrete>
{
    public ExchangeSymbol ExchangeSymbol { get; init; } = default!;
}

public class SymbolBot2<TParameters> : Bot2<TParameters>
      where TParameters : PSymbolBot2<TParameters>
{

    #region Parameters (Derived)

    public ExchangeSymbol ExchangeSymbol { get; set; } = default!;
    public string Symbol => ExchangeSymbol.Symbol!;

    public IAccount2<double> Account { get; set; } = default!;

    #endregion

    #region Lifecycle

    public SymbolBot2()
    {
    }

    public override void Init()
    {
        base.Init();
        if (Controller == null) throw new ArgumentNullException(nameof(Controller));

        //if(Parameters is IPTimeFrameMarketProcessor tf)
        //{
        //}
        //if (ExchangeSymbol == null && Parameters is IPSymbolBarsBot2 s)
        //{
        ExchangeSymbol = Parameters.ExchangeSymbol;
        //}

        Account = Controller.Account ?? throw new NotImplementedException();

        if (ExchangeSymbol == null) throw new InvalidOperationException($"Failed to resolve {nameof(ExchangeSymbol)}");
        if (Account == null) throw new InvalidOperationException($"Failed to resolve {nameof(Account)}");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
    }

    #endregion

}
