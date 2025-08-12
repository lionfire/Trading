using LionFire.Structures;
using LionFire.Trading.ValueWindows;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// Parameters for the Rate of Change (ROC) indicator.
/// ROC measures the percentage change between the current value and the value n periods ago.
/// Input: Price values (typically Close)
/// Output: ROC percentage value
/// </summary>
public class PROC<TPrice, TOutput> : IndicatorParameters<ROC_QC<TPrice, TOutput>, TPrice, TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ROC({Period})";

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
    /// The period over which to calculate the ROC. Default is 10.
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 2,
        DefaultValue = 10,
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
    public int Period { get; set; } = 10;

    #endregion

    #region Inputs

    /// <summary>
    /// The lookback period required for this indicator
    /// </summary>
    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}