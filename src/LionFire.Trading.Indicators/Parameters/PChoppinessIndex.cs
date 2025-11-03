using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading.Data;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Choppiness Index implementations
/// </summary>
public class PChoppinessIndex<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ChoppinessIndex({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for Choppiness Index calculation (default: 14)
    /// </summary>
    [TradingParameter(
        HardValueMin = 2,
        DefaultMin = 5,
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
        DefaultValue = 14)]
    public int Period { get; set; } = 14;

    /// <summary>
    /// Choppy/consolidating threshold (default: 61.8)
    /// Values above this level indicate choppy/sideways market conditions
    /// </summary>
    [TradingParameter(
        HardValueMin = 50,
        DefaultMin = 55,
        DefaultMax = 75,
        HardValueMax = 100,
        Step = 1.0,
        OptimizePriority = -1,
        DefaultValue = 61.8)]
    public TOutput ChoppyThreshold { get; set; } = TOutput.CreateChecked(61.8);

    /// <summary>
    /// Trending threshold (default: 38.2)
    /// Values below this level indicate trending market conditions
    /// </summary>
    [TradingParameter(
        HardValueMin = 0,
        DefaultMin = 25,
        DefaultMax = 45,
        HardValueMax = 50,
        Step = 1.0,
        OptimizePriority = -1,
        DefaultValue = 38.2)]
    public TOutput TrendingThreshold { get; set; } = TOutput.CreateChecked(38.2);

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [TradingParameter(OptimizePriority = -10)]
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

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}