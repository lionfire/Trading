using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Ichimoku Cloud implementations
/// </summary>
public class PIchimokuCloud<TPrice, TOutput> : IndicatorParameters<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"IchimokuCloud({ConversionLinePeriod},{BaseLinePeriod},{LeadingSpanBPeriod},{Displacement})";

    #endregion

    #region Parameters

    /// <summary>
    /// Conversion Line Period (Tenkan-sen) - default: 9
    /// </summary>
    [Parameter(
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
    public int ConversionLinePeriod { get; set; } = 9;

    /// <summary>
    /// Base Line Period (Kijun-sen) - default: 26
    /// </summary>
    [Parameter(
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
    public int BaseLinePeriod { get; set; } = 26;

    /// <summary>
    /// Leading Span B Period (Senkou Span B) - default: 52
    /// </summary>
    [Parameter(
        HardValueMin = 3,
        DefaultMin = 20,
        DefaultMax = 100,
        ValueMax = 200,
        HardValueMax = 1000,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 20,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 52)]
    public int LeadingSpanBPeriod { get; set; } = 52;

    /// <summary>
    /// Displacement - periods ahead for leading spans and behind for lagging span (default: 26)
    /// </summary>
    [Parameter(
        HardValueMin = 1,
        DefaultMin = 5,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 200,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 3.0,
        MinStep = 1,
        Step = 1,
        MaxStep = 10,
        OptimizerHints = OptimizationDistributionKind.Period,
        SearchLogarithmExponent = 2.0,
        DefaultValue = 26)]
    public int Displacement { get; set; } = 26;

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
            Name = "Source",
            ValueType = typeof(HLC<TPrice>),
            Aspects = DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Math.Max(LeadingSpanBPeriod, BaseLinePeriod) + Displacement;

    #endregion

    #region Validation

    /// <summary>
    /// Validates that the parameters are logically consistent
    /// </summary>
    public void Validate()
    {
        if (ConversionLinePeriod < 1 || BaseLinePeriod < 1 || LeadingSpanBPeriod < 1 || Displacement < 1)
        {
            throw new ArgumentException("All periods and displacement must be positive integers");
        }
        
        // Traditional Ichimoku relationships: ConversionLine < BaseLine < LeadingSpanB
        if (ConversionLinePeriod >= BaseLinePeriod)
        {
            throw new ArgumentException("ConversionLinePeriod should typically be less than BaseLinePeriod for traditional Ichimoku analysis");
        }
        
        if (BaseLinePeriod >= LeadingSpanBPeriod)
        {
            throw new ArgumentException("BaseLinePeriod should typically be less than LeadingSpanBPeriod for traditional Ichimoku analysis");
        }
    }

    #endregion
}