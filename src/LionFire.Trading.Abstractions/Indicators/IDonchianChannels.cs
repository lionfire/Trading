using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Donchian Channels indicator interface.
/// </summary>
/// <remarks>
/// Donchian Channels consist of three lines: Upper Channel (highest high over N periods),
/// Lower Channel (lowest low over N periods), and Middle Channel (average of upper and lower).
/// 
/// Available implementations:
/// - DonchianChannels_QC: QuantConnect implementation (default, stable)
/// - DonchianChannels_FP: First-party implementation (custom features, optimized)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IDonchianChannels<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for calculating highest high and lowest low
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The upper channel value (highest high over the period)
    /// </summary>
    TOutput UpperChannel { get; }
    
    /// <summary>
    /// The lower channel value (lowest low over the period)
    /// </summary>
    TOutput LowerChannel { get; }
    
    /// <summary>
    /// The middle channel value (average of upper and lower channels)
    /// </summary>
    TOutput MiddleChannel { get; }
    
    /// <summary>
    /// The width between upper and lower channels (Upper Channel - Lower Channel)
    /// </summary>
    TOutput ChannelWidth { get; }
    
    /// <summary>
    /// Position of current price within the channels (0 = lower channel, 1 = upper channel)
    /// Calculated as: (Price - Lower Channel) / (Upper Channel - Lower Channel)
    /// </summary>
    TOutput PercentPosition { get; }
}