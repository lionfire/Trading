using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Hull Moving Average (HMA) indicator interface.
/// </summary>
/// <remarks>
/// Hull Moving Average is a fast moving average that aims to eliminate the lag 
/// associated with traditional moving averages. It uses weighted moving averages 
/// to achieve superior smoothing with reduced lag.
/// 
/// Calculation:
/// 1. WMA1 = WMA(Price, Period/2)
/// 2. WMA2 = WMA(Price, Period)
/// 3. Raw HMA = WMA(2 × WMA1 - WMA2, SQRT(Period))
/// 
/// Available implementations:
/// - HullMovingAverage_QC: QuantConnect implementation (default, stable)
/// - HullMovingAverage_FP: First-party implementation (custom features, optimized)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IHullMovingAverage<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for Hull Moving Average calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current Hull Moving Average value
    /// </summary>
    TOutput Value { get; }
}