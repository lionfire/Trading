using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// ZigZag indicator interface.
/// Identifies significant price swing highs and lows by filtering out minor price movements.
/// </summary>
/// <remarks>
/// Available implementations:
/// - ZigZag_QC: QuantConnect implementation (when available)
/// - ZigZag_FP: First-party implementation (default, custom features)
/// - ZigZagOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// 
/// The ZigZag indicator connects significant swing highs and lows based on a minimum
/// percentage change (deviation) and minimum number of bars between pivots (depth).
/// It helps identify trend reversals and key support/resistance levels.
/// </remarks>
public interface IZigZag<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The minimum percentage deviation required to form a new pivot point
    /// </summary>
    TOutput Deviation { get; }
    
    /// <summary>
    /// The minimum number of bars required between pivot points
    /// </summary>
    int Depth { get; }
    
    /// <summary>
    /// Current ZigZag value (price of the last confirmed pivot point)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// The price of the last confirmed pivot high
    /// </summary>
    TOutput LastPivotHigh { get; }
    
    /// <summary>
    /// The price of the last confirmed pivot low
    /// </summary>
    TOutput LastPivotLow { get; }
    
    /// <summary>
    /// Current direction of the ZigZag line (1 for up, -1 for down, 0 for indeterminate)
    /// </summary>
    int Direction { get; }
    
    /// <summary>
    /// Indicates if the indicator has enough data to produce reliable results
    /// </summary>
    bool IsReady { get; }
    
    /// <summary>
    /// Gets a list of recent pivot points (optional, implementation dependent)
    /// </summary>
    IReadOnlyList<ZigZagPivot<TOutput>>? RecentPivots { get; }
}

/// <summary>
/// Represents a pivot point identified by the ZigZag indicator
/// </summary>
/// <typeparam name="TOutput">The numeric type for prices</typeparam>
public struct ZigZagPivot<TOutput>
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The price level of the pivot point
    /// </summary>
    public TOutput Price { get; set; }
    
    /// <summary>
    /// The bar index where this pivot occurred
    /// </summary>
    public int BarIndex { get; set; }
    
    /// <summary>
    /// Whether this is a pivot high (true) or pivot low (false)
    /// </summary>
    public bool IsHigh { get; set; }
    
    /// <summary>
    /// Whether this pivot point has been confirmed (won't change)
    /// </summary>
    public bool IsConfirmed { get; set; }
    
    public override string ToString() => $"{(IsHigh ? "High" : "Low")} @ {Price} (Bar: {BarIndex}){(IsConfirmed ? " [Confirmed]" : "")}";
}