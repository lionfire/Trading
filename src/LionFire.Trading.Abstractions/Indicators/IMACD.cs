using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// MACD (Moving Average Convergence Divergence) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - MACD_QC: QuantConnect implementation (default, stable)
/// - MACD_FP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IMACD<TInput, TOutput> : IIndicator2
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
    /// The signal period for EMA calculation of the MACD line
    /// </summary>
    int SignalPeriod { get; }
    
    /// <summary>
    /// Current MACD line value (Fast EMA - Slow EMA)
    /// </summary>
    TOutput MACD { get; }
    
    /// <summary>
    /// Current Signal line value (EMA of MACD line)
    /// </summary>
    TOutput Signal { get; }
    
    /// <summary>
    /// Current Histogram value (MACD - Signal)
    /// </summary>
    TOutput Histogram { get; }
}