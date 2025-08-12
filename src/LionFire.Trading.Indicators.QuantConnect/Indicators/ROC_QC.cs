using LionFire.Structures;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Rate of Change (ROC) indicator.
/// ROC measures the percentage change between the current value and the value n periods ago.
/// Formula: ((Current Price - Price N periods ago) / Price N periods ago) * 100
/// </summary>
public class ROC_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<ROC_QC<TPrice, TOutput>, global::QuantConnect.Indicators.RateOfChange, PROC<TPrice, TOutput>, TPrice, TOutput>, 
    IIndicator2<ROC_QC<TPrice, TOutput>, PROC<TPrice, TOutput>, TPrice, TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the ROC indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "ROC",
                ValueType = typeof(TOutput),
                // Description = "Rate of Change percentage value"
            }];

    /// <summary>
    /// Gets the output slots for the ROC indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PROC<TPrice, TOutput> p)
        => [new() {
                Name = "ROC",
                ValueType = typeof(TOutput),
                // Description = "Rate of Change percentage value"
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The ROC parameters
    /// </summary>
    public readonly PROC<TPrice, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new ROC indicator instance
    /// </summary>
    public static ROC_QC<TPrice, TOutput> Create(PROC<TPrice, TOutput> p) => new ROC_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the ROC indicator
    /// </summary>
    public ROC_QC(PROC<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.RateOfChange(parameters.Period))
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
    /// Gets the current ROC value (percentage change)
    /// </summary>
    public TOutput CurrentValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Value) : default(TOutput)!;

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
            // Create a TradeBar with the price value (ROC uses Close price)
            // For ROC, we only need the Close price, other OHLC values are not used
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: QuantConnect.Symbol.None,
                open: Convert.ToDecimal(input),
                high: Convert.ToDecimal(input),
                low: Convert.ToDecimal(input),
                close: Convert.ToDecimal(input),
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the ROC value if ready
            if (WrappedIndicator.IsReady)
            {
                // QuantConnect ROC returns decimal rate of change, multiply by 100 to get percentage
                var rocValue = WrappedIndicator.Current.Value * 100m;
                var rocValueOutput = ConvertToOutput(rocValue);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { rocValueOutput });
                }
                
                OnNext_PopulateOutput(rocValueOutput, output, ref outputIndex, ref outputSkip);
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