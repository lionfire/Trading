using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
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


// Adds a bit of convenience for the common case of bots that primarily target one symbol on one exchange area.
public class SymbolBot2<TParameters, TValue> : Bot2<TParameters, TValue>
      where TParameters : PSymbolBot2<TParameters>
    where TValue : struct, INumber<TValue>
{

    #region Parameters (Derived)

    public ExchangeSymbol ExchangeSymbol { get; set; } = default!;
    public string Symbol => ExchangeSymbol.Symbol!;

   
    #endregion

    #region Lifecycle

    public SymbolBot2() { }

    public override void Init()
    {
        base.Init();

        //if(PMultiSim is IPTimeFrameMarketProcessor tf)
        //{
        //}
        //if (ExchangeSymbol == null && PMultiSim is IPSymbolBarsBot2 s)
        //{
        ExchangeSymbol = ((PSymbolBot2<TParameters>)Parameters).ExchangeSymbol;
        //}



        if (ExchangeSymbol == null) throw new InvalidOperationException($"Failed to resolve {nameof(ExchangeSymbol)}");
  
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
    }

    #endregion

}
