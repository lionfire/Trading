using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Awesome Oscillator (AO) implementations
/// </summary>
public abstract class AwesomeOscillatorBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PAwesomeOscillator<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    IAwesomeOscillator<HLC<TPrice>, TOutput>
    where TConcrete : AwesomeOscillatorBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PAwesomeOscillator<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PAwesomeOscillator<TPrice, TOutput> Parameters;

    /// <summary>
    /// The fast period for SMA calculation
    /// </summary>
    public int FastPeriod => Parameters.FastPeriod;

    /// <summary>
    /// The slow period for SMA calculation
    /// </summary>
    public int SlowPeriod => Parameters.SlowPeriod;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.SlowPeriod;

    #endregion

    #region Lifecycle

    protected AwesomeOscillatorBase(PAwesomeOscillator<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Parameters.Validate();
    }

    #endregion

    #region State

    /// <summary>
    /// Current Awesome Oscillator value (Fast SMA - Slow SMA of median price)
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Awesome Oscillator indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "AO",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Awesome Oscillator indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PAwesomeOscillator<TPrice, TOutput> p)
        => [new() {
                Name = "AO",
                ValueType = typeof(TOutput),
            }];

    #endregion
}