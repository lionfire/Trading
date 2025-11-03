using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Pivot Points implementations
/// </summary>
public class PPivotPoints<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"PivotPoints({PeriodType})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period type for pivot calculation (default: Daily)
    /// </summary>
    [TradingParameter(
        OptimizePriority = -1,
        DefaultValue = PivotPointsPeriod.Daily)]
    public PivotPointsPeriod PeriodType { get; set; } = PivotPointsPeriod.Daily;

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
            Name = "OHLC",
            ValueType = typeof(TInput),
            Aspects = DataPointAspect.Open | DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    /// <summary>
    /// Pivot Points require at least 1 period to calculate, but need historical data for period aggregation
    /// </summary>
    public int LookbackForInputSlot(InputSlot inputSlot) => 1;

    #endregion
}