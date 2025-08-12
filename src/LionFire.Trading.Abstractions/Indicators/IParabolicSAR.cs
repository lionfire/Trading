using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Parabolic SAR (Stop and Reverse) indicator interface.
/// </summary>
/// <remarks>
/// The Parabolic SAR is a trend-following indicator that provides potential reversal points.
/// It uses an acceleration factor that increases as the trend develops, making the SAR 
/// converge on the price action over time.
/// 
/// The SAR value acts as a trailing stop loss - when price crosses the SAR, the trend 
/// reverses and the SAR switches sides (from below price in uptrend to above price in downtrend).
/// 
/// Available implementations:
/// - ParabolicSAR_QC: QuantConnect implementation (default, stable)
/// - ParabolicSAR_FP: First-party implementation (custom features)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IParabolicSAR<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The initial acceleration factor (typically 0.02)
    /// </summary>
    TOutput AccelerationFactor { get; }
    
    /// <summary>
    /// The maximum acceleration factor (typically 0.20)
    /// </summary>
    TOutput MaxAccelerationFactor { get; }
    
    /// <summary>
    /// Current SAR value
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Current trend direction (true for long/uptrend, false for short/downtrend)
    /// </summary>
    bool IsLong { get; }
    
    /// <summary>
    /// Indicates if the SAR has recently switched direction
    /// </summary>
    bool HasReversed { get; }
    
    /// <summary>
    /// Current acceleration factor being used in calculations
    /// </summary>
    TOutput CurrentAccelerationFactor { get; }
}