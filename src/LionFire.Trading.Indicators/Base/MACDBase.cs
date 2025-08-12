using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for MACD (Moving Average Convergence Divergence) implementations
/// </summary>
public abstract class MACDBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PMACD<TPrice, TOutput>, TPrice, TOutput>, 
    IMACD<TPrice, TOutput>
    where TConcrete : MACDBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PMACD<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PMACD<TPrice, TOutput> Parameters;

    /// <summary>
    /// The fast period for EMA calculation
    /// </summary>
    public int FastPeriod => Parameters.FastPeriod;

    /// <summary>
    /// The slow period for EMA calculation
    /// </summary>
    public int SlowPeriod => Parameters.SlowPeriod;

    /// <summary>
    /// The signal period for EMA calculation of the MACD line
    /// </summary>
    public int SignalPeriod => Parameters.SignalPeriod;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.SlowPeriod + Parameters.SignalPeriod - 1;

    #endregion

    #region Lifecycle

    protected MACDBase(PMACD<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Parameters.Validate();
    }

    #endregion

    #region State

    /// <summary>
    /// Current MACD line value (Fast EMA - Slow EMA)
    /// </summary>
    public abstract TOutput MACD { get; }

    /// <summary>
    /// Current Signal line value (EMA of MACD line)
    /// </summary>
    public abstract TOutput Signal { get; }

    /// <summary>
    /// Current Histogram value (MACD - Signal)
    /// </summary>
    public abstract TOutput Histogram { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the MACD indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "MACD",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Histogram",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the MACD indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PMACD<TPrice, TOutput> p)
        => [
            new() {
                Name = "MACD",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Histogram",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion
}