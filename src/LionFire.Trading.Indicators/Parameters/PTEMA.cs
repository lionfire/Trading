using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all TEMA implementations
/// </summary>
public class PTEMA<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"TEMA({Period})";

    #endregion

    #region Parameters

    /// <summary>
    /// The period for TEMA calculation (default: 14)
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 5,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 1000,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 14)]
    public int Period { get; set; } = 14;

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

    public int LookbackForInputSlot(InputSlot inputSlot) => Period * 3; // Three levels of EMA need warm-up

    #endregion
}