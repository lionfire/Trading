using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;

/// <summary>
/// A bot with a primary Bars input (HLC).
/// </summary>
/// <typeparam name="TParameters"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class BarsBot2<TParameters, TValue> : SymbolBot2<TParameters, TValue>, IBarsBot<TValue>
      where TParameters : PBarsBot2<TParameters, TValue>
    where TValue : struct, INumber<TValue>
{
    IPBarsBot2 IBarsBot<TValue>.Parameters => (IPBarsBot2)Parameters;

    #region Injected

    [Signal(-1000)]
    public IReadOnlyValuesWindow<HLC<TValue>> Bars { get; set; } = null!;


    #endregion
}
