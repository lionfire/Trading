using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all VWAP implementations
/// </summary>
public class PVWAP<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"VWAP({ResetPeriod})";

    #endregion

    #region Parameters

    /// <summary>
    /// How often to reset the VWAP calculation (default: Daily)
    /// </summary>
    [TradingParameter(
        DefaultValue = VWAPResetPeriod.Daily,
        OptimizePriority = 5)]
    public VWAPResetPeriod ResetPeriod { get; set; } = VWAPResetPeriod.Daily;

    /// <summary>
    /// Custom reset time for when ResetPeriod is set to Custom
    /// Only used when ResetPeriod = Custom
    /// </summary>
    [TradingParameter(OptimizePriority = -5)]
    public TimeSpan? CustomResetTime { get; set; }

    /// <summary>
    /// Whether to use typical price (H+L+C)/3 instead of close price for calculation
    /// </summary>
    [TradingParameter(
        DefaultValue = true,
        OptimizePriority = 3)]
    public bool UseTypicalPrice { get; set; } = true;

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
            Name = "PriceVolume",
            ValueType = typeof(TInput),
            Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close | DataPointAspect.Volume,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => 1;

    #endregion
}