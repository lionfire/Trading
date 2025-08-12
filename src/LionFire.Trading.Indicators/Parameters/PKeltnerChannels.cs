using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Keltner Channels implementations
/// </summary>
public class PKeltnerChannels<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"KC({Period},{AtrPeriod},{AtrMultiplier})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for the EMA calculation (default: 20)
    /// </summary>
    [Parameter(
        HardValueMin = 2,
        DefaultMin = 10,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 65_536, // Arbitrary, can increase
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 10,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 20)]
    public int Period { get; set; } = 20;

    /// <summary>
    /// The period for the ATR calculation (default: 10)
    /// </summary>
    [Parameter(
        HardValueMin = 2,
        DefaultMin = 5,
        DefaultMax = 30,
        ValueMax = 50,
        HardValueMax = 65_536,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 10)]
    public int AtrPeriod { get; set; } = 10;

    /// <summary>
    /// The ATR multiplier for the channel bands (default: 2.0)
    /// </summary>
    [Parameter(
        HardValueMin = 0.5,
        DefaultMin = 1.0,
        DefaultMax = 4.0,
        HardValueMax = 10.0,
        Step = 0.1,
        OptimizePriority = -1,
        DefaultValue = 2.0)]
    public TOutput AtrMultiplier { get; set; } = TOutput.CreateChecked(2.0);

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
        => [new InputSlot()
        {
            Name = "Source",
            ValueType = typeof(TInput),
            Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Math.Max(Period, AtrPeriod);

    #endregion
}