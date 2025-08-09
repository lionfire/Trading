using LionFire.Structures;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Stochastic Oscillator implementations
/// </summary>
public class PStochastic<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"Stochastic({FastPeriod},{SlowKPeriod},{SlowDPeriod})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for %K calculation (default: 14)
    /// Represents the number of periods to look back for high/low range
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
        DefaultValue = 14)]
    public int FastPeriod { get; set; } = 14;

    /// <summary>
    /// The smoothing period for %K (default: 3)
    /// This creates the slow %K by smoothing the fast %K
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 1,
        DefaultMax = 10,
        ValueMax = 20,
        HardValueMax = 100,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizePriority = -1,
        DefaultValue = 3)]
    public int SlowKPeriod { get; set; } = 3;

    /// <summary>
    /// The period for %D signal line calculation (default: 3)
    /// %D is a moving average of the slow %K
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 1,
        DefaultMax = 10,
        ValueMax = 20,
        HardValueMax = 100,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizePriority = -1,
        DefaultValue = 3)]
    public int SlowDPeriod { get; set; } = 3;

    /// <summary>
    /// Overbought level threshold (default: 80)
    /// </summary>
    [Parameter(
        HardValueMin = 50,
        DefaultMin = 70,
        DefaultMax = 90,
        HardValueMax = 100,
        Step = 5,
        OptimizePriority = -2,
        DefaultValue = 80)]
    public TOutput OverboughtLevel { get; set; } = TOutput.CreateChecked(80);

    /// <summary>
    /// Oversold level threshold (default: 20)
    /// </summary>
    [Parameter(
        HardValueMin = 0,
        DefaultMin = 10,
        DefaultMax = 30,
        HardValueMax = 50,
        Step = 5,
        OptimizePriority = -2,
        DefaultValue = 20)]
    public TOutput OversoldLevel { get; set; } = TOutput.CreateChecked(20);

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

    public int LookbackForInputSlot(InputSlot inputSlot) => FastPeriod + SlowKPeriod + SlowDPeriod;

    #endregion
}