using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all CCI (Commodity Channel Index) implementations
/// </summary>
public class PCCI<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"CCI({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for CCI calculation (default: 20)
    /// </summary>
    [TradingParameter(
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
    /// The constant used in CCI calculation (default: 0.015)
    /// This is the traditional CCI constant that normalizes the oscillator
    /// </summary>
    [TradingParameter(
        HardValueMin = 0.001,
        DefaultMin = 0.01,
        DefaultMax = 0.05,
        HardValueMax = 1.0,
        Step = 0.001,
        OptimizePriority = -5,
        DefaultValue = 0.015)]
    public double Constant { get; set; } = 0.015;

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [TradingParameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

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