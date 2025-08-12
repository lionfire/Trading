using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Accumulation/Distribution Line (A/D Line) indicator interface.
/// </summary>
/// <remarks>
/// The A/D Line is a cumulative indicator that uses volume and price to assess 
/// whether a stock is being accumulated or distributed. It combines price and volume 
/// to show how money is flowing in and out of a security.
/// 
/// Calculation:
/// 1. Money Flow Multiplier = ((Close - Low) - (High - Close)) / (High - Low)
/// 2. Money Flow Volume = Money Flow Multiplier × Volume
/// 3. A/D Line = Previous A/D Line + Money Flow Volume
/// 
/// Available implementations:
/// - AccumulationDistributionLine_QC: QuantConnect implementation (wrapping QuantConnect's AccumulationDistribution)
/// - AccumulationDistributionLine_FP: First-party implementation (custom features)
/// - AccumulationDistributionLineOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IAccumulationDistributionLine<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Current Accumulation/Distribution Line value (cumulative)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Gets the latest Money Flow Volume added to the A/D Line
    /// Positive indicates accumulation (buying pressure), negative indicates distribution (selling pressure)
    /// </summary>
    TOutput LastMoneyFlowVolume { get; }
    
    /// <summary>
    /// Gets the latest Money Flow Multiplier calculated from price action
    /// Range: -1.0 to +1.0
    /// </summary>
    TOutput LastMoneyFlowMultiplier { get; }
    
    /// <summary>
    /// Gets a value indicating if the A/D Line is showing accumulation (upward trend)
    /// </summary>
    bool IsAccumulating { get; }
    
    /// <summary>
    /// Gets a value indicating if the A/D Line is showing distribution (downward trend)
    /// </summary>
    bool IsDistributing { get; }
}