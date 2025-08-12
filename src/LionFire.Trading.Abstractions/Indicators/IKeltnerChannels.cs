using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Keltner Channels indicator interface.
/// </summary>
/// <remarks>
/// Keltner Channels consist of a middle line (EMA) and upper/lower bands
/// calculated using Average True Range (ATR).
/// 
/// Available implementations:
/// - KeltnerChannels_QC: QuantConnect implementation (default, stable)
/// - KeltnerChannels_FP: First-party implementation (custom features)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface IKeltnerChannels<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for the EMA calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// The period used for the ATR calculation
    /// </summary>
    int AtrPeriod { get; }
    
    /// <summary>
    /// The ATR multiplier for the channel bands
    /// </summary>
    TOutput AtrMultiplier { get; }
    
    /// <summary>
    /// The upper channel band (EMA + ATR * Multiplier)
    /// </summary>
    TOutput UpperBand { get; }
    
    /// <summary>
    /// The middle line (EMA of Close prices)
    /// </summary>
    TOutput MiddleLine { get; }
    
    /// <summary>
    /// The lower channel band (EMA - ATR * Multiplier)
    /// </summary>
    TOutput LowerBand { get; }
    
    /// <summary>
    /// The current Average True Range value
    /// </summary>
    TOutput AtrValue { get; }
    
    /// <summary>
    /// The channel width (Upper Band - Lower Band)
    /// </summary>
    TOutput ChannelWidth { get; }
}