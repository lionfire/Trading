using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Hull Moving Average (HMA) implementations
/// </summary>
public abstract class HullMovingAverageBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PHullMovingAverage<TPrice, TOutput>, TPrice, TOutput>, 
    IHullMovingAverage<TPrice, TOutput>
    where TConcrete : HullMovingAverageBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PHullMovingAverage<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PHullMovingAverage<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Hull Moving Average calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The period for the first WMA calculation (Period/2)
    /// </summary>
    public int WMA1Period => Parameters.WMA1Period;

    /// <summary>
    /// The period for the second WMA calculation (Period)
    /// </summary>
    public int WMA2Period => Parameters.WMA2Period;

    /// <summary>
    /// The period for the final Hull WMA calculation (sqrt(Period))
    /// </summary>
    public int HullWMAPeriod => Parameters.HullWMAPeriod;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected HullMovingAverageBase(PHullMovingAverage<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current Hull Moving Average value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Hull Moving Average indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "HMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Hull Moving Average indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PHullMovingAverage<TPrice, TOutput> p)
        => [new() {
                Name = "HMA",
                ValueType = typeof(TOutput),
            }];

    #endregion
}