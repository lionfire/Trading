using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Chaikin Money Flow (CMF) indicator interface.
/// CMF is a volume-weighted average of accumulation and distribution over a specified period.
/// It oscillates between -1 and +1, measuring the amount of Money Flow Volume over the period.
/// </summary>
/// <remarks>
/// Available implementations:
/// - ChaikinMoneyFlow_QC: QuantConnect implementation (default, stable)  
/// - ChaikinMoneyFlow_FP: First-party implementation (custom features)
/// - ChaikinMoneyFlowOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IChaikinMoneyFlow<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for CMF calculation (default: 21)
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current CMF value (range -1 to +1)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Indicates if the CMF is currently above zero (buying pressure)
    /// </summary>
    bool IsBullish { get; }
    
    /// <summary>
    /// Indicates if the CMF is currently below zero (selling pressure)
    /// </summary>
    bool IsBearish { get; }

    /// <summary>
    /// Gets the sum of money flow volume over the current period
    /// </summary>
    TOutput MoneyFlowVolumeSum { get; }

    /// <summary>
    /// Gets the sum of volume over the current period
    /// </summary>
    TOutput VolumeSum { get; }
}