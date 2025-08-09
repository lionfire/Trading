using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Exponential Moving Average (EMA) indicator interface.
/// </summary>
/// <remarks>
/// EMA gives more weight to recent prices using exponential smoothing.
/// Formula: EMA = (Price - Previous EMA) × Multiplier + Previous EMA
/// Where Multiplier = 2 / (Period + 1)
/// 
/// Available implementations:
/// - EMAQC: QuantConnect implementation (default, stable)
/// - EMAFP: First-party implementation (custom features, uses SMA for initial seed)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IEMA<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for EMA calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current EMA value
    /// </summary>
    TOutput Value { get; }
    
    /// <summary>
    /// The smoothing factor (multiplier) used in EMA calculation
    /// Default: 2 / (Period + 1)
    /// </summary>
    TOutput SmoothingFactor { get; }
}