using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Williams %R implementations
/// </summary>
public class PWilliamsR<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"WilliamsR({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for Williams %R calculation (default: 14)
    /// Represents the number of periods to look back for high/low range
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
    /// Overbought level threshold (default: -20)
    /// Williams %R values above this level indicate overbought conditions
    /// </summary>
    [TradingParameter(
        HardValueMin = -50,
        DefaultMin = -30,
        DefaultMax = -10,
        HardValueMax = 0,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = -20)]
    public TOutput OverboughtLevel { get; set; } = TOutput.CreateChecked(-20);

    /// <summary>
    /// Oversold level threshold (default: -80)
    /// Williams %R values below this level indicate oversold conditions
    /// </summary>
    [TradingParameter(
        HardValueMin = -100,
        DefaultMin = -90,
        DefaultMax = -70,
        HardValueMax = -50,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = -80)]
    public TOutput OversoldLevel { get; set; } = TOutput.CreateChecked(-80);

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

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Period;

    #endregion
}