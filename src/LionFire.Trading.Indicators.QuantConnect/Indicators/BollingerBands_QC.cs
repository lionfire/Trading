using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Bollinger Bands indicator.
/// Bollinger Bands consist of a middle band (SMA) and two outer bands
/// calculated as standard deviations from the middle band.
/// </summary>
public class BollingerBands_QC<TInput, TOutput> : QuantConnectIndicatorWrapper<BollingerBands_QC<TInput, TOutput>, global::QuantConnect.Indicators.BollingerBands, PBollingerBands<TInput, TOutput>, TInput, TOutput>, 
    IIndicator2<BollingerBands_QC<TInput, TOutput>, PBollingerBands<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Bollinger Bands indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "UpperBand",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "MiddleBand",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "LowerBand",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Bollinger Bands indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PBollingerBands<TInput, TOutput> p)
        => [
            new() {
                Name = "UpperBand",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "MiddleBand",
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
    /// The Bollinger Bands parameters
    /// </summary>
    public readonly PBollingerBands<TInput, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Bollinger Bands indicator instance
    /// </summary>
    public static BollingerBands_QC<TInput, TOutput> Create(PBollingerBands<TInput, TOutput> p) => new BollingerBands_QC<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Bollinger Bands indicator
    /// </summary>
    public BollingerBands_QC(PBollingerBands<TInput, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.BollingerBands(
            parameters.Period, 
            Convert.ToDecimal(parameters.StandardDeviations)))
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
    /// Gets the middle band value (SMA)
    /// </summary>
    public TOutput MiddleBand => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.MiddleBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the lower band value
    /// </summary>
    public TOutput LowerBand => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.LowerBand.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the band width (Upper - Lower)
    /// </summary>
    public TOutput BandWidth => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.BandWidth.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the %B value
    /// </summary>
    public TOutput PercentB => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.PercentB.Current.Price) : default(TOutput)!;

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;

    #endregion

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Create a TradeBar with the price value
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.None,
                open: Convert.ToDecimal(input),
                high: Convert.ToDecimal(input),
                low: Convert.ToDecimal(input),
                close: Convert.ToDecimal(input),
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the band values if ready
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