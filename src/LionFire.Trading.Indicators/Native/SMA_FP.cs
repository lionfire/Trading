using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Simple Moving Average (SMA) indicator.
/// Uses a circular buffer for efficient O(1) calculation of the moving average.
/// </summary>
public class SMA_FP<TPrice, TOutput> : SingleInputIndicatorBase<SMA_FP<TPrice, TOutput>, PSMA<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<SMA_FP<TPrice, TOutput>, PSMA<TPrice, TOutput>, TPrice, TOutput>,
    ISMA<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the SMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "SMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the SMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PSMA<TPrice, TOutput> p)
        => [new() {
                Name = "SMA",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The SMA parameters
    /// </summary>
    public readonly PSMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for SMA calculation
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
    /// Creates a new SMA indicator instance
    /// </summary>
    public static SMA_FP<TPrice, TOutput> Create(PSMA<TPrice, TOutput> p) => new SMA_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the SMA indicator
    /// </summary>
    public SMA_FP(PSMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        buffer = new TOutput[parameters.Period];
        bufferIndex = 0;
        count = 0;
        sum = TOutput.Zero;
    }

    #endregion

    #region State

    private readonly TOutput[] buffer;
    private int bufferIndex;
    private int count;
    private TOutput sum;
    private TOutput currentValue;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= Period;

    /// <summary>
    /// Gets the current SMA value
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

            // If buffer is full, subtract the oldest value from the sum
            if (count >= Period)
            {
                sum -= buffer[bufferIndex];
            }

            // Add new value to buffer and sum
            buffer[bufferIndex] = value;
            sum += value;

            // Move to next position in circular buffer
            bufferIndex = (bufferIndex + 1) % Period;

            // Increment count until we reach the period
            if (count < Period)
            {
                count++;
            }

            // Calculate and store the average
            if (IsReady)
            {
                currentValue = sum / TOutput.CreateChecked(Period);
                
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
        sum = TOutput.Zero;
        currentValue = default(TOutput)!;
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