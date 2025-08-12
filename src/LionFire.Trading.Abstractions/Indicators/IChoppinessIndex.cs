using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Choppiness Index indicator interface.
/// The Choppiness Index measures market directionality versus choppiness on a scale of 0-100.
/// Higher values indicate choppy/sideways markets, while lower values indicate trending markets.
/// </summary>
/// <remarks>
/// Available implementations:
/// - ChoppinessIndex_FP: First-party implementation (default)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// 
/// Interpretation:
/// - Above 61.8: Market is choppy/consolidating
/// - Below 38.2: Market is trending
/// - 38.2-61.8: Transitional zone
/// </remarks>
public interface IChoppinessIndex<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for Choppiness Index calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The choppy threshold level (typically 61.8)
    /// Values above this indicate choppy/sideways market conditions
    /// </summary>
    TOutput ChoppyThreshold { get; }
    
    /// <summary>
    /// The trending threshold level (typically 38.2)
    /// Values below this indicate trending market conditions
    /// </summary>
    TOutput TrendingThreshold { get; }
    
    /// <summary>
    /// Current Choppiness Index value (0-100)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Indicates if the market is currently choppy/consolidating
    /// </summary>
    bool IsChoppy { get; }
    
    /// <summary>
    /// Indicates if the market is currently trending
    /// </summary>
    bool IsTrending { get; }

    /// <summary>
    /// Gets the sum of True Range over the current period
    /// </summary>
    TOutput TrueRangeSum { get; }

    /// <summary>
    /// Gets the maximum range (max high - min low) over the current period
    /// </summary>
    TOutput MaxRange { get; }
}