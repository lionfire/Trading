using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Aroon indicator interface.
/// The Aroon indicator helps identify trend changes and the strength of trends.
/// It consists of Aroon Up and Aroon Down lines which oscillate between 0 and 100.
/// </summary>
/// <remarks>
/// Available implementations:
/// - Aroon_QC: QuantConnect implementation (wrapping AroonOscillator)
/// - Aroon_FP: First-party implementation (optimized for streaming updates)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// 
/// The Aroon indicator uses the following formulas:
/// - Aroon Up = ((Period - Periods since highest high) / Period) × 100
/// - Aroon Down = ((Period - Periods since lowest low) / Period) × 100
/// - Aroon Oscillator = Aroon Up - Aroon Down (range: -100 to +100)
/// </remarks>
public interface IAroon<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for Aroon calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current Aroon Up value (0-100)
    /// Measures the strength of the upward trend
    /// </summary>
    TOutput AroonUp { get; }
    
    /// <summary>
    /// Current Aroon Down value (0-100)  
    /// Measures the strength of the downward trend
    /// </summary>
    TOutput AroonDown { get; }
    
    /// <summary>
    /// Current Aroon Oscillator value (-100 to +100)
    /// Calculated as Aroon Up - Aroon Down
    /// </summary>
    TOutput AroonOscillator { get; }
    
    /// <summary>
    /// Indicates if the market is in a strong uptrend (Aroon Up > 70 and Aroon Down < 30)
    /// </summary>
    bool IsUptrend { get; }
    
    /// <summary>
    /// Indicates if the market is in a strong downtrend (Aroon Down > 70 and Aroon Up < 30)
    /// </summary>
    bool IsDowntrend { get; }
    
    /// <summary>
    /// Indicates if the market is consolidating (both Aroon Up and Down are between 30-70)
    /// </summary>
    bool IsConsolidating { get; }
}