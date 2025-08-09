using LionFire.Structures;
using LionFire.Trading.ValueWindows;
using System.Text.Json.Serialization;
using MAT = QuantConnect.Indicators.MovingAverageType;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// Parameters for the Relative Strength Index (RSI) indicator.
/// RSI measures momentum and identifies overbought/oversold conditions.
/// Input: Price values (typically Close)
/// Output: RSI value between 0 and 100
/// </summary>
public class PRSI<TPrice, TOutput> : IndicatorParameters<RSIQC<TPrice, TOutput>, TPrice, TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"RSI({Period})";

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
      => [new InputSlot() {
                    Name = "Source",
                    ValueType = typeof(TOutput),
                    Aspects = DataPointAspect.Close,
                    DefaultSource = 0,
                }];

    #endregion

    #region Parameters

    /// <summary>
    /// The period over which to calculate the RSI. Default is 14.
    /// </summary>
    [Parameter(
        HardValueMin = 2,
        DefaultMin = 5,
        DefaultValue = 14,
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
        SearchLogarithmExponent = 2.0)]
    public int Period { get; set; } = 14;

    /// <summary>
    /// The type of moving average to use for smoothing. Default is Wilders.
    /// </summary>
    [Parameter(
        OptimizePriority = -3, 
        OptimizerHints = OptimizationDistributionKind.Category, 
        DefaultValue = MAT.Wilders,
        HardValueMin = 0,
        HardValueMax = 10,
        DefaultSearchSpaces = [
            (MAT[])[
                MAT.Simple,
                MAT.Exponential,
                MAT.Wilders,
            ],
            (MAT[])[
                MAT.Exponential,
                MAT.Wilders,
            ]
        ])]
    public MAT MovingAverageType { get; set; } = MAT.Wilders;

    /// <summary>
    /// The overbought threshold level. Default is 70.
    /// Signals potential selling opportunity when RSI exceeds this level.
    /// </summary>
    [Parameter(
        HardValueMin = 50,
        DefaultMin = 60,
        DefaultValue = 70,
        DefaultMax = 90,
        HardValueMax = 100,
        Step = 5,
        OptimizePriority = -1)]
    public decimal OverboughtLevel { get; set; } = 70;

    /// <summary>
    /// The oversold threshold level. Default is 30.
    /// Signals potential buying opportunity when RSI falls below this level.
    /// </summary>
    [Parameter(
        HardValueMin = 0,
        DefaultMin = 10,
        DefaultValue = 30,
        DefaultMax = 40,
        HardValueMax = 50,
        Step = 5,
        OptimizePriority = -1)]
    public decimal OversoldLevel { get; set; } = 30;

    #endregion

    #region Inputs

    /// <summary>
    /// The lookback period required for this indicator
    /// </summary>
    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}