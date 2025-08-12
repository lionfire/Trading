using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Volume Weighted Moving Average (VWMA) indicator interface.
/// </summary>
/// <remarks>
/// VWMA = Σ(Price × Volume) / Σ(Volume) over the specified period
/// 
/// Available implementations:
/// - VWMA_QC: QuantConnect implementation (default, stable)
/// - VWMA_FP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IVWMA<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for VWMA calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current VWMA value (volume-weighted moving average)
    /// </summary>
    TOutput Value { get; }
    
    /// <summary>
    /// Gets the sum of (price × volume) for the current period
    /// </summary>
    TOutput PriceVolumeSum { get; }
    
    /// <summary>
    /// Gets the sum of volumes for the current period
    /// </summary>
    TOutput VolumeSum { get; }
}