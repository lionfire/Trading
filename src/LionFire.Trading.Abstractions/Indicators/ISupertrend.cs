using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Supertrend indicator interface.
/// </summary>
/// <remarks>
/// The Supertrend indicator is a trend-following indicator that uses ATR (Average True Range) 
/// to create dynamic support and resistance levels.
/// 
/// Formula:
/// - Basic Upper Band = (HIGH + LOW) / 2 + Multiplier × ATR
/// - Basic Lower Band = (HIGH + LOW) / 2 - Multiplier × ATR
/// - Final bands stay flat or follow price based on trend direction
/// 
/// Available implementations:
/// - Supertrend_QC: QuantConnect implementation (default, stable)
/// - Supertrend_FP: First-party implementation (custom features, optimized for streaming)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface ISupertrend<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for ATR calculation
    /// </summary>
    int AtrPeriod { get; }
    
    /// <summary>
    /// The multiplier used for band calculation
    /// </summary>
    TOutput Multiplier { get; }
    
    /// <summary>
    /// Current Supertrend value (dynamic support/resistance line)
    /// </summary>
    TOutput Value { get; }
    
    /// <summary>
    /// Current trend direction (1 for uptrend, -1 for downtrend)
    /// </summary>
    int TrendDirection { get; }
    
    /// <summary>
    /// Indicates if the current trend is upward
    /// </summary>
    bool IsUptrend { get; }
    
    /// <summary>
    /// Indicates if the current trend is downward
    /// </summary>
    bool IsDowntrend { get; }
    
    /// <summary>
    /// Current ATR value used in the calculation
    /// </summary>
    TOutput CurrentATR { get; }
}