using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading.Indicators.Native;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Chandelier Exit implementations
/// </summary>
public class PChandelierExit<TPrice, TOutput> : IndicatorParameters<ChandelierExit_FP<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"CE({Period},{AtrMultiplier})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for ATR calculation and highest/lowest tracking (default: 22)
    /// </summary>
    /// <remarks>
    /// 22 represents approximately one trading month. Common alternatives include
    /// 10 (two trading weeks) or 14 (standard ATR period).
    /// </remarks>
    [TradingParameter(
        HardValueMin = 2,
        DefaultMin = 10,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 65_536,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 10,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 22)]
    public int Period { get; set; } = 22;

    /// <summary>
    /// The multiplier for ATR bands calculation (default: 3.0)
    /// </summary>
    /// <remarks>
    /// Higher multipliers create wider stops (more room for price movement).
    /// Lower multipliers create tighter stops (closer exit points).
    /// </remarks>
    [TradingParameter(
        HardValueMin = 0.5,
        DefaultMin = 1.0,
        DefaultMax = 5.0,
        HardValueMax = 10.0,
        Step = 0.1,
        OptimizePriority = -1,
        DefaultValue = 3.0)]
    public TOutput AtrMultiplier { get; set; } = TOutput.CreateChecked(3.0);

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

    [JsonIgnore]
    public override IReadOnlyList<InputSlot> InputSlots => GetInputSlots();

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}
