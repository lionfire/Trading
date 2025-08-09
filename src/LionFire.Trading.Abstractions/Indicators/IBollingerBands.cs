using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Bollinger Bands indicator interface.
/// </summary>
/// <remarks>
/// Bollinger Bands consist of a middle band (Simple Moving Average) and two outer bands
/// calculated as standard deviations from the middle band.
/// 
/// Available implementations:
/// - BollingerBandsQC: QuantConnect implementation (default, stable)
/// - BollingerBandsFP: First-party implementation (custom features)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IBollingerBands<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for moving average and standard deviation calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The number of standard deviations from the middle band
    /// </summary>
    TOutput StandardDeviations { get; }
    
    /// <summary>
    /// The upper band value (Middle Band + k * Standard Deviation)
    /// </summary>
    TOutput UpperBand { get; }
    
    /// <summary>
    /// The middle band value (Simple Moving Average)
    /// </summary>
    TOutput MiddleBand { get; }
    
    /// <summary>
    /// The lower band value (Middle Band - k * Standard Deviation)
    /// </summary>
    TOutput LowerBand { get; }
    
    /// <summary>
    /// The width between upper and lower bands (Upper Band - Lower Band)
    /// </summary>
    TOutput BandWidth { get; }
    
    /// <summary>
    /// %B indicator: (Price - Lower Band) / (Upper Band - Lower Band)
    /// Indicates where price is relative to the bands (0 = lower band, 1 = upper band)
    /// </summary>
    TOutput PercentB { get; }
}