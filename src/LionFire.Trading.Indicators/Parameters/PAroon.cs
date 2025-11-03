using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Aroon implementations
/// </summary>
public class PAroon<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"Aroon({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for Aroon calculation (default: 14)
    /// Represents the number of periods to look back for highest high and lowest low
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
    /// Threshold for strong uptrend detection (default: 70)
    /// When Aroon Up is above this level and Aroon Down is below (100 - this level), indicates strong uptrend
    /// </summary>
    [TradingParameter(
        HardValueMin = 50,
        DefaultMin = 60,
        DefaultMax = 90,
        HardValueMax = 100,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 70)]
    public TOutput UptrendThreshold { get; set; } = TOutput.CreateChecked(70);

    /// <summary>
    /// Threshold for strong downtrend detection (default: 70)
    /// When Aroon Down is above this level and Aroon Up is below (100 - this level), indicates strong downtrend
    /// </summary>
    [TradingParameter(
        HardValueMin = 50,
        DefaultMin = 60,
        DefaultMax = 90,
        HardValueMax = 100,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 70)]
    public TOutput DowntrendThreshold { get; set; } = TOutput.CreateChecked(70);

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
            Aspects = DataPointAspect.High | DataPointAspect.Low,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}