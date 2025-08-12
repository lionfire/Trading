using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Stochastic Oscillator indicator interface.
/// </summary>
/// <remarks>
/// The Stochastic Oscillator is a momentum indicator that compares a security's closing price
/// to its price range over a given time period. It consists of two lines:
/// - %K: The fast stochastic indicator
/// - %D: The slow stochastic indicator (signal line), which is a moving average of %K
/// 
/// Available implementations:
/// - Stochastic_QC: QuantConnect implementation (default, stable)
/// - Stochastic_FP: First-party implementation (custom features)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IStochastic<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for %K calculation (fast period)
    /// </summary>
    int FastPeriod { get; }
    
    /// <summary>
    /// The period used for smoothing %K (slow K period)
    /// </summary>
    int SlowKPeriod { get; }
    
    /// <summary>
    /// The period used for %D signal line calculation (slow D period)
    /// </summary>
    int SlowDPeriod { get; }
    
    /// <summary>
    /// The overbought threshold level (typically 80)
    /// </summary>
    TOutput OverboughtLevel { get; }
    
    /// <summary>
    /// The oversold threshold level (typically 20)
    /// </summary>
    TOutput OversoldLevel { get; }
    
    /// <summary>
    /// Current %K value (fast stochastic)
    /// </summary>
    TOutput PercentK { get; }
    
    /// <summary>
    /// Current %D value (signal line - moving average of %K)
    /// </summary>
    TOutput PercentD { get; }
    
    /// <summary>
    /// Indicates if the Stochastic is currently above the overbought level
    /// </summary>
    bool IsOverbought { get; }
    
    /// <summary>
    /// Indicates if the Stochastic is currently below the oversold level
    /// </summary>
    bool IsOversold { get; }
}