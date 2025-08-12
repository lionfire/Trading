using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Klinger Oscillator indicator interface.
/// Measures volume trends and momentum using volume force calculations.
/// </summary>
/// <remarks>
/// Available implementations:
/// - KlingerOscillator_QC: QuantConnect implementation (if available)
/// - KlingerOscillator_FP: First-party implementation (custom features, optimized)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IKlingerOscillator<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The fast period for EMA calculation
    /// </summary>
    int FastPeriod { get; }
    
    /// <summary>
    /// The slow period for EMA calculation
    /// </summary>
    int SlowPeriod { get; }
    
    /// <summary>
    /// The signal period for EMA calculation of the Klinger line
    /// </summary>
    int SignalPeriod { get; }
    
    /// <summary>
    /// Current Klinger Oscillator value (Fast EMA - Slow EMA of Volume Force)
    /// </summary>
    TOutput Klinger { get; }
    
    /// <summary>
    /// Current Signal line value (EMA of Klinger line)
    /// </summary>
    TOutput Signal { get; }
    
    /// <summary>
    /// Current Volume Force value (used internally for calculation)
    /// </summary>
    TOutput VolumeForce { get; }
}