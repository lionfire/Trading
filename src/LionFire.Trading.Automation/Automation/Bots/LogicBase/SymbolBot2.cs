using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;


public abstract class PSymbolBot2<TConcrete> : PBot2<TConcrete> 
    where TConcrete : PBot2<TConcrete>
{
    public abstract ExchangeSymbol ExchangeSymbol { get; }
}



public class SymbolBot2<TParameters, TValue> : Bot2<TParameters>
      where TParameters : PSymbolBot2<TParameters>
{

    #region Parameters (Derived)

    public ExchangeSymbol ExchangeSymbol { get; set; } = default!;
    public string Symbol => ExchangeSymbol.Symbol!;

    public IAccount2 Account { get; set; } = default!;

    #endregion

    #region Lifecycle

    public SymbolBot2() { }

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
