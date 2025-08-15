using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Donchian Channels indicator.
/// Donchian Channels consist of upper channel (highest high), lower channel (lowest low), 
/// and middle channel (average of upper and lower).
/// </summary>
public class DonchianChannels_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<DonchianChannels_QC<TPrice, TOutput>, global::QuantConnect.Indicators.DonchianChannel, PDonchianChannels<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IIndicator2<DonchianChannels_QC<TPrice, TOutput>, PDonchianChannels<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IDonchianChannels<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
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

    #region Parameters

    /// <summary>
    /// The Donchian Channels parameters
    /// </summary>
    public readonly PDonchianChannels<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Donchian Channels calculation
    /// </summary>
    public int Period => Parameters.Period;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Donchian Channels indicator instance
    /// </summary>
    public static DonchianChannels_QC<TPrice, TOutput> Create(PDonchianChannels<TPrice, TOutput> p) => new DonchianChannels_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Donchian Channels indicator
    /// </summary>
    public DonchianChannels_QC(PDonchianChannels<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.DonchianChannel(parameters.Period))
    {
        Parameters = parameters;
    }

    #endregion

    #region State

    private HLC<TPrice> currentInput;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the upper channel value (highest high over the period)
    /// </summary>
    public TOutput UpperChannel => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.UpperBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the lower channel value (lowest low over the period)
    /// </summary>
    public TOutput LowerChannel => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.LowerBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the middle channel value (average of upper and lower channels)
    /// </summary>
    public TOutput MiddleChannel => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the channel width (Upper Channel - Lower Channel)
    /// </summary>
    public TOutput ChannelWidth 
    { 
        get
        {
            if (!WrappedIndicator.IsReady) return default(TOutput)!;
            return UpperChannel - LowerChannel;
        }
    }

    /// <summary>
    /// Gets the percent position of current price within the channels
    /// </summary>
    public TOutput PercentPosition 
    { 
        get
        {
            if (!WrappedIndicator.IsReady) return default(TOutput)!;
            
            var width = ChannelWidth;
            if (width == TOutput.Zero) return TOutput.CreateChecked(0.5);
            
            // Use close price for percent position calculation
            var currentPrice = ConvertToOutput(Convert.ToDecimal(currentInput.Close));
            var lowerChannel = LowerChannel;
            
            return (currentPrice - lowerChannel) / width;
        }
    }

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;

    #endregion

    /// <summary>
    /// Process a batch of HLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            currentInput = input;

            // Create a TradeBar from HLC data
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.Empty,
                open: Convert.ToDecimal(input.Close), // Use close for open since we don't have open
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Close),
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the values if ready
            if (WrappedIndicator.IsReady)
            {
                // For simplicity, output the middle channel as primary value
                // In practice, you might want multiple outputs
                var middleValue = MiddleChannel;
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { middleValue });
                }
                
                OnNext_PopulateOutput(middleValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Helper method to populate the output buffer
    /// </summary>
    private static void OnNext_PopulateOutput(TOutput value, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        if (outputSkip > 0) 
        { 
            outputSkip--; 
        }
        else if (outputBuffer != null) 
        {
            outputBuffer[outputIndex++] = value;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clears and resets the indicator state
    /// </summary>
    public override void Clear() 
    { 
        base.Clear(); 
        WrappedIndicator.Reset();
        endTime = DefaultEndTime;
        currentInput = default;
    }

    #endregion
}