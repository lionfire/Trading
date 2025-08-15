using LionFire.Structures;
using LionFire.Trading.ValueWindows;
using LionFire.Trading;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of On Balance Volume (OBV) indicator.
/// OBV tracks cumulative volume flow based on price direction.
/// It adds volume when price closes higher than previous close,
/// subtracts volume when price closes lower, and keeps volume unchanged
/// when price closes at the same level.
/// </summary>
public class OBV_QC<TInput, TOutput> : QuantConnectIndicatorWrapper<OBV_QC<TInput, TOutput>, global::QuantConnect.Indicators.OnBalanceVolume, POBV<TInput, TOutput>, TInput, TOutput>, 
    IIndicator2<OBV_QC<TInput, TOutput>, POBV<TInput, TOutput>, TInput, TOutput>,
    IOBV<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the OBV indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "OBV",
                ValueType = typeof(TOutput),
                // Description = "On Balance Volume cumulative value"
            }];

    /// <summary>
    /// Gets the output slots for the OBV indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(POBV<TInput, TOutput> p)
        => [new() {
                Name = "OBV",
                ValueType = typeof(TOutput),
                // Description = "On Balance Volume cumulative value"
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The OBV parameters
    /// </summary>
    public readonly POBV<TInput, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => 1; // OBV needs current and previous bar

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new OBV indicator instance
    /// </summary>
    public static OBV_QC<TInput, TOutput> Create(POBV<TInput, TOutput> p) => new OBV_QC<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the OBV indicator
    /// </summary>
    public OBV_QC(POBV<TInput, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.OnBalanceVolume())
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
    /// Gets the current OBV value
    /// </summary>
    public TOutput CurrentValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the last change in OBV (difference from previous value)
    /// </summary>
    public TOutput LastChange 
    {
        get
        {
            if (WrappedIndicator.IsReady && WrappedIndicator.Samples > 1)
            {
                // Calculate change from previous value
                var window = WrappedIndicator.Window;
                if (window.Count >= 2)
                {
                    var current = ConvertToOutput(window[0].Price);
                    var previous = ConvertToOutput(window[1].Price);
                    return current - previous;
                }
            }
            return TOutput.Zero;
        }
    }

    /// <summary>
    /// Checks if OBV is trending upward
    /// </summary>
    public bool IsRising => LastChange > TOutput.Zero;

    /// <summary>
    /// Checks if OBV is trending downward
    /// </summary>
    public bool IsFalling => LastChange < TOutput.Zero;

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;

    #endregion

    /// <summary>
    /// Process a batch of price and volume inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract price and volume from input
            var (price, volume) = ExtractPriceAndVolume(input);
            
            // Create a TradeBar with the price and volume values
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.None,
                open: price,
                high: price,
                low: price,
                close: price,
                volume: (long)volume, // QuantConnect expects long for volume
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the OBV value if ready
            if (WrappedIndicator.IsReady)
            {
                var obvValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { obvValue });
                }
                
                OnNext_PopulateOutput(obvValue, output, ref outputIndex, ref outputSkip);
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

    #region Helper Methods

    /// <summary>
    /// Extracts price and volume from the input data structure
    /// </summary>
    private (decimal price, decimal volume) ExtractPriceAndVolume(TInput input)
    {
        // Handle different input types using runtime type checking
        var boxed = (object)input;
        
        // Check for Bar type (runtime check since TInput is struct-constrained)
        if (boxed is Bar bar)
        {
            return (Convert.ToDecimal(bar.Close), Convert.ToDecimal(bar.Volume));
        }
        
        // Check for TimedBar type
        if (boxed is TimedBar timedBar)
        {
            return (Convert.ToDecimal(timedBar.Close), Convert.ToDecimal(timedBar.Volume));
        }
        
        // Try reflection for dynamic property access
        var inputType = typeof(TInput);
        
        // Look for Close and Volume properties
        var closeProperty = inputType.GetProperty("Close");
        var volumeProperty = inputType.GetProperty("Volume");
        
        if (closeProperty != null && volumeProperty != null)
        {
            var closeValue = closeProperty.GetValue(boxed);
            var volumeValue = volumeProperty.GetValue(boxed);
            
            return (
                Convert.ToDecimal(closeValue!),
                Convert.ToDecimal(volumeValue!)
            );
        }
        
        // If no Close/Volume properties, assume input is a tuple or has similar structure
        if (inputType.IsGenericType)
        {
            var genericArgs = inputType.GetGenericArguments();
            if (genericArgs.Length >= 2)
            {
                // Try to treat as (price, volume) tuple
                var fields = inputType.GetFields();
                if (fields.Length >= 2)
                {
                    var item1 = fields[0].GetValue(boxed);
                    var item2 = fields[1].GetValue(boxed);
                    
                    return (
                        Convert.ToDecimal(item1!),
                        Convert.ToDecimal(item2!)
                    );
                }
            }
        }
        
        throw new InvalidOperationException(
            $"Unable to extract price and volume from input type {inputType.Name}. " +
            "Input must have Close and Volume properties or be a (price, volume) tuple.");
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