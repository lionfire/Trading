using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all ADX (Average Directional Index) implementations
/// </summary>
public class PADX<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ADX({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for ADX calculation (default: 14)
    /// Used for smoothing True Range, +DM, -DM, and calculating ADX from DX values
    /// </summary>
    [Parameter(
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
    /// Strong trend threshold for ADX (default: 25)
    /// Values above this level typically indicate a strong trend
    /// </summary>
    [Parameter(
        HardValueMin = 10,
        DefaultMin = 15,
        DefaultMax = 35,
        HardValueMax = 50,
        Step = 5,
        OptimizePriority = -1,
        DefaultValue = 25)]
    public TOutput StrongTrendThreshold { get; set; } = TOutput.CreateChecked(25);

    /// <summary>
    /// Very strong trend threshold for ADX (default: 50)
    /// Values above this level indicate a very strong trend
    /// </summary>
    [Parameter(
        HardValueMin = 30,
        DefaultMin = 40,
        DefaultMax = 70,
        HardValueMax = 80,
        Step = 5,
        OptimizePriority = -2,
        DefaultValue = 50)]
    public TOutput VeryStrongTrendThreshold { get; set; } = TOutput.CreateChecked(50);

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
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

    public int LookbackForInputSlot(InputSlot inputSlot) => Period * 2; // Need extra period for initial smoothing

    #endregion
}