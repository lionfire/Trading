using LionFire.Trading.Automation.Bots;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.ValueWindows;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

public interface IPBarsBot2
{
    ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; set; }

    void FinalizeInit();
}

[ContainsParameters]
public abstract class PBarsBot2<TConcrete, TValue>
    : PSymbolBot2<TConcrete>
    , IPBarsBot2
    , IPTimeFrameBot2
    where TConcrete : PBarsBot2<TConcrete, TValue>
{
    [JsonIgnore]
    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; set; }

    #region Derived

    [JsonIgnore]
    public override ExchangeSymbol ExchangeSymbol => ExchangeSymbolTimeFrame;
    [JsonIgnore]
    public TimeFrame TimeFrame => ExchangeSymbolTimeFrame.TimeFrame;

    #endregion

    #region Inputs

    [Signal(-1000)]
    //public SymbolValueAspect<TValue>? Bars { get; set; }
    [JsonIgnore]
    public HLCReference<TValue>? Bars { get; set; }

    #endregion

    #region Lifecycle

    public PBarsBot2()
    {
        ExchangeSymbolTimeFrame = null!; // REVIEW - make it nullable, but invalid if null
    }

    public PBarsBot2(ExchangeSymbolTimeFrame e)
    {
        ExchangeSymbolTimeFrame = e;
    }

    public virtual void FinalizeInit()
    {
        var pBotInfo = PBotInfos.Get(this.GetType());

        if (pBotInfo.Bars != null)
        {
            if (pBotInfo.Bars.PropertyType == typeof(SymbolValueAspect<double>))
            {
                pBotInfo.Bars.SetValue(this, SymbolValueAspect<double>.Create(ExchangeSymbolTimeFrame, DataPointAspect.HLC));
            }
            else if (pBotInfo.Bars.PropertyType == typeof(HLCReference<double>))
            {
                pBotInfo.Bars.SetValue(this, new HLCReference<double>(ExchangeSymbolTimeFrame));
            }
            else if (pBotInfo.Bars.PropertyType == typeof(OHLCReference<double>))
            {
                pBotInfo.Bars.SetValue(this, new OHLCReference<double>(ExchangeSymbolTimeFrame));
            }
            else { throw new NotImplementedException(); }
        }
    }

    #endregion
}

public interface IBarsBot<TValue>
{
    IReadOnlyValuesWindow<HLC<TValue>> Bars { get; set; }

    IPBarsBot2 Parameters { get; }
}

public class BarsBot2<TParameters, TValue> : SymbolBot2<TParameters, TValue>, IBarsBot<TValue>
      where TParameters : PBarsBot2<TParameters, TValue>
    where TValue : struct, INumber<TValue>
{
    IPBarsBot2 IBarsBot<TValue>.Parameters => Parameters;

    #region Injected

    [Signal(-1000)]
    public IReadOnlyValuesWindow<HLC<TValue>> Bars { get; set; } = null!;


    #endregion
}
