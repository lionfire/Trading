using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// On Balance Volume (OBV) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - OBV_QC: QuantConnect implementation (default, stable)
/// - OBV_FP: First-party implementation (custom features)
/// - OBVOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IOBV<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Current OBV value (cumulative volume)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Gets the trend direction based on the latest OBV change
    /// Positive indicates buying pressure, negative indicates selling pressure
    /// </summary>
    TOutput LastChange { get; }
    
    /// <summary>
    /// Gets a value indicating if OBV is trending upward (positive momentum)
    /// </summary>
    bool IsRising { get; }
    
    /// <summary>
    /// Gets a value indicating if OBV is trending downward (negative momentum)
    /// </summary>
    bool IsFalling { get; }
}