using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Exponential Moving Average (EMA) implementations
/// </summary>
public abstract class EMABase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PEMA<TPrice, TOutput>, TPrice, TOutput>, 
    IEMA<TPrice, TOutput>
    where TConcrete : EMABase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PEMA<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PEMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for EMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The smoothing factor used in EMA calculation
    /// </summary>
    public abstract TOutput SmoothingFactor { get; }

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected EMABase(PEMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current EMA value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the EMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "EMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the EMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PEMA<TPrice, TOutput> p)
        => [new() {
                Name = "EMA",
                ValueType = typeof(TOutput),
            }];

    #endregion
}