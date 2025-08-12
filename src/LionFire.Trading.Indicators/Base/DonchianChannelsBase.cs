using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Donchian Channels implementations
/// </summary>
public abstract class DonchianChannelsBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PDonchianChannels<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    IDonchianChannels<HLC<TPrice>, TOutput>
    where TConcrete : DonchianChannelsBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PDonchianChannels<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PDonchianChannels<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Donchian Channels calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected DonchianChannelsBase(PDonchianChannels<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// The upper channel value (highest high over the period)
    /// </summary>
    public abstract TOutput UpperChannel { get; }

    /// <summary>
    /// The lower channel value (lowest low over the period)
    /// </summary>
    public abstract TOutput LowerChannel { get; }

    /// <summary>
    /// The middle channel value (average of upper and lower channels)
    /// </summary>
    public virtual TOutput MiddleChannel 
    { 
        get
        {
            if (!IsReady) return default(TOutput)!;
            
            // Calculate (UpperChannel + LowerChannel) / 2
            var sum = UpperChannel + LowerChannel;
            return sum / TOutput.CreateChecked(2);
        }
    }

    /// <summary>
    /// The width between upper and lower channels (Upper Channel - Lower Channel)
    /// </summary>
    public virtual TOutput ChannelWidth 
    { 
        get
        {
            if (!IsReady) return default(TOutput)!;
            return UpperChannel - LowerChannel;
        }
    }

    /// <summary>
    /// Position of current price within the channels (0 = lower channel, 1 = upper channel)
    /// </summary>
    public virtual TOutput PercentPosition 
    { 
        get
        {
            if (!IsReady) return default(TOutput)!;
            
            var width = ChannelWidth;
            if (width == TOutput.Zero) return TOutput.CreateChecked(0.5); // If width is zero, return middle
            
            // This will need to be overridden in derived classes to access current price
            return default(TOutput)!;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Donchian Channels indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "UpperChannel", ValueType = typeof(TOutput) },
            new() { Name = "LowerChannel", ValueType = typeof(TOutput) },
            new() { Name = "MiddleChannel", ValueType = typeof(TOutput) },
            new() { Name = "ChannelWidth", ValueType = typeof(TOutput) },
            new() { Name = "PercentPosition", ValueType = typeof(TOutput) }
        ];

    /// <summary>
    /// Gets the output slots for the Donchian Channels indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PDonchianChannels<TPrice, TOutput> p)
        => [
            new() { Name = "UpperChannel", ValueType = typeof(TOutput) },
            new() { Name = "LowerChannel", ValueType = typeof(TOutput) },
            new() { Name = "MiddleChannel", ValueType = typeof(TOutput) },
            new() { Name = "ChannelWidth", ValueType = typeof(TOutput) },
            new() { Name = "PercentPosition", ValueType = typeof(TOutput) }
        ];

    #endregion
}