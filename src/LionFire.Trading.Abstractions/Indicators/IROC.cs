using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Rate of Change (ROC) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - ROC_QC: QuantConnect implementation (default, stable)
/// - ROC_FP: First-party implementation (custom features)
/// - ROCOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IROC<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for ROC calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current ROC value (percentage change)
    /// </summary>
    TOutput CurrentValue { get; }
}