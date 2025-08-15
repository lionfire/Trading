using LionFire.Structures;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Relative Strength Index (RSI) indicator.
/// RSI is a momentum oscillator that measures the speed and magnitude of price changes.
/// Values range from 0 to 100, with readings above 70 typically considered overbought
/// and below 30 considered oversold.
/// </summary>
public class RSI_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<RSI_QC<TPrice, TOutput>, global::QuantConnect.Indicators.RelativeStrengthIndex, PRSI<TPrice, TOutput>, TPrice, TOutput>, 
    IIndicator2<RSI_QC<TPrice, TOutput>, PRSI<TPrice, TOutput>, TPrice, TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the RSI indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "RSI",
                ValueType = typeof(TOutput),
                // Description = "Relative Strength Index value (0-100)"
            }];

    /// <summary>
    /// Gets the output slots for the RSI indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PRSI<TPrice, TOutput> p)
        => [new() {
                Name = "RSI",
                ValueType = typeof(TOutput),
                // Description = "Relative Strength Index value (0-100)"
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The RSI parameters
    /// </summary>
    public readonly PRSI<TPrice, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new RSI indicator instance
    /// </summary>
    public static RSI_QC<TPrice, TOutput> Create(PRSI<TPrice, TOutput> p) => new RSI_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the RSI indicator
    /// </summary>
    public RSI_QC(PRSI<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.RelativeStrengthIndex(
            parameters.Period, 
            parameters.MovingAverageType))
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
    /// Gets the current RSI value
    /// </summary>
    public TOutput CurrentValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Checks if the RSI indicates overbought conditions
    /// </summary>
    public bool IsOverbought => WrappedIndicator.IsReady && 
        Convert.ToDecimal(WrappedIndicator.Current.Price) >= Parameters.OverboughtLevel;

    /// <summary>
    /// Checks if the RSI indicates oversold conditions
    /// </summary>
    public bool IsOversold => WrappedIndicator.IsReady && 
        Convert.ToDecimal(WrappedIndicator.Current.Price) <= Parameters.OversoldLevel;

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
    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Create a TradeBar with the price value (RSI uses Close price)
            // For RSI, we only need the Close price, other OHLC values are not used
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

            // Output the RSI value if ready
            if (WrappedIndicator.IsReady)
            {
                var rsiValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { rsiValue });
                }
                
                OnNext_PopulateOutput(rsiValue, output, ref outputIndex, ref outputSkip);
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
    }

    #endregion
}