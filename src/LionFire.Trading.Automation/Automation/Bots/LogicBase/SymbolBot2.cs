using LionFire.Trading.ValueWindows;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

public interface IPSymbolBot2
{
    ExchangeSymbol ExchangeSymbol { get; }
}

public abstract class PSymbolBot2<TConcrete> : PBot2<TConcrete>, IPSymbolBot2
    where TConcrete : PBot2<TConcrete>
{
    [JsonIgnore]
    public abstract ExchangeSymbol ExchangeSymbol { get; }
}


public class SymbolBot2<TParameters, TValue> : Bot2<TParameters, double>
      where TParameters : PSymbolBot2<TParameters>
    where TValue : struct, INumber<TValue>
{

    #region Parameters (Derived)

    public ExchangeSymbol ExchangeSymbol { get; set; } = default!;
    public string Symbol => ExchangeSymbol.Symbol!;

    public IAccount2<TValue> Account { get; set; } = default!;
    public IAccount2<double> DoubleAccount => Account as IAccount2<double> ?? (doubleAccountAdapter ??= Account == null ? throw new NotSupportedException() : new AccountPrecisionAdapter<double, decimal>(DecimalAccount));
    private IAccount2<double>? doubleAccountAdapter;
    public IAccount2<decimal> DecimalAccount => Account as IAccount2<decimal> ?? (decimalAccountAdapter ??= Account == null ? throw new NotSupportedException() : new AccountPrecisionAdapter<decimal, double>(DoubleAccount));
    private IAccount2<decimal>? decimalAccountAdapter;

    #endregion

    #region Lifecycle

    public SymbolBot2() { }

    public override void Init()
    {
        base.Init();
        if (Context == null) throw new ArgumentNullException(nameof(Context));

        //if(PMultiSim is IPTimeFrameMarketProcessor tf)
        //{
        //}
        //if (ExchangeSymbol == null && PMultiSim is IPSymbolBarsBot2 s)
        //{
        ExchangeSymbol = Parameters.ExchangeSymbol;
        //}

        Account = Context.Sim.DefaultAccount as IAccount2<TValue> ?? throw new NotImplementedException();

        if (ExchangeSymbol == null) throw new InvalidOperationException($"Failed to resolve {nameof(ExchangeSymbol)}");
        if (Account == null) throw new InvalidOperationException($"Failed to resolve {nameof(Account)}");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
    }

    #endregion

}
