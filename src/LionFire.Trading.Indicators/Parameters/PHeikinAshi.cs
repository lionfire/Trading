using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Heikin-Ashi implementations
/// </summary>
public class PHeikinAshi<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => "HeikinAshi";

    #endregion

    #region Parameters

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

    /// <summary>
    /// Doji threshold for determining if open and close are approximately equal
    /// Default: 0.001 (0.1% price difference)
    /// </summary>
    [Parameter(
        HardValueMin = 0.0001,
        DefaultMin = 0.0005,
        DefaultMax = 0.01,
        ValueMax = 0.05,
        HardValueMax = 0.1,
        DefaultValue = 0.001)]
    public double DojiThreshold { get; set; } = 0.001;

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
        => [new InputSlot()
        {
            Name = "OHLC",
            ValueType = typeof(TInput),
            Aspects = DataPointAspect.Open | DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    /// <summary>
    /// Heikin-Ashi requires at least one previous candle to calculate properly
    /// </summary>
    public int LookbackForInputSlot(InputSlot inputSlot) => 1;

    #endregion
}