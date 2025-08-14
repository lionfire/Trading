using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Hull Moving Average (HMA) implementations
/// </summary>
public class PHullMovingAverage<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"HMA({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for Hull Moving Average calculation (default: 14)
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 5,
        DefaultMax = 100,
        ValueMax = 200,
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
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the period for the first WMA calculation (Period/2)
    /// </summary>
    public int WMA1Period => Period / 2;

    /// <summary>
    /// Gets the period for the second WMA calculation (Period)
    /// </summary>
    public int WMA2Period => Period;

    /// <summary>
    /// Gets the period for the final Hull WMA calculation (sqrt(Period))
    /// </summary>
    public int HullWMAPeriod => (int)Math.Sqrt(Period);

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