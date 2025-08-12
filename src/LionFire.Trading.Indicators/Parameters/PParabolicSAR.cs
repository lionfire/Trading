using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Parabolic SAR implementations
/// </summary>
public class PParabolicSAR<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ParabolicSAR({AccelerationFactor},{MaxAccelerationFactor})";

    #endregion

    #region Parameters

    /// <summary>
    /// The initial acceleration factor (default: 0.02)
    /// Controls how quickly the SAR moves towards the price
    /// </summary>
    [Parameter(
        HardValueMin = 0.001,
        DefaultMin = 0.01,
        DefaultMax = 0.05,
        ValueMax = 0.10,
        HardValueMax = 0.50,
        Step = 0.001,
        OptimizePriority = 1,
        DefaultValue = 0.02)]
    public TOutput AccelerationFactor { get; set; } = TOutput.CreateChecked(0.02);

    /// <summary>
    /// The maximum acceleration factor (default: 0.20)
    /// The acceleration factor will not exceed this value
    /// </summary>
    [Parameter(
        HardValueMin = 0.05,
        DefaultMin = 0.10,
        DefaultMax = 0.50,
        ValueMax = 1.0,
        HardValueMax = 2.0,
        Step = 0.01,
        OptimizePriority = 0,
        DefaultValue = 0.20)]
    public TOutput MaxAccelerationFactor { get; set; } = TOutput.CreateChecked(0.20);

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
            Aspects = DataPointAspect.High | DataPointAspect.Low,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => 2; // Parabolic SAR needs at least 2 periods to establish initial trend

    #endregion

    #region Validation

    /// <summary>
    /// Validates that the parameters are logically consistent
    /// </summary>
    public void Validate()
    {
        if (AccelerationFactor <= TOutput.Zero)
        {
            throw new ArgumentException("AccelerationFactor must be greater than zero");
        }
        if (MaxAccelerationFactor <= AccelerationFactor)
        {
            throw new ArgumentException("MaxAccelerationFactor must be greater than AccelerationFactor");
        }
        
        var one = TOutput.CreateChecked(1.0);
        if (MaxAccelerationFactor > one)
        {
            throw new ArgumentException("MaxAccelerationFactor should typically not exceed 1.0");
        }
    }

    #endregion
}