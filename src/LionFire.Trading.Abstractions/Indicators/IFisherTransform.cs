using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Fisher Transform indicator interface.
/// </summary>
/// <remarks>
/// The Fisher Transform converts prices into a Gaussian normal distribution
/// and produces an oscillator that can help identify turning points.
/// 
/// Available implementations:
/// - FisherTransform_QC: QuantConnect implementation (if available)
/// - FisherTransform_FP: First-party implementation (custom features, optimized with circular buffers)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IFisherTransform<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for normalization (default: 10)
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current Fisher Transform value
    /// </summary>
    TOutput Fisher { get; }
    
    /// <summary>
    /// Previous Fisher Transform value (trigger line for crossovers)
    /// </summary>
    TOutput Trigger { get; }
}