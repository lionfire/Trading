using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Volume Weighted Moving Average (VWMA) implementations
/// </summary>
public abstract class VWMABase<TConcrete, TInput, TOutput> : SingleInputIndicatorBase<TConcrete, PVWMA<TInput, TOutput>, TInput, TOutput>, 
    IVWMA<TInput, TOutput>
    where TConcrete : VWMABase<TConcrete, TInput, TOutput>, IIndicator2<TConcrete, PVWMA<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PVWMA<TInput, TOutput> Parameters;

    /// <summary>
    /// The period used for VWMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected VWMABase(PVWMA<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current VWMA value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets the sum of (price × volume) for the current period
    /// </summary>
    public abstract TOutput PriceVolumeSum { get; }

    /// <summary>
    /// Gets the sum of volumes for the current period
    /// </summary>
    public abstract TOutput VolumeSum { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the VWMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "VWMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the VWMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PVWMA<TInput, TOutput> p)
        => [new() {
                Name = "VWMA",
                ValueType = typeof(TOutput),
            }];

    #endregion
}