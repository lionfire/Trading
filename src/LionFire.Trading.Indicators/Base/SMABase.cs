using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Simple Moving Average (SMA) implementations
/// </summary>
public abstract class SMABase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PSMA<TPrice, TOutput>, TPrice, TOutput>, 
    ISMA<TPrice, TOutput>
    where TConcrete : SMABase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PSMA<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PSMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for SMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected SMABase(PSMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current SMA value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the SMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "SMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the SMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PSMA<TPrice, TOutput> p)
        => [new() {
                Name = "SMA",
                ValueType = typeof(TOutput),
            }];

    #endregion
}