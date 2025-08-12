using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Standard Deviation indicator.
/// Uses Welford's algorithm for numerically stable calculation of variance and standard deviation.
/// </summary>
public class StandardDeviation_FP<TPrice, TOutput> : SingleInputIndicatorBase<StandardDeviation_FP<TPrice, TOutput>, PStandardDeviation<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<StandardDeviation_FP<TPrice, TOutput>, PStandardDeviation<TPrice, TOutput>, TPrice, TOutput>,
    IStandardDeviation<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Standard Deviation indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "StdDev",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Standard Deviation indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PStandardDeviation<TPrice, TOutput> p)
        => [new() {
                Name = "StdDev",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The Standard Deviation parameters
    /// </summary>
    public readonly PStandardDeviation<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Standard Deviation calculation
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
    /// Creates a new Standard Deviation indicator instance
    /// </summary>
    public static StandardDeviation_FP<TPrice, TOutput> Create(PStandardDeviation<TPrice, TOutput> p) => new StandardDeviation_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Standard Deviation indicator
    /// </summary>
    public StandardDeviation_FP(PStandardDeviation<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        buffer = new TOutput[parameters.Period];
        bufferIndex = 0;
        count = 0;
        mean = TOutput.Zero;
        m2 = TOutput.Zero;
        currentValue = TOutput.Zero;
    }

    #endregion

    #region State

    private readonly TOutput[] buffer;
    private int bufferIndex;
    private int count;
    
    // Welford's algorithm state variables
    private TOutput mean;      // Running mean
    private TOutput m2;        // Running sum of squares of differences from current mean
    private TOutput currentValue;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= Period;

    /// <summary>
    /// Gets the current Standard Deviation value
    /// </summary>
    public TOutput Value => IsReady ? currentValue : default(TOutput)!;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput
            TOutput value = ConvertToOutput(input);
            
            // Store old value if buffer is full (for removing from calculation)
            TOutput oldValue = default(TOutput)!;
            bool hasOldValue = count >= Period;
            
            if (hasOldValue)
            {
                oldValue = buffer[bufferIndex];
            }

            // Store new value in circular buffer
            buffer[bufferIndex] = value;
            bufferIndex = (bufferIndex + 1) % Period;

            // Update count
            if (count < Period)
            {
                count++;
            }

            // Calculate standard deviation using online algorithm
            if (count == 1)
            {
                // First value
                mean = value;
                m2 = TOutput.Zero;
                currentValue = TOutput.Zero;
            }
            else if (!hasOldValue)
            {
                // Adding values until buffer is full - use Welford's online algorithm
                TOutput delta = value - mean;
                mean += delta / TOutput.CreateChecked(count);
                TOutput delta2 = value - mean;
                m2 += delta * delta2;
                
                if (count >= Period)
                {
                    // Calculate standard deviation: sqrt(m2 / n)
                    TOutput variance = m2 / TOutput.CreateChecked(Period);
                    currentValue = TOutput.CreateChecked(Math.Sqrt(Convert.ToDouble(variance)));
                }
            }
            else
            {
                // Buffer is full - need to remove old value and add new value
                // This is more complex with Welford's algorithm, so we recalculate
                // For efficiency, we could implement a sliding window Welford's algorithm
                // But for simplicity and correctness, we'll recalculate from buffer
                RecalculateFromBuffer();
            }

            // Output values
            if (IsReady)
            {
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { currentValue });
                }
                
                OnNext_PopulateOutput(currentValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Recalculate standard deviation from the current buffer contents
    /// Uses the traditional two-pass algorithm for accuracy
    /// </summary>
    private void RecalculateFromBuffer()
    {
        if (count < Period) return;

        // First pass: calculate mean
        TOutput sum = TOutput.Zero;
        for (int i = 0; i < Period; i++)
        {
            sum += buffer[i];
        }
        mean = sum / TOutput.CreateChecked(Period);

        // Second pass: calculate sum of squared differences
        TOutput sumSquaredDiffs = TOutput.Zero;
        for (int i = 0; i < Period; i++)
        {
            TOutput diff = buffer[i] - mean;
            sumSquaredDiffs += diff * diff;
        }

        // Calculate standard deviation
        TOutput variance = sumSquaredDiffs / TOutput.CreateChecked(Period);
        currentValue = TOutput.CreateChecked(Math.Sqrt(Convert.ToDouble(variance)));
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
        Array.Clear(buffer, 0, buffer.Length);
        bufferIndex = 0;
        count = 0;
        mean = TOutput.Zero;
        m2 = TOutput.Zero;
        currentValue = TOutput.Zero;
    }

    /// <summary>
    /// Convert input type to output type
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
}