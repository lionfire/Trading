using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Volume Weighted Average Price (VWAP) implementations
/// </summary>
public abstract class VWAPBase<TConcrete, TInput, TOutput> : SingleInputIndicatorBase<TConcrete, PVWAP<TInput, TOutput>, TInput, TOutput>, 
    IVWAP<TInput, TOutput>
    where TConcrete : VWAPBase<TConcrete, TInput, TOutput>, IIndicator2<TConcrete, PVWAP<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PVWAP<TInput, TOutput> Parameters;

    /// <summary>
    /// The reset period for VWAP calculation
    /// </summary>
    public VWAPResetPeriod ResetPeriod => Parameters.ResetPeriod;

    /// <summary>
    /// Whether to use typical price (H+L+C)/3 instead of close price
    /// </summary>
    public bool UseTypicalPrice => Parameters.UseTypicalPrice;

    /// <summary>
    /// Custom reset time when using Custom reset period
    /// </summary>
    public TimeSpan? CustomResetTime => Parameters.CustomResetTime;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => 1;

    #endregion

    #region Lifecycle

    protected VWAPBase(PVWAP<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current VWAP value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets the cumulative typical price × volume sum for the current period
    /// </summary>
    public abstract TOutput CumulativePriceVolume { get; }

    /// <summary>
    /// Gets the cumulative volume for the current period
    /// </summary>
    public abstract TOutput CumulativeVolume { get; }

    /// <summary>
    /// Gets a value indicating whether the VWAP has been reset for the current period
    /// </summary>
    public abstract bool HasReset { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the VWAP indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "VWAP",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the VWAP indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PVWAP<TInput, TOutput> p)
        => [new() {
                Name = "VWAP",
                ValueType = typeof(TOutput),
            }];

    #endregion
}