using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all ROC implementations
/// </summary>
public class PROC<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ROC({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for ROC calculation (default: 10)
    /// </summary>
    [TradingParameter(
        HardValueMin = 1,
        DefaultMin = 2,
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
        DefaultValue = 10)]
    public int Period { get; set; } = 10;

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