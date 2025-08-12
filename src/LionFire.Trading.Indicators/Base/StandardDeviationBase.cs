using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Standard Deviation implementations
/// </summary>
public abstract class StandardDeviationBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PStandardDeviation<TPrice, TOutput>, TPrice, TOutput>, 
    IStandardDeviation<TPrice, TOutput>
    where TConcrete : StandardDeviationBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PStandardDeviation<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PStandardDeviation<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Standard Deviation calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected StandardDeviationBase(PStandardDeviation<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current Standard Deviation value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    /// <summary>
    /// Missing output value for when the indicator is not ready
    /// </summary>
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Standard Deviation indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "StdDev",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Standard Deviation indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PStandardDeviation<TPrice, TOutput> p)
        => [new() {
                Name = "StdDev",
                ValueType = typeof(TOutput),
            }];

    #endregion
}