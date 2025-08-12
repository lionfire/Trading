using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Donchian Channels implementations
/// </summary>
public class PDonchianChannels<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"DC({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for Donchian Channels calculation (default: 20)
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 5,
        DefaultMax = 100,
        ValueMax = 200,
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
            ValueType = typeof(HLC<TPrice>),
            Aspects = DataPointAspect.High | DataPointAspect.Low,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}