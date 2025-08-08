using LionFire.Trading.Automation.Bots;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.ValueWindows;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

public interface IPBarsBot2 : IPRequiresInitBeforeUse
{
    ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; set; }

    //void InferMissingParameters();
}

public interface IPRequiresInitBeforeUse
{
    void Init();

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

    
    [JsonIgnore]
    public HLCReference<TValue>? HLCBars 
    { 
        get => hlcBars;
        set => hlcBars = value;
    }
    private HLCReference<TValue>? hlcBars;
    //public SymbolValueAspect<TValue>? Bars { get; set; }
    
    // Override abstract property from PMarketProcessor base class
    [JsonIgnore]
    public override IPInput Bars => hlcBars!;

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

    public void Init() => InferMissingParameters();

    /// <summary>
    /// Uses common parameters to populate missing parameters:
    /// - ExchangeSymbolTimeFrame -> Bars 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual void InferMissingParameters()
    {
        var pBotInfo = PBotInfos.Get(this.GetType());

        /// Set Bars from ExchangeSymbolTimeFrame
        if (Bars == null)
        {
            if (pBotInfo.Bars != null)
            {
                if (pBotInfo.Bars.PropertyType == typeof(SymbolValueAspect<TValue>))
                {
                    pBotInfo.Bars.SetValue(this, SymbolValueAspect<TValue>.Create(ExchangeSymbolTimeFrame, DataPointAspect.HLC));
                }
                else if (pBotInfo.Bars.PropertyType == typeof(HLCReference<TValue>))
                {
                    pBotInfo.Bars.SetValue(this, new HLCReference<TValue>(ExchangeSymbolTimeFrame));
                }
                else if (pBotInfo.Bars.PropertyType == typeof(OHLCReference<TValue>))
                {
                    pBotInfo.Bars.SetValue(this, new OHLCReference<TValue>(ExchangeSymbolTimeFrame));
                }
                else { throw new NotImplementedException(); }
            }
        }
    }

    #endregion
}

public interface IBarsBot<TValue>
{
    IReadOnlyValuesWindow<HLC<TValue>> Bars { get; set; }

    IPBarsBot2 Parameters { get; }
}
