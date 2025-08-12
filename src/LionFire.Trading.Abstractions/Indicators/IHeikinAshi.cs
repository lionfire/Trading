using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Heikin-Ashi (Average Bar) indicator interface.
/// Transforms regular candlesticks to Heikin-Ashi candles for smoother trend visualization.
/// </summary>
/// <remarks>
/// Available implementations:
/// - HeikinAshi_FP: First-party implementation (optimized for streaming updates)
/// 
/// The Heikin-Ashi technique creates "average bars" by smoothing price data:
/// - HA_Close = (Open + High + Low + Close) / 4
/// - HA_Open = (Previous HA_Open + Previous HA_Close) / 2
/// - HA_High = Max(High, HA_Open, HA_Close)
/// - HA_Low = Min(Low, HA_Open, HA_Close)
/// 
/// Benefits:
/// - Smooths price action for clearer trend identification
/// - Reduces noise and false signals
/// - Provides visual trend strength indicators through candle colors
/// </remarks>
public interface IHeikinAshi<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Current Heikin-Ashi Open value
    /// </summary>
    TOutput HA_Open { get; }
    
    /// <summary>
    /// Current Heikin-Ashi High value
    /// </summary>
    TOutput HA_High { get; }
    
    /// <summary>
    /// Current Heikin-Ashi Low value
    /// </summary>
    TOutput HA_Low { get; }
    
    /// <summary>
    /// Current Heikin-Ashi Close value
    /// </summary>
    TOutput HA_Close { get; }
    
    /// <summary>
    /// Indicates if the current Heikin-Ashi candle is bullish (close > open)
    /// </summary>
    bool IsBullish { get; }
    
    /// <summary>
    /// Indicates if the current Heikin-Ashi candle is bearish (close < open)
    /// </summary>
    bool IsBearish { get; }
    
    /// <summary>
    /// Indicates if the current Heikin-Ashi candle is a doji (close ~= open)
    /// </summary>
    bool IsDoji { get; }
    
    /// <summary>
    /// Provides trend strength indication:
    /// - Strong bullish: long green body, small/no upper shadow
    /// - Strong bearish: long red body, small/no lower shadow
    /// - Consolidation: small bodies with long shadows
    /// </summary>
    int TrendStrength { get; }
}