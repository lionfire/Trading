using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Defines how often the VWAP indicator resets its cumulative calculations
/// </summary>
public enum VWAPResetPeriod
{
    /// <summary>
    /// Never reset - running cumulative VWAP from inception
    /// </summary>
    Never = 0,
    
    /// <summary>
    /// Reset daily at market open (typical default)
    /// </summary>
    Daily = 1,
    
    /// <summary>
    /// Reset weekly on Monday market open
    /// </summary>
    Weekly = 2,
    
    /// <summary>
    /// Reset monthly on first trading day
    /// </summary>
    Monthly = 3,
    
    /// <summary>
    /// Reset at specified time intervals (custom)
    /// </summary>
    Custom = 4
}

/// <summary>
/// Volume Weighted Average Price (VWAP) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - VWAP_QC: QuantConnect implementation (default, stable)
/// - VWAP_FP: First-party implementation (custom features, optimized cumulative calculation)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IVWAP<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Current VWAP value (volume-weighted average price)
    /// </summary>
    TOutput Value { get; }
    
    /// <summary>
    /// Gets the cumulative typical price × volume sum for the current period
    /// </summary>
    TOutput CumulativePriceVolume { get; }
    
    /// <summary>
    /// Gets the cumulative volume for the current period
    /// </summary>
    TOutput CumulativeVolume { get; }
    
    /// <summary>
    /// Gets the reset period type for VWAP calculation
    /// </summary>
    VWAPResetPeriod ResetPeriod { get; }
    
    /// <summary>
    /// Gets a value indicating whether the VWAP has been reset for the current period
    /// </summary>
    bool HasReset { get; }
}