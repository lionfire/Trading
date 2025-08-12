using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Supertrend implementations
/// </summary>
public class PSupertrend<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"Supertrend({AtrPeriod},{Multiplier})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for ATR calculation (default: 10)
    /// </summary>
    [Parameter(
        HardValueMin = 2,
        DefaultMin = 5,
        DefaultMax = 30,
        ValueMax = 50,
        HardValueMax = 65_536, // Arbitrary, can increase
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 10,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 10)]
    public int AtrPeriod { get; set; } = 10;

    /// <summary>
    /// The multiplier for ATR bands calculation (default: 3.0)
    /// </summary>
    [Parameter(
        HardValueMin = 0.5,
        DefaultMin = 1.0,
        DefaultMax = 5.0,
        HardValueMax = 10.0,
        Step = 0.1,
        OptimizePriority = -1,
        DefaultValue = 3.0)]
    public TOutput Multiplier { get; set; } = TOutput.CreateChecked(3.0);

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
    public ImplementationHint PreferredImplementation { get; set; } = ImplementationHint.Auto;

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
        => [new InputSlot()
        {
            Name = "Source",
            ValueType = typeof(HLC<TPrice>),
            Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => AtrPeriod;

    #endregion
}