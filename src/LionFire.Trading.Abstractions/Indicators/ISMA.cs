using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Simple Moving Average (SMA) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - SMAQC: QuantConnect implementation (default, stable)
/// - SMAFP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface ISMA<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for SMA calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current SMA value
    /// </summary>
    TOutput Value { get; }
}