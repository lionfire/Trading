using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Fibonacci Retracement indicator interface.
/// </summary>
/// <remarks>
/// Fibonacci Retracement is a technical analysis tool that identifies potential support
/// and resistance levels based on key Fibonacci ratios derived from the highest high
/// and lowest low over a specified lookback period.
/// 
/// Available implementations:
/// - FibonacciRetracement_FP: First-party implementation (optimized for streaming)
/// 
/// The indicator outputs multiple retracement levels:
/// - 0.0% (swing low)
/// - 23.6% retracement
/// - 38.2% retracement 
/// - 50.0% retracement
/// - 61.8% retracement (golden ratio)
/// - 78.6% retracement
/// - 100.0% (swing high)
/// - Extension levels: 161.8%, 261.8% (optional)
/// </remarks>
public interface IFibonacciRetracement<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The lookback period used to find swing high and low
    /// </summary>
    int LookbackPeriod { get; }
    
    /// <summary>
    /// The highest high value over the lookback period (100% level)
    /// </summary>
    TOutput SwingHigh { get; }
    
    /// <summary>
    /// The lowest low value over the lookback period (0% level)
    /// </summary>
    TOutput SwingLow { get; }
    
    /// <summary>
    /// 0.0% Fibonacci retracement level (swing low)
    /// </summary>
    TOutput Level000 { get; }
    
    /// <summary>
    /// 23.6% Fibonacci retracement level
    /// </summary>
    TOutput Level236 { get; }
    
    /// <summary>
    /// 38.2% Fibonacci retracement level
    /// </summary>
    TOutput Level382 { get; }
    
    /// <summary>
    /// 50.0% Fibonacci retracement level (mid-point)
    /// </summary>
    TOutput Level500 { get; }
    
    /// <summary>
    /// 61.8% Fibonacci retracement level (golden ratio)
    /// </summary>
    TOutput Level618 { get; }
    
    /// <summary>
    /// 78.6% Fibonacci retracement level
    /// </summary>
    TOutput Level786 { get; }
    
    /// <summary>
    /// 100.0% Fibonacci retracement level (swing high)
    /// </summary>
    TOutput Level1000 { get; }
    
    /// <summary>
    /// 161.8% Fibonacci extension level (optional)
    /// </summary>
    TOutput Level1618 { get; }
    
    /// <summary>
    /// 261.8% Fibonacci extension level (optional)
    /// </summary>
    TOutput Level2618 { get; }
    
    /// <summary>
    /// Indicates if the indicator has sufficient data to calculate retracement levels
    /// </summary>
    bool IsReady { get; }
}