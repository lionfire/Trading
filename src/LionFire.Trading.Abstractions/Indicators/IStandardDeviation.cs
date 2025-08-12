using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Standard Deviation indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - StandardDeviation_QC: QuantConnect implementation (default, stable)
/// - StandardDeviation_FP: First-party implementation (custom features, optimized with Welford's algorithm)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IStandardDeviation<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for Standard Deviation calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current Standard Deviation value
    /// </summary>
    TOutput Value { get; }
}