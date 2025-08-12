using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Pivot Points indicator interface.
/// Pivot Points are support and resistance levels calculated from the previous period's High, Low, and Close prices.
/// </summary>
/// <remarks>
/// Pivot Points provide seven key levels:
/// - Pivot Point (P): The main pivot level calculated as (High + Low + Close) / 3
/// - Resistance levels (R1, R2, R3): Above the pivot point
/// - Support levels (S1, S2, S3): Below the pivot point
/// 
/// Available implementations:
/// - PivotPoints_QC: QuantConnect implementation (when available)
/// - PivotPoints_FP: First-party implementation (default)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IPivotPoints<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period type used for pivot calculation (Daily, Weekly, Monthly)
    /// </summary>
    PivotPointsPeriod PeriodType { get; }
    
    /// <summary>
    /// The main pivot point value: (High + Low + Close) / 3
    /// </summary>
    TOutput PivotPoint { get; }
    
    /// <summary>
    /// First resistance level: (2 × P) - Low
    /// </summary>
    TOutput Resistance1 { get; }
    
    /// <summary>
    /// First support level: (2 × P) - High
    /// </summary>
    TOutput Support1 { get; }
    
    /// <summary>
    /// Second resistance level: P + (High - Low)
    /// </summary>
    TOutput Resistance2 { get; }
    
    /// <summary>
    /// Second support level: P - (High - Low)
    /// </summary>
    TOutput Support2 { get; }
    
    /// <summary>
    /// Third resistance level: High + 2 × (P - Low)
    /// </summary>
    TOutput Resistance3 { get; }
    
    /// <summary>
    /// Third support level: Low - 2 × (High - P)
    /// </summary>
    TOutput Support3 { get; }
}

/// <summary>
/// Enumeration of pivot point period types
/// </summary>
public enum PivotPointsPeriod
{
    /// <summary>
    /// Daily pivot points (default) - calculated from previous day's OHLC
    /// </summary>
    Daily,
    
    /// <summary>
    /// Weekly pivot points - calculated from previous week's OHLC
    /// </summary>
    Weekly,
    
    /// <summary>
    /// Monthly pivot points - calculated from previous month's OHLC
    /// </summary>
    Monthly
}