using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// ADX (Average Directional Index) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - ADX_QC: QuantConnect implementation (default, stable)
/// - ADX_FP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IADX<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for ADX calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current ADX value (0-100)
    /// Measures the strength of trend, regardless of direction
    /// </summary>
    TOutput ADX { get; }
    
    /// <summary>
    /// Current Plus Directional Indicator (+DI) value (0-100)
    /// Measures upward price movement
    /// </summary>
    TOutput PlusDI { get; }
    
    /// <summary>
    /// Current Minus Directional Indicator (-DI) value (0-100)
    /// Measures downward price movement
    /// </summary>
    TOutput MinusDI { get; }
}