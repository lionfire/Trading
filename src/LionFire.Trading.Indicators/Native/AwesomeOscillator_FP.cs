using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Utils;
using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.Indicators;
using System.Numerics;

/// <summary>
/// First-party implementation of Awesome Oscillator (AO) indicator.
/// Uses circular buffers for efficient O(1) calculation of the moving averages.
/// AO = SMA(Median Price, FastPeriod) - SMA(Median Price, SlowPeriod)
/// where Median Price = (High + Low) / 2
/// </summary>
public class AwesomeOscillator_FP<TPrice, TOutput> 
    : AwesomeOscillatorBase<AwesomeOscillator_FP<TPrice, TOutput>, TPrice, TOutput>
    , IIndicator2<AwesomeOscillator_FP<TPrice, TOutput>, PAwesomeOscillator<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Awesome Oscillator indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "AO",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Awesome Oscillator indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PAwesomeOscillator<TPrice, TOutput> p)
        => [new() {
                Name = "AO",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Fields

    // Circular buffers for SMA calculations
    private readonly CircularBuffer<TOutput> fastBuffer;
    private readonly CircularBuffer<TOutput> slowBuffer;
    
    // Running sums for efficient SMA calculation
    private TOutput fastSum;
    private TOutput slowSum;
    
    // Current values
    private TOutput currentValue;
    private TOutput fastSMA;
    private TOutput slowSMA;
    
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput Value => IsReady ? currentValue : default(TOutput)!;
    
    public override bool IsReady => dataPointsReceived >= Parameters.SlowPeriod;

    /// <summary>
    /// Current fast SMA value (for debugging/inspection purposes)
    /// </summary>
    public TOutput FastSMA => dataPointsReceived >= Parameters.FastPeriod ? fastSMA : default(TOutput)!;
    
    /// <summary>
    /// Current slow SMA value (for debugging/inspection purposes)
    /// </summary>
    public TOutput SlowSMA => IsReady ? slowSMA : default(TOutput)!;

    #endregion

    #region Lifecycle

    public static AwesomeOscillator_FP<TPrice, TOutput> Create(PAwesomeOscillator<TPrice, TOutput> p)
        => new AwesomeOscillator_FP<TPrice, TOutput>(p);

    public AwesomeOscillator_FP(PAwesomeOscillator<TPrice, TOutput> parameters) : base(parameters)
    {
        // Initialize circular buffers
        fastBuffer = new CircularBuffer<TOutput>(parameters.FastPeriod);
        slowBuffer = new CircularBuffer<TOutput>(parameters.SlowPeriod);
        
        // Initialize sums
        fastSum = TOutput.Zero;
        slowSum = TOutput.Zero;
        currentValue = TOutput.Zero;
        fastSMA = TOutput.Zero;
        slowSMA = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Calculate median price: (High + Low) / 2
            var high = ConvertToOutput(input.High);
            var low = ConvertToOutput(input.Low);
            var medianPrice = (high + low) / TOutput.CreateChecked(2);
            
            dataPointsReceived++;
            
            // Update fast SMA (always)
            UpdateFastSMA(medianPrice);
            
            // Update slow SMA (always)
            UpdateSlowSMA(medianPrice);
            
            // Calculate Awesome Oscillator value when we have enough data
            if (IsReady)
            {
                currentValue = fastSMA - slowSMA;
            }
            
            // Output values
            var outputValue = IsReady ? currentValue : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length)
            {
                output[outputIndex++] = outputValue;
            }
            
            // Notify observers
            if (subject != null && IsReady)
            {
                subject.OnNext(new List<TOutput> { currentValue });
            }
        }
    }

    #endregion

    #region SMA Calculations

    /// <summary>
    /// Update the fast SMA using circular buffer for O(1) performance
    /// </summary>
    private void UpdateFastSMA(TOutput medianPrice)
    {
        // If buffer is full, subtract the oldest value from the sum
        if (fastBuffer.Count >= Parameters.FastPeriod)
        {
            fastSum -= fastBuffer.Front;
        }
        
        // Add new value to buffer and sum
        fastBuffer.Add(medianPrice);
        fastSum += medianPrice;
        
        // Calculate average if we have enough data points
        if (fastBuffer.Count >= Parameters.FastPeriod)
        {
            fastSMA = fastSum / TOutput.CreateChecked(Parameters.FastPeriod);
        }
    }
    
    /// <summary>
    /// Update the slow SMA using circular buffer for O(1) performance
    /// </summary>
    private void UpdateSlowSMA(TOutput medianPrice)
    {
        // If buffer is full, subtract the oldest value from the sum
        if (slowBuffer.Count >= Parameters.SlowPeriod)
        {
            slowSum -= slowBuffer.Front;
        }
        
        // Add new value to buffer and sum
        slowBuffer.Add(medianPrice);
        slowSum += medianPrice;
        
        // Calculate average if we have enough data points
        if (slowBuffer.Count >= Parameters.SlowPeriod)
        {
            slowSMA = slowSum / TOutput.CreateChecked(Parameters.SlowPeriod);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Convert input type to output type efficiently
    /// </summary>
    private static TOutput ConvertToOutput(TPrice input)
    {
        // Handle common conversions efficiently
        if (typeof(TPrice) == typeof(TOutput))
        {
            return (TOutput)(object)input;
        }
        else if (typeof(TPrice) == typeof(double) && typeof(TOutput) == typeof(double))
        {
            return (TOutput)(object)input;
        }
        else if (typeof(TPrice) == typeof(float) && typeof(TOutput) == typeof(float))
        {
            return (TOutput)(object)input;
        }
        else if (typeof(TPrice) == typeof(decimal) && typeof(TOutput) == typeof(decimal))
        {
            return (TOutput)(object)input;
        }
        else
        {
            // Use generic conversion for other types
            return TOutput.CreateChecked(Convert.ToDouble(input));
        }
    }

    #endregion

    #region Cleanup

    public override void Clear()
    {
        base.Clear();
        
        // Clear buffers
        fastBuffer.Clear();
        slowBuffer.Clear();
        
        // Reset sums and values
        fastSum = TOutput.Zero;
        slowSum = TOutput.Zero;
        currentValue = TOutput.Zero;
        fastSMA = TOutput.Zero;
        slowSMA = TOutput.Zero;
        dataPointsReceived = 0;
    }

    #endregion
}