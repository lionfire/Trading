using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Keltner Channels indicator.
/// Keltner Channels consist of a middle line (EMA) and upper/lower bands
/// calculated using Average True Range (ATR).
/// </summary>
public class KeltnerChannels_QC<TInput, TOutput> : QuantConnectIndicatorWrapper<KeltnerChannels_QC<TInput, TOutput>, global::QuantConnect.Indicators.KeltnerChannels, PKeltnerChannels<TInput, TOutput>, TInput, TOutput>, 
    IIndicator2<KeltnerChannels_QC<TInput, TOutput>, PKeltnerChannels<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Keltner Channels indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "UpperBand",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "MiddleLine",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "LowerBand",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Keltner Channels indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PKeltnerChannels<TInput, TOutput> p)
        => [
            new() {
                Name = "UpperBand",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "MiddleLine",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "LowerBand",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion

    #region Parameters

    /// <summary>
    /// The Keltner Channels parameters
    /// </summary>
    public readonly PKeltnerChannels<TInput, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Math.Max(Parameters.Period, Parameters.AtrPeriod);

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Keltner Channels indicator instance
    /// </summary>
    public static KeltnerChannels_QC<TInput, TOutput> Create(PKeltnerChannels<TInput, TOutput> p) => new KeltnerChannels_QC<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Keltner Channels indicator
    /// </summary>
    public KeltnerChannels_QC(PKeltnerChannels<TInput, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.KeltnerChannels(
            parameters.Period, 
            Convert.ToDecimal(parameters.AtrMultiplier),
            QuantConnect.Indicators.MovingAverageType.Exponential,
            QuantConnect.Indicators.MovingAverageType.Simple,
            parameters.AtrPeriod))
    {
        Parameters = parameters;
    }

    #endregion

    #region State

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the upper band value
    /// </summary>
    public TOutput UpperBand => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.UpperBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the middle line value (EMA)
    /// </summary>
    public TOutput MiddleLine => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.MiddleBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the lower band value
    /// </summary>
    public TOutput LowerBand => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.LowerBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current ATR value
    /// </summary>
    public TOutput AtrValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.AverageTrueRange.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the channel width (Upper - Lower)
    /// </summary>
    public TOutput ChannelWidth => WrappedIndicator.IsReady ? UpperBand - LowerBand : default(TOutput)!;

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
    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // For Keltner Channels, we need HLC data. TInput should be HLC<T> or similar
            // Convert the input to decimal values for High, Low, Close
            var close = Convert.ToDecimal(input);
            
            // Create a TradeBar with HLC values
            // For single-value input, we assume Close = High = Low
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: QuantConnect.Symbol.None,
                open: close,
                high: close,
                low: close,
                close: close,
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the channel values if ready
            if (WrappedIndicator.IsReady)
            {
                var upper = ConvertToOutput(WrappedIndicator.UpperBand.Current.Price);
                var middle = ConvertToOutput(WrappedIndicator.MiddleBand.Current.Price);
                var lower = ConvertToOutput(WrappedIndicator.LowerBand.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { upper, middle, lower });
                }
                
                // Output in the order: Upper, Middle, Lower
                OnNext_PopulateOutput(upper, output, ref outputIndex, ref outputSkip);
                OnNext_PopulateOutput(middle, output, ref outputIndex, ref outputSkip);
                OnNext_PopulateOutput(lower, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
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
        else if (outputBuffer != null && outputIndex < outputBuffer.Length) 
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
    }

    #endregion
}