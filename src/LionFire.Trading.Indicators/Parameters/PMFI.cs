using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Money Flow Index (MFI) implementations
/// </summary>
public class PMFI<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"MFI({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for MFI calculation (default: 14)
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
    /// Overbought level threshold (default: 80)
    /// </summary>
    [TradingParameter(
        HardValueMin = 50,
        DefaultMin = 70,
        DefaultMax = 90,
        HardValueMax = 100,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 80)]
    public TOutput OverboughtLevel { get; set; } = TOutput.CreateChecked(80);

    /// <summary>
    /// Oversold level threshold (default: 20)
    /// </summary>
    [TradingParameter(
        HardValueMin = 0,
        DefaultMin = 10,
        DefaultMax = 30,
        HardValueMax = 50,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 20)]
    public TOutput OversoldLevel { get; set; } = TOutput.CreateChecked(20);

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
            Name = "OHLCV",
            ValueType = typeof(TInput),
            Aspects = DataPointAspect.Open | DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close | DataPointAspect.Volume,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}