using LionFire.Structures;

namespace LionFire.Trading.Automation;

public sealed class PSimAccount<TPrecision>
#if BacktestAccountSlottedParameters
    : SlottedParameters<BacktestAccount2<T>>
#else
    : IPMayHaveUnboundInputSlots
#endif
    , IParametersFor<SimAccount<TPrecision>>
    , ICloneable
    where TPrecision : struct, INumber<TPrecision>
{

    #region (static)

    //public static PSimAccount<TPrecision> Default { get; }
    public static PSimAccount<TPrecision> DefaultForBacktesting { get; }

    static PSimAccount()
    {
        //Default = new PSimAccount<TPrecision>(ExchangeSymbol.GenericUSD);
        //Default.DefaultHolding = PSimulatedHolding<TPrecision>.Default;

        DefaultForBacktesting = new PSimAccount<TPrecision>(ExchangeSymbol.GenericUSD)
        {
            DefaultHolding = PSimulatedHolding<TPrecision>.DefaultForBacktesting
        };
    }

    #endregion

    #region Identity

    public Type MaterializedType => typeof(SimAccount<TPrecision>);

    public ExchangeArea ExchangeArea { get; set; }

    #endregion

    #region Lifecycle

    public PSimAccount(ExchangeArea exchangeArea)
    {
        ExchangeArea = exchangeArea;
    }

    #region Misc

    public object Clone()
    {
        // REVIEW - deeper clone for Holdings?
        return this.MemberwiseClone();
    }

    #endregion

    #endregion

    #region Holdings

    public PSimulatedHolding<TPrecision>? DefaultHolding { get; init; }

    /// <summary>
    /// May not include DefaultHolding
    /// </summary>
    public Dictionary<string, PSimulatedHolding<TPrecision>>? Holdings { get; set; }

    #endregion

    #region Markets - REVIEW

    // TODO: Make Accounts dynamically support assets and positions, looking up the Bars at runtime.

    /// <summary>
    /// If null, populate from base.DefaultSymbol, if set
    /// </summary>
    [Obsolete]
    public HLCReference<TPrecision>? Bars { get; set; }

    #endregion

    #region REVIEW 

    /// <summary>
    /// The account will update its status once per bar/tick at a resolution of DefaultTimeFrame
    /// </summary>
    public TimeFrame? TimeFrame { get; set; }

    #endregion

    #region Inputs

    public IReadOnlyList<InputSlot> InputSlots => InputSlotsReflection.GetInputSlots(typeof(PSimAccount<TPrecision>));

    #endregion
}
