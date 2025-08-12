using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Ichimoku Cloud indicator interface.
/// </summary>
/// <remarks>
/// The Ichimoku Cloud (Ichimoku Kinko Hyo) is a comprehensive technical analysis system that provides
/// information about support and resistance, trend direction, momentum, and trading signals.
/// 
/// The indicator consists of five lines:
/// - Tenkan-sen (Conversion Line): (9-period high + 9-period low) / 2
/// - Kijun-sen (Base Line): (26-period high + 26-period low) / 2  
/// - Senkou Span A (Leading Span A): (Tenkan + Kijun) / 2, plotted 26 periods ahead
/// - Senkou Span B (Leading Span B): (52-period high + 52-period low) / 2, plotted 26 periods ahead
/// - Chikou Span (Lagging Span): Close price plotted 26 periods behind
/// 
/// Available implementations:
/// - IchimokuCloud_QC: QuantConnect implementation (default, stable)
/// - IchimokuCloud_FP: First-party implementation (custom features, optimized with circular buffers)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IIchimokuCloud<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Conversion Line Period (Tenkan-sen period)
    /// </summary>
    int ConversionLinePeriod { get; }
    
    /// <summary>
    /// Base Line Period (Kijun-sen period)
    /// </summary>
    int BaseLinePeriod { get; }
    
    /// <summary>
    /// Leading Span B Period (Senkou Span B period)
    /// </summary>
    int LeadingSpanBPeriod { get; }
    
    /// <summary>
    /// Displacement (periods ahead for leading spans and behind for lagging span)
    /// </summary>
    int Displacement { get; }
    
    /// <summary>
    /// Tenkan-sen (Conversion Line): (9-period high + 9-period low) / 2
    /// </summary>
    TOutput TenkanSen { get; }
    
    /// <summary>
    /// Kijun-sen (Base Line): (26-period high + 26-period low) / 2
    /// </summary>
    TOutput KijunSen { get; }
    
    /// <summary>
    /// Senkou Span A (Leading Span A): (Tenkan + Kijun) / 2, plotted 26 periods ahead
    /// </summary>
    TOutput SenkouSpanA { get; }
    
    /// <summary>
    /// Senkou Span B (Leading Span B): (52-period high + 52-period low) / 2, plotted 26 periods ahead
    /// </summary>
    TOutput SenkouSpanB { get; }
    
    /// <summary>
    /// Chikou Span (Lagging Span): Close price plotted 26 periods behind
    /// </summary>
    TOutput ChikouSpan { get; }
}