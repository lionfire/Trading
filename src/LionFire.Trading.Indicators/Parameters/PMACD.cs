using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all MACD implementations
/// </summary>
public class PMACD<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"MACD({FastPeriod},{SlowPeriod},{SignalPeriod})";

    #endregion

    #region Parameters

    /// <summary>
    /// The fast period for EMA calculation (default: 12)
    /// </summary>
    [TradingParameter(
        HardValueMin = 1,
        DefaultMin = 3,
        DefaultMax = 30,
        ValueMax = 50,
        HardValueMax = 200,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 12)]
    public int FastPeriod { get; set; } = 12;

    /// <summary>
    /// The slow period for EMA calculation (default: 26)
    /// </summary>
    [TradingParameter(
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
        DefaultValue = 26)]
    public int SlowPeriod { get; set; } = 26;

    /// <summary>
    /// The signal period for EMA calculation of the MACD line (default: 9)
    /// </summary>
    [TradingParameter(
        HardValueMin = 1,
        DefaultMin = 3,
        DefaultMax = 30,
        ValueMax = 50,
        HardValueMax = 200,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 9)]
    public int SignalPeriod { get; set; } = 9;

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
            Name = "Source",
            ValueType = typeof(TPrice),
            Aspects = DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => SlowPeriod + SignalPeriod - 1;

    #endregion

    #region Validation

    /// <summary>
    /// Validates that the parameters are logically consistent
    /// </summary>
    public void Validate()
    {
        if (FastPeriod >= SlowPeriod)
        {
            throw new ArgumentException("FastPeriod must be less than SlowPeriod for MACD calculation");
        }
        if (FastPeriod < 1 || SlowPeriod < 1 || SignalPeriod < 1)
        {
            throw new ArgumentException("All periods must be positive integers");
        }
    }

    #endregion
}