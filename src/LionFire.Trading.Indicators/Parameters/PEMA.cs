using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all EMA implementations
/// </summary>
public class PEMA<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"EMA({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for EMA calculation (default: 20)
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
        DefaultValue = 20)]
    public int Period { get; set; } = 20;

    /// <summary>
    /// Optional smoothing factor override. 
    /// If not specified (null or zero), will be calculated as 2/(Period+1)
    /// </summary>
    [Parameter(OptimizePriority = -5)]
    public TOutput? SmoothingFactor { get; set; }

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the effective smoothing factor, either the specified value or calculated from period
    /// </summary>
    public TOutput GetEffectiveSmoothingFactor()
    {
        if (SmoothingFactor.HasValue && !SmoothingFactor.Value.Equals(TOutput.Zero))
        {
            return SmoothingFactor.Value;
        }
        
        // Calculate default: 2 / (Period + 1)
        var two = TOutput.CreateChecked(2);
        var periodPlusOne = TOutput.CreateChecked(Period + 1);
        return two / periodPlusOne;
    }

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