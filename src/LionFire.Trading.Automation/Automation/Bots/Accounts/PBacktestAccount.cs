//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
using LionFire.Structures;

namespace LionFire.Trading.Automation;

public class PBacktestAccount<TPrecision>
#if BacktestAccountSlottedParameters
    : SlottedParameters<BacktestAccount2<T>>
    , IPTimeFrameMarketProcessor
#else
    : PAccount2<TPrecision>
    , IPMayHaveUnboundInputSlots
#endif
    , IPTimeFrameMarketProcessor
    , IParametersFor<BacktestAccount2<TPrecision>>
    , ICloneable
    where TPrecision : struct, INumber<TPrecision>
{

    #region (static)

    public static PBacktestAccount<TPrecision> Default { get; }

    static PBacktestAccount()
    {
        if (typeof(TPrecision) == typeof(double))
        {
            Default = (PBacktestAccount<TPrecision>)(object)new PBacktestAccount<double>(10_000.0)
            {
                //StartingBalance = 
            };
        }
        else if (typeof(TPrecision) == typeof(decimal))
        {
            Default = (PBacktestAccount<TPrecision>)(object)new PBacktestAccount<decimal>(10_000m)
            {
                //StartingBalance = 
            };
        }
        else
        {
            Default = (PBacktestAccount<TPrecision>)Activator.CreateInstance(typeof(PBacktestAccount<TPrecision>), [default(TPrecision)])!;
        }
    }

    #endregion

    #region Lifecycle

    //public PBacktestAccount() { }
    public PBacktestAccount(TPrecision startingBalance)
    {
        StartingBalance = startingBalance;
    }

    #endregion

#if BacktestAccountSlottedParameters
    // Get slots using: InputSlotsReflection.GetInputSlots(this.GetType());
    //[Slot(0)]
#endif
    /// <summary>
    /// If null, populate from base.DefaultSymbol, if set
    /// </summary>
    public HLCReference<TPrecision>? Bars { get; set; }


    public TimeFrame TimeFrame { get; set; }

    public int[]? InputLookbacks => [1];
    public Type MaterializedType => typeof(BacktestAccount2<double>);

    public IReadOnlyList<InputSlot> InputSlots => InputSlotsReflection.GetInputSlots(typeof(PBacktestAccount<TPrecision>));

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
