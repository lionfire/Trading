using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Fibonacci Retracement implementations
/// </summary>
public class PFibonacciRetracement<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"FibRetracement({LookbackPeriod},{(IncludeExtensionLevels ? "Ext" : "NoExt")})";

    #endregion

    #region Parameters

    /// <summary>
    /// The lookback period to find swing high and low points (default: 100)
    /// </summary>
    [Parameter(
        HardValueMin = 10,
        DefaultMin = 20,
        DefaultMax = 200,
        ValueMax = 500,
        HardValueMax = 65_536, // Arbitrary, can increase
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 5,
        MaxStep = 20,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 100)]
    public int LookbackPeriod { get; set; } = 100;

    /// <summary>
    /// Whether to include extension levels (161.8% and 261.8%) in calculations (default: true)
    /// </summary>
    [Parameter(
        OptimizePriority = -5,
        DefaultValue = true)]
    public bool IncludeExtensionLevels { get; set; } = true;

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
            Name = "HighLow",
            ValueType = typeof(TInput),
            Aspects = DataPointAspect.High | DataPointAspect.Low,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => LookbackPeriod;

    #endregion
}