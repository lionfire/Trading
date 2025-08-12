using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Triple Exponential Moving Average (TEMA) implementations
/// </summary>
public abstract class TEMABase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PTEMA<TPrice, TOutput>, TPrice, TOutput>, 
    ITEMA<TPrice, TOutput>
    where TConcrete : TEMABase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PTEMA<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PTEMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for TEMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The smoothing factor used in underlying EMA calculations
    /// </summary>
    public abstract TOutput SmoothingFactor { get; }

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period * 3; // Three levels of EMA need warm-up

    #endregion

    #region Lifecycle

    protected TEMABase(PTEMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current TEMA value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// First EMA value (EMA of input prices)
    /// </summary>
    public abstract TOutput EMA1 { get; }

    /// <summary>
    /// Second EMA value (EMA of EMA1)
    /// </summary>
    public abstract TOutput EMA2 { get; }

    /// <summary>
    /// Third EMA value (EMA of EMA2)
    /// </summary>
    public abstract TOutput EMA3 { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the TEMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "TEMA",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA1",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA2",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA3",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the TEMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PTEMA<TPrice, TOutput> p)
        => [new() {
                Name = "TEMA",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA1",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA2",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA3",
                ValueType = typeof(TOutput),
            }];

    #endregion
}