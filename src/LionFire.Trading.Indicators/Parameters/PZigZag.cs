using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all ZigZag implementations
/// </summary>
public class PZigZag<TPrice, TOutput> : IndicatorParameters<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"ZigZag({Deviation:F1}%,{Depth})";

    #endregion

    #region Parameters

    /// <summary>
    /// The minimum percentage deviation required to form a new pivot point (default: 5.0%)
    /// </summary>
    [Parameter(
        HardValueMin = 0.1,
        DefaultMin = 1.0,
        DefaultMax = 20.0,
        ValueMax = 50.0,
        HardValueMax = 100.0,
        DefaultExponent = 1.0,
        MinExponent = 1.0,
        MaxExponent = 2.0,
        MinStep = 0.1,
        Step = 0.5,
        MaxStep = 2.0,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 1.5,
        DefaultValue = 5.0)]
    public TOutput Deviation { get; set; } = TOutput.CreateChecked(5.0);

    /// <summary>
    /// The minimum number of bars required between pivot points (default: 12)
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 3,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 1000,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 2.5,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 12)]
    public int Depth { get; set; } = 12;

    /// <summary>
    /// Maximum number of recent pivot points to retain in memory (default: 100)
    /// </summary>
    [Parameter(
        HardValueMin = 10,
        DefaultMin = 50,
        DefaultMax = 500,
        HardValueMax = 2000,
        Step = 10,
        OptimizePriority = -2,
        DefaultValue = 100)]
    public int MaxPivotHistory { get; set; } = 100;

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [Parameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
        => [new InputSlot()
        {
            Name = "HLC",
            ValueType = typeof(TPrice),
            Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Depth;

    #endregion

    #region Validation

    /// <summary>
    /// Validates the ZigZag parameters
    /// </summary>
    public void Validate()
    {
        if (Depth < 1)
            throw new ArgumentException("ZigZag depth must be at least 1", nameof(Depth));
            
        var zero = TOutput.Zero;
        var hundred = TOutput.CreateChecked(100);
        
        if (Deviation <= zero)
            throw new ArgumentException("ZigZag deviation must be greater than 0", nameof(Deviation));
            
        if (Deviation >= hundred)
            throw new ArgumentException("ZigZag deviation must be less than 100%", nameof(Deviation));
            
        if (MaxPivotHistory < 10)
            throw new ArgumentException("MaxPivotHistory must be at least 10", nameof(MaxPivotHistory));
    }

    #endregion
}