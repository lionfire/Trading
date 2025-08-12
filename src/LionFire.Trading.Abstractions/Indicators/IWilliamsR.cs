using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Williams %R indicator interface.
/// </summary>
/// <remarks>
/// Williams %R is a momentum indicator that measures overbought and oversold levels.
/// It compares the closing price to the high-low range over a specific period.
/// The indicator oscillates between -100 and 0, with readings from -80 to -100 indicating
/// oversold conditions, and readings from -20 to 0 indicating overbought conditions.
/// 
/// Formula: %R = ((Highest High - Close) / (Highest High - Lowest Low)) * -100
/// 
/// Available implementations:
/// - WilliamsR_QC: QuantConnect implementation (default, stable)
/// - WilliamsR_FP: First-party implementation (custom features)
/// - WilliamsROpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IWilliamsR<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for Williams %R calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The overbought threshold level (typically -20)
    /// Values above this level indicate overbought conditions
    /// </summary>
    TOutput OverboughtLevel { get; }
    
    /// <summary>
    /// The oversold threshold level (typically -80)
    /// Values below this level indicate oversold conditions
    /// </summary>
    TOutput OversoldLevel { get; }
    
    /// <summary>
    /// Current Williams %R value (ranges from -100 to 0)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Indicates if Williams %R is currently above the overbought level
    /// </summary>
    bool IsOverbought { get; }
    
    /// <summary>
    /// Indicates if Williams %R is currently below the oversold level
    /// </summary>
    bool IsOversold { get; }
}