using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Volume Weighted Average Price (VWAP) indicator.
/// VWAP calculates the volume-weighted average price over a specified period.
/// Wraps QuantConnect's VolumeWeightedAveragePriceIndicator.
/// </summary>
public class VWAP_QC<TInput, TOutput> : QuantConnectIndicatorWrapper<VWAP_QC<TInput, TOutput>, global::QuantConnect.Indicators.VolumeWeightedAveragePriceIndicator, PVWAP<TInput, TOutput>, TInput, TOutput>,
    IIndicator2<VWAP_QC<TInput, TOutput>, PVWAP<TInput, TOutput>, TInput, TOutput>,
    IVWAP<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the VWAP indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "VWAP",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the VWAP indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PVWAP<TInput, TOutput> p)
        => [new() {
                Name = "VWAP",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The VWAP parameters
    /// </summary>
    public readonly PVWAP<TInput, TOutput> Parameters;

    /// <summary>
    /// The reset period for VWAP calculation
    /// </summary>
    public VWAPResetPeriod ResetPeriod => Parameters.ResetPeriod;

    /// <summary>
    /// Whether to use typical price (H+L+C)/3 instead of close price
    /// </summary>
    public bool UseTypicalPrice => Parameters.UseTypicalPrice;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => 1;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new VWAP indicator instance
    /// </summary>
    public static VWAP_QC<TInput, TOutput> Create(PVWAP<TInput, TOutput> p) => new VWAP_QC<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the VWAP indicator
    /// </summary>
    public VWAP_QC(PVWAP<TInput, TOutput> parameters) : base(
        CreateQuantConnectVWAP(parameters))
    {
        Parameters = parameters;
        lastResetTime = null;
        hasReset = false;
    }

    /// <summary>
    /// Creates the underlying QuantConnect VWAP indicator based on reset period
    /// </summary>
    private static global::QuantConnect.Indicators.VolumeWeightedAveragePriceIndicator CreateQuantConnectVWAP(PVWAP<TInput, TOutput> parameters)
    {
        // QuantConnect VWAP uses a period parameter, but for daily reset we use a large period
        // and handle reset logic ourselves
        int period = parameters.ResetPeriod switch
        {
            VWAPResetPeriod.Never => 1000000, // Very large period for never reset
            VWAPResetPeriod.Daily => 1000, // Large enough for daily bars
            VWAPResetPeriod.Weekly => 7000, // Large enough for weekly bars  
            VWAPResetPeriod.Monthly => 30000, // Large enough for monthly bars
            VWAPResetPeriod.Custom => 1000, // Default, handle reset manually
            _ => 1000
        };

        return new global::QuantConnect.Indicators.VolumeWeightedAveragePriceIndicator(period);
    }

    #endregion

    #region State

    private DateTime? lastResetTime;
    private bool hasReset;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the current VWAP value
    /// </summary>
    public TOutput Value => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : TOutput.Zero;

    /// <summary>
    /// Gets the cumulative typical price × volume sum for the current period
    /// Note: QuantConnect's VWAP doesn't expose these values directly, so we approximate
    /// </summary>
    public TOutput CumulativePriceVolume => WrappedIndicator.IsReady 
        ? ConvertToOutput(WrappedIndicator.Current.Price * WrappedIndicator.Samples) 
        : TOutput.Zero;

    /// <summary>
    /// Gets the cumulative volume for the current period
    /// Note: QuantConnect's VWAP doesn't expose volume directly, approximated from samples
    /// </summary>
    public TOutput CumulativeVolume => WrappedIndicator.IsReady 
        ? TOutput.CreateChecked(WrappedIndicator.Samples) 
        : TOutput.Zero;

    /// <summary>
    /// Gets a value indicating whether the VWAP has been reset for the current period
    /// </summary>
    public bool HasReset => hasReset;

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
            // Extract price, volume, and timestamp from input
            var (high, low, close, volume, timestamp) = ExtractOHLCVTimestamp(input);

            // Check if we need to reset based on the reset period
            CheckAndPerformReset(timestamp);

            // Calculate typical price if needed
            decimal effectivePrice = UseTypicalPrice 
                ? (high + low + close) / 3.0m 
                : close;

            // Create a TradeBar for QuantConnect's VWAP indicator
            var tradeBar = new TradeBar
            {
                Time = endTime,
                Open = effectivePrice,
                High = UseTypicalPrice ? effectivePrice : high,
                Low = UseTypicalPrice ? effectivePrice : low,
                Close = effectivePrice,
                Volume = volume
            };

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the VWAP value if ready
            if (WrappedIndicator.IsReady)
            {
                var vwapValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { vwapValue });
                }
                
                OnNext_PopulateOutput(vwapValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(TOutput.Zero, output, ref outputIndex, ref outputSkip);
            }
            
            hasReset = false; // Clear the reset flag after processing
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
    /// Extracts OHLCV and timestamp from the input data structure
    /// </summary>
    private (decimal high, decimal low, decimal close, decimal volume, DateTime timestamp) ExtractOHLCVTimestamp(TInput input)
    {
        DateTime timestamp = DateTime.UtcNow; // Default timestamp

        // Handle different input types
        switch (input)
        {
            case Bar bar:
                return (Convert.ToDecimal(bar.High), Convert.ToDecimal(bar.Low), 
                       Convert.ToDecimal(bar.Close), Convert.ToDecimal(bar.Volume),
                       bar.TimeStart?.DateTime ?? timestamp);
            
            case TimedBar timedBar:
                return (Convert.ToDecimal(timedBar.High), Convert.ToDecimal(timedBar.Low), 
                       Convert.ToDecimal(timedBar.Close), Convert.ToDecimal(timedBar.Volume),
                       timedBar.TimeStart?.DateTime ?? timestamp);
            
            default:
                // Try reflection for dynamic property access
                var inputType = typeof(TInput);
                var boxed = (object)input;
                
                // Look for OHLCV and Time properties
                var highProperty = inputType.GetProperty("High");
                var lowProperty = inputType.GetProperty("Low");
                var closeProperty = inputType.GetProperty("Close");
                var volumeProperty = inputType.GetProperty("Volume");
                var timeProperty = inputType.GetProperty("Time") ?? 
                                 inputType.GetProperty("Timestamp") ?? 
                                 inputType.GetProperty("DateTime");
                
                if (closeProperty != null && volumeProperty != null)
                {
                    var highValue = highProperty?.GetValue(boxed) ?? closeProperty.GetValue(boxed);
                    var lowValue = lowProperty?.GetValue(boxed) ?? closeProperty.GetValue(boxed);
                    var closeValue = closeProperty.GetValue(boxed);
                    var volumeValue = volumeProperty.GetValue(boxed);
                    var timeValue = timeProperty?.GetValue(boxed);
                    
                    if (timeValue is DateTime dt)
                        timestamp = dt;
                    else if (timeValue is DateTimeOffset dto)
                        timestamp = dto.DateTime;
                    
                    return (
                        Convert.ToDecimal(highValue!),
                        Convert.ToDecimal(lowValue!),
                        Convert.ToDecimal(closeValue!),
                        Convert.ToDecimal(volumeValue!),
                        timestamp
                    );
                }
                
                throw new InvalidOperationException(
                    $"Unable to extract OHLCV data from input type {inputType.Name}. " +
                    "Input must have High, Low, Close and Volume properties.");
        }
    }

    /// <summary>
    /// Checks if VWAP should be reset based on the reset period and current time
    /// </summary>
    private void CheckAndPerformReset(DateTime currentTime)
    {
        bool shouldReset = false;

        switch (ResetPeriod)
        {
            case VWAPResetPeriod.Never:
                // Never reset
                break;
                
            case VWAPResetPeriod.Daily:
                shouldReset = ShouldResetDaily(currentTime);
                break;
                
            case VWAPResetPeriod.Weekly:
                shouldReset = ShouldResetWeekly(currentTime);
                break;
                
            case VWAPResetPeriod.Monthly:
                shouldReset = ShouldResetMonthly(currentTime);
                break;
                
            case VWAPResetPeriod.Custom:
                shouldReset = ShouldResetCustom(currentTime);
                break;
        }

        if (shouldReset)
        {
            PerformReset(currentTime);
        }
    }

    /// <summary>
    /// Determines if daily reset should occur
    /// </summary>
    private bool ShouldResetDaily(DateTime currentTime)
    {
        if (!lastResetTime.HasValue)
            return true; // First time, always reset
            
        return currentTime.Date > lastResetTime.Value.Date;
    }

    /// <summary>
    /// Determines if weekly reset should occur (Monday)
    /// </summary>
    private bool ShouldResetWeekly(DateTime currentTime)
    {
        if (!lastResetTime.HasValue)
            return true; // First time, always reset
            
        // Check if we've moved to a new week (Monday to Monday)
        var currentWeekStart = GetWeekStart(currentTime);
        var lastWeekStart = GetWeekStart(lastResetTime.Value);
        
        return currentWeekStart > lastWeekStart;
    }

    /// <summary>
    /// Determines if monthly reset should occur
    /// </summary>
    private bool ShouldResetMonthly(DateTime currentTime)
    {
        if (!lastResetTime.HasValue)
            return true; // First time, always reset
            
        return currentTime.Month != lastResetTime.Value.Month || 
               currentTime.Year != lastResetTime.Value.Year;
    }

    /// <summary>
    /// Determines if custom reset should occur
    /// </summary>
    private bool ShouldResetCustom(DateTime currentTime)
    {
        if (!Parameters.CustomResetTime.HasValue)
            return false; // No custom time set
            
        if (!lastResetTime.HasValue)
            return true; // First time, always reset
            
        var customTime = Parameters.CustomResetTime.Value;
        var lastResetDate = lastResetTime.Value.Date;
        var currentDate = currentTime.Date;
        
        // Check if we've passed the custom reset time today
        if (currentDate > lastResetDate)
        {
            var todayResetTime = currentDate.Add(customTime);
            return currentTime >= todayResetTime;
        }
        
        return false;
    }

    /// <summary>
    /// Gets the start of the week (Monday) for a given date
    /// </summary>
    private DateTime GetWeekStart(DateTime date)
    {
        int daysFromMonday = ((int)date.DayOfWeek + 6) % 7; // Monday = 0
        return date.Date.AddDays(-daysFromMonday);
    }

    /// <summary>
    /// Performs the actual reset of the QuantConnect indicator
    /// </summary>
    private void PerformReset(DateTime currentTime)
    {
        WrappedIndicator.Reset();
        hasReset = true;
        lastResetTime = currentTime;
        endTime = DefaultEndTime; // Reset time tracking
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
        lastResetTime = null;
        hasReset = false;
    }

    #endregion
}