using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Relative Strength Index (RSI) indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - RSIQC: QuantConnect implementation (default, stable)
/// - RSIFP: First-party implementation (custom features)
/// - RSIOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IRSI<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for RSI calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The overbought threshold level (typically 70)
    /// </summary>
    TOutput OverboughtLevel { get; }
    
    /// <summary>
    /// The oversold threshold level (typically 30)
    /// </summary>
    TOutput OversoldLevel { get; }
    
    /// <summary>
    /// Current RSI value
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Indicates if the RSI is currently above the overbought level
    /// </summary>
    bool IsOverbought { get; }
    
    /// <summary>
    /// Indicates if the RSI is currently below the oversold level
    /// </summary>
    bool IsOversold { get; }
}