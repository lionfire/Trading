using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Triple Exponential Moving Average (TEMA) indicator interface.
/// </summary>
/// <remarks>
/// TEMA is a smoothed moving average that reduces lag compared to traditional EMAs.
/// It applies exponential smoothing three times and combines the results to create
/// a more responsive indicator with reduced noise.
/// 
/// Formula: TEMA = 3 × EMA1 - 3 × EMA2 + EMA3
/// Where:
/// - EMA1 = EMA(Price, Period)
/// - EMA2 = EMA(EMA1, Period) 
/// - EMA3 = EMA(EMA2, Period)
/// 
/// Available implementations:
/// - TEMA_QC: QuantConnect implementation (default, stable)
/// - TEMA_FP: First-party implementation (custom features, efficient cascading EMAs)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface ITEMA<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for TEMA calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current TEMA value
    /// </summary>
    TOutput Value { get; }
    
    /// <summary>
    /// The smoothing factor used in the underlying EMA calculations
    /// Default: 2 / (Period + 1)
    /// </summary>
    TOutput SmoothingFactor { get; }
    
    /// <summary>
    /// First EMA value (EMA of input prices)
    /// </summary>
    TOutput EMA1 { get; }
    
    /// <summary>
    /// Second EMA value (EMA of EMA1)
    /// </summary>
    TOutput EMA2 { get; }
    
    /// <summary>
    /// Third EMA value (EMA of EMA2)
    /// </summary>
    TOutput EMA3 { get; }
}