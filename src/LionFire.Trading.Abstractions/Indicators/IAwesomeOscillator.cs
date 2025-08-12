using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Awesome Oscillator (AO) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - AwesomeOscillator_QC: QuantConnect implementation (default, stable)
/// - AwesomeOscillator_FP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// 
/// The Awesome Oscillator measures the difference between a 5-period and 34-period
/// simple moving average of the median price (High + Low) / 2.
/// AO = SMA(Median Price, 5) - SMA(Median Price, 34)
/// </remarks>
public interface IAwesomeOscillator<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The fast period for SMA calculation (typically 5)
    /// </summary>
    int FastPeriod { get; }
    
    /// <summary>
    /// The slow period for SMA calculation (typically 34)
    /// </summary>
    int SlowPeriod { get; }
    
    /// <summary>
    /// Current Awesome Oscillator value (Fast SMA - Slow SMA of median price)
    /// </summary>
    TOutput Value { get; }
}