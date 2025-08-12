using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Volume Weighted Average Price (VWAP) indicator.
/// VWAP = Σ(Typical Price × Volume) / Σ(Volume)
/// Uses efficient cumulative calculation with configurable reset periods.
/// </summary>
public class VWAP_FP<TInput, TOutput> : SingleInputIndicatorBase<VWAP_FP<TInput, TOutput>, PVWAP<TInput, TOutput>, TInput, TOutput>,
    IIndicator2<VWAP_FP<TInput, TOutput>, PVWAP<TInput, TOutput>, TInput, TOutput>,
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
    public static VWAP_FP<TInput, TOutput> Create(PVWAP<TInput, TOutput> p) => new VWAP_FP<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the VWAP indicator
    /// </summary>
    public VWAP_FP(PVWAP<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        cumulativePriceVolume = TOutput.Zero;
        cumulativeVolume = TOutput.Zero;
        currentValue = TOutput.Zero;
        hasData = false;
        hasReset = false;
        lastResetTime = null;
    }

    #endregion

    #region State

    private TOutput cumulativePriceVolume;
    private TOutput cumulativeVolume;
    private TOutput currentValue;
    private bool hasData;
    private bool hasReset;
    private DateTime? lastResetTime;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => hasData && cumulativeVolume > TOutput.Zero;

    /// <summary>
    /// Gets the current VWAP value
    /// </summary>
    public TOutput Value => IsReady ? currentValue : TOutput.Zero;

    /// <summary>
    /// Gets the cumulative typical price × volume sum for the current period
    /// </summary>
    public TOutput CumulativePriceVolume => cumulativePriceVolume;

    /// <summary>
    /// Gets the cumulative volume for the current period
    /// </summary>
    public TOutput CumulativeVolume => cumulativeVolume;

    /// <summary>
    /// Gets a value indicating whether the VWAP has been reset for the current period
    /// </summary>
    public bool HasReset => hasReset;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract price, volume, and timestamp from input
            var (price, volume, timestamp) = ExtractPriceVolumeTimestamp(input);
            
            // Check if we need to reset based on the reset period
            CheckAndPerformReset(timestamp);
            
            // Calculate typical price if needed
            TOutput effectivePrice = UseTypicalPrice ? CalculateTypicalPrice(input) : price;
            
            // Update cumulative values
            TOutput priceVolume = effectivePrice * volume;
            cumulativePriceVolume += priceVolume;
            cumulativeVolume += volume;
            
            // Calculate VWAP
            if (cumulativeVolume > TOutput.Zero)
            {
                currentValue = cumulativePriceVolume / cumulativeVolume;
                hasData = true;
            }
            
            var outputValue = IsReady ? currentValue : TOutput.Zero;
            
            if (subject != null && IsReady)
            {
                subject.OnNext(new List<TOutput> { outputValue });
            }
            
            OnNext_PopulateOutput(outputValue, output, ref outputIndex, ref outputSkip);
            
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
    /// Extracts price, volume, and timestamp from the input data structure
    /// </summary>
    private (TOutput price, TOutput volume, DateTime timestamp) ExtractPriceVolumeTimestamp(TInput input)
    {
        DateTime timestamp = DateTime.UtcNow; // Default timestamp

        // Handle different input types
        switch (input)
        {
            case TimedBar timedBar:
                return (TOutput.CreateChecked(timedBar.Close), TOutput.CreateChecked(timedBar.Volume), 
                    timedBar.OpenTime.DateTime);
            
            default:
                // Try reflection for dynamic property access
                var inputType = typeof(TInput);
                var boxed = (object)input;
                
                // Look for Close, Volume, and Time/Timestamp properties
                var closeProperty = inputType.GetProperty("Close");
                var volumeProperty = inputType.GetProperty("Volume");
                var timeProperty = inputType.GetProperty("Time") ?? 
                                 inputType.GetProperty("Timestamp") ?? 
                                 inputType.GetProperty("DateTime");
                
                if (closeProperty != null && volumeProperty != null)
                {
                    var closeValue = closeProperty.GetValue(boxed);
                    var volumeValue = volumeProperty.GetValue(boxed);
                    var timeValue = timeProperty?.GetValue(boxed);
                    
                    if (timeValue is DateTime dt)
                        timestamp = dt;
                    else if (timeValue is DateTimeOffset dto)
                        timestamp = dto.DateTime;
                    
                    return (
                        TOutput.CreateChecked(Convert.ToDecimal(closeValue!)),
                        TOutput.CreateChecked(Convert.ToDecimal(volumeValue!)),
                        timestamp
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
                                TOutput.CreateChecked(Convert.ToDecimal(item1!)),
                                TOutput.CreateChecked(Convert.ToDecimal(item2!)),
                                timestamp
                            );
                        }
                    }
                }
                
                throw new InvalidOperationException(
                    $"Unable to extract price and volume from input type {inputType.Name}. " +
                    "Input must have Close and Volume properties or be a (price, volume) tuple.");
        }
    }

    /// <summary>
    /// Calculates typical price (H+L+C)/3 from input data
    /// </summary>
    private TOutput CalculateTypicalPrice(TInput input)
    {
        // Handle different input types for typical price calculation
        switch (input)
        {
            case TimedBar timedBar:
                return TOutput.CreateChecked((timedBar.High + timedBar.Low + timedBar.Close) / 3.0);
            
            default:
                // Try reflection for dynamic property access
                var inputType = typeof(TInput);
                var boxed = (object)input;
                
                var highProperty = inputType.GetProperty("High");
                var lowProperty = inputType.GetProperty("Low");
                var closeProperty = inputType.GetProperty("Close");
                
                if (highProperty != null && lowProperty != null && closeProperty != null)
                {
                    var highValue = highProperty.GetValue(boxed);
                    var lowValue = lowProperty.GetValue(boxed);
                    var closeValue = closeProperty.GetValue(boxed);
                    
                    var high = Convert.ToDecimal(highValue!);
                    var low = Convert.ToDecimal(lowValue!);
                    var close = Convert.ToDecimal(closeValue!);
                    
                    return TOutput.CreateChecked((high + low + close) / 3.0m);
                }
                
                // Fall back to close price if H/L/C not available
                var (price, _, _) = ExtractPriceVolumeTimestamp(input);
                return price;
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
    /// Performs the actual reset of cumulative values
    /// </summary>
    private void PerformReset(DateTime currentTime)
    {
        cumulativePriceVolume = TOutput.Zero;
        cumulativeVolume = TOutput.Zero;
        currentValue = TOutput.Zero;
        hasData = false;
        hasReset = true;
        lastResetTime = currentTime;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clears and resets the indicator state
    /// </summary>
    public override void Clear() 
    { 
        base.Clear();
        cumulativePriceVolume = TOutput.Zero;
        cumulativeVolume = TOutput.Zero;
        currentValue = TOutput.Zero;
        hasData = false;
        hasReset = false;
        lastResetTime = null;
    }

    #endregion
}