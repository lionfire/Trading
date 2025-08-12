using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Money Flow Index (MFI) indicator interface.
/// MFI is a momentum oscillator that uses price and volume to identify overbought 
/// or oversold conditions in a security. Also known as volume-weighted RSI.
/// </summary>
/// <remarks>
/// Available implementations:
/// - MFI_QC: QuantConnect implementation (default, stable)  
/// - MFI_FP: First-party implementation (custom features)
/// - MFIOpt: Optimized implementation (when available)
/// 
/// Selection: Automatic based on performance profile, or set
/// PreferredImplementation in parameters.
/// </remarks>
public interface IMFI<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for MFI calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The overbought threshold level (typically 80)
    /// </summary>
    TOutput OverboughtLevel { get; }
    
    /// <summary>
    /// The oversold threshold level (typically 20)
    /// </summary>
    TOutput OversoldLevel { get; }
    
    /// <summary>
    /// Current MFI value (0-100)
    /// </summary>
    TOutput CurrentValue { get; }
    
    /// <summary>
    /// Indicates if the MFI is currently above the overbought level
    /// </summary>
    bool IsOverbought { get; }
    
    /// <summary>
    /// Indicates if the MFI is currently below the oversold level
    /// </summary>
    bool IsOversold { get; }

    /// <summary>
    /// Gets the sum of positive money flow over the current period
    /// </summary>
    TOutput PositiveMoneyFlow { get; }

    /// <summary>
    /// Gets the sum of negative money flow over the current period  
    /// </summary>
    TOutput NegativeMoneyFlow { get; }

    /// <summary>
    /// Gets the current money flow ratio (positive money flow / negative money flow)
    /// </summary>
    TOutput MoneyFlowRatio { get; }
}