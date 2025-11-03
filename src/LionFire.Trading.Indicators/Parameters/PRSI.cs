using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all RSI implementations
/// </summary>
public class PRSI<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"RSI({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for RSI calculation (default: 14)
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
    /// Overbought level threshold (default: 70)
    /// </summary>
    [TradingParameter(
        HardValueMin = 50,
        DefaultMin = 60,
        DefaultMax = 90,
        HardValueMax = 100,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 70)]
    public TOutput OverboughtLevel { get; set; } = TOutput.CreateChecked(70);

    /// <summary>
    /// Oversold level threshold (default: 30)
    /// </summary>
    [TradingParameter(
        HardValueMin = 0,
        DefaultMin = 10,
        DefaultMax = 40,
        HardValueMax = 50,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 30)]
    public TOutput OversoldLevel { get; set; } = TOutput.CreateChecked(30);

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
            ValueType = typeof(TPrice),
            Aspects = DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}

