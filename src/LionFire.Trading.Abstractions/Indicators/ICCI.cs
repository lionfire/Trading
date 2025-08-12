using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// CCI (Commodity Channel Index) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - CCI_QC: QuantConnect implementation (default, stable)
/// - CCI_FP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface ICCI<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for CCI calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current CCI oscillator value
    /// CCI = (Typical Price - SMA of Typical Price) / (0.015 * Mean Deviation)
    /// Typically ranges from -100 to +100, but can exceed these bounds
    /// </summary>
    TOutput Value { get; }
}