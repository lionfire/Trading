using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Money Flow Index (MFI) indicator.
/// MFI is a volume-weighted momentum oscillator that uses both price and volume 
/// to identify overbought or oversold conditions. Also known as volume-weighted RSI.
/// </summary>
public class MFI_QC<TInput, TOutput> : QuantConnectIndicatorWrapper<MFI_QC<TInput, TOutput>, global::QuantConnect.Indicators.MoneyFlowIndex, PMFI<TInput, TOutput>, TInput, TOutput>, 
    IIndicator2<MFI_QC<TInput, TOutput>, PMFI<TInput, TOutput>, TInput, TOutput>,
    IMFI<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the MFI indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "MFI",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the MFI indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PMFI<TInput, TOutput> p)
        => [new() {
                Name = "MFI",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The MFI parameters
    /// </summary>
    public readonly PMFI<TInput, TOutput> Parameters;

    /// <summary>
    /// The period used for MFI calculation
    /// </summary>
    public int Period => Parameters.Period;
    
    /// <summary>
    /// The overbought threshold level
    /// </summary>
    public TOutput OverboughtLevel => Parameters.OverboughtLevel;
    
    /// <summary>
    /// The oversold threshold level
    /// </summary>
    public TOutput OversoldLevel => Parameters.OversoldLevel;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new MFI indicator instance
    /// </summary>
    public static MFI_QC<TInput, TOutput> Create(PMFI<TInput, TOutput> p) => new MFI_QC<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the MFI indicator
    /// </summary>
    public MFI_QC(PMFI<TInput, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.MoneyFlowIndex(parameters.Period))
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
    /// Gets the current MFI value
    /// </summary>
    public TOutput CurrentValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : TOutput.Zero;

    /// <summary>
    /// Checks if the MFI indicates overbought conditions
    /// </summary>
    public bool IsOverbought => WrappedIndicator.IsReady && CurrentValue > OverboughtLevel;

    /// <summary>
    /// Checks if the MFI indicates oversold conditions
    /// </summary>
    public bool IsOversold => WrappedIndicator.IsReady && CurrentValue < OversoldLevel;

    /// <summary>
    /// Gets the sum of positive money flow over the current period
    /// Note: QuantConnect's MoneyFlowIndex doesn't expose this directly
    /// </summary>
    public TOutput PositiveMoneyFlow => WrappedIndicator.IsReady 
        ? ConvertToOutput(WrappedIndicator.PositiveMoneyFlow) 
        : TOutput.Zero;

    /// <summary>
    /// Gets the sum of negative money flow over the current period
    /// Note: QuantConnect's MoneyFlowIndex doesn't expose this directly
    /// </summary>
    public TOutput NegativeMoneyFlow => WrappedIndicator.IsReady 
        ? ConvertToOutput(WrappedIndicator.NegativeMoneyFlow) 
        : TOutput.Zero;

    /// <summary>
    /// Gets the current money flow ratio
    /// </summary>
    public TOutput MoneyFlowRatio => NegativeMoneyFlow == TOutput.Zero 
        ? TOutput.CreateChecked(100) 
        : PositiveMoneyFlow / NegativeMoneyFlow;

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
            // Extract OHLCV data from input
            var (open, high, low, close, volume) = ExtractOHLCV(input);

            // Create a TradeBar with OHLCV values for QuantConnect's MoneyFlowIndex
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.None,
                open: open,
                high: high,
                low: low,
                close: close,
                volume: volume,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the MFI value if ready
            if (WrappedIndicator.IsReady)
            {
                var mfiValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { mfiValue });
                }
                
                OnNext_PopulateOutput(mfiValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(TOutput.Zero, output, ref outputIndex, ref outputSkip);
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

    #region Helper Methods

    /// <summary>
    /// Extracts OHLCV data from the input data structure
    /// </summary>
    private (decimal open, decimal high, decimal low, decimal close, decimal volume) ExtractOHLCV(TInput input)
    {
        // Handle different input types
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Look for OHLCV properties
        var openProperty = inputType.GetProperty("Open");
        var highProperty = inputType.GetProperty("High");
        var lowProperty = inputType.GetProperty("Low");
        var closeProperty = inputType.GetProperty("Close");
        var volumeProperty = inputType.GetProperty("Volume");
        
        if (openProperty != null && highProperty != null && lowProperty != null && 
            closeProperty != null && volumeProperty != null)
        {
            var openValue = openProperty.GetValue(boxed);
            var highValue = highProperty.GetValue(boxed);
            var lowValue = lowProperty.GetValue(boxed);
            var closeValue = closeProperty.GetValue(boxed);
            var volumeValue = volumeProperty.GetValue(boxed);
            
            return (
                Convert.ToDecimal(openValue!),
                Convert.ToDecimal(highValue!),
                Convert.ToDecimal(lowValue!),
                Convert.ToDecimal(closeValue!),
                Convert.ToDecimal(volumeValue!)
            );
        }
        
        throw new InvalidOperationException(
            $"Unable to extract OHLCV data from input type {inputType.Name}. " +
            "Input must have Open, High, Low, Close, and Volume properties.");
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