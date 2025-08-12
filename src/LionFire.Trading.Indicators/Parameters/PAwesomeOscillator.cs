using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Awesome Oscillator implementations
/// </summary>
public class PAwesomeOscillator<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"AwesomeOscillator({FastPeriod},{SlowPeriod})";

    #endregion

    #region Parameters

    /// <summary>
    /// The fast period for SMA calculation (default: 5)
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 2,
        DefaultMax = 15,
        ValueMax = 30,
        HardValueMax = 100,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 5)]
    public int FastPeriod { get; set; } = 5;

    /// <summary>
    /// The slow period for SMA calculation (default: 34)
    /// </summary>
    [Parameter(
        HardValueMin = 2,
        DefaultMin = 10,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 500,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 10,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 34)]
    public int SlowPeriod { get; set; } = 34;

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

    public int LookbackForInputSlot(InputSlot inputSlot) => SlowPeriod;

    #endregion

    #region Validation

    /// <summary>
    /// Validates that the parameters are logically consistent
    /// </summary>
    public void Validate()
    {
        if (FastPeriod >= SlowPeriod)
        {
            throw new ArgumentException("FastPeriod must be less than SlowPeriod for Awesome Oscillator calculation");
        }
        if (FastPeriod < 1 || SlowPeriod < 1)
        {
            throw new ArgumentException("All periods must be positive integers");
        }
    }

    #endregion
}