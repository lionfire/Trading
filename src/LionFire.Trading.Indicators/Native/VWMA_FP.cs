using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Volume Weighted Moving Average (VWMA) indicator.
/// VWMA = Σ(Price × Volume) / Σ(Volume) over the specified period
/// Uses circular buffers for efficient O(1) calculation of the moving average.
/// </summary>
public class VWMA_FP<TInput, TOutput> : SingleInputIndicatorBase<VWMA_FP<TInput, TOutput>, PVWMA<TInput, TOutput>, TInput, TOutput>,
    IIndicator2<VWMA_FP<TInput, TOutput>, PVWMA<TInput, TOutput>, TInput, TOutput>,
    IVWMA<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the VWMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "VWMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the VWMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PVWMA<TInput, TOutput> p)
        => [new() {
                Name = "VWMA",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The VWMA parameters
    /// </summary>
    public readonly PVWMA<TInput, TOutput> Parameters;

    /// <summary>
    /// The period used for VWMA calculation
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
    /// Creates a new VWMA indicator instance
    /// </summary>
    public static VWMA_FP<TInput, TOutput> Create(PVWMA<TInput, TOutput> p) => new VWMA_FP<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the VWMA indicator
    /// </summary>
    public VWMA_FP(PVWMA<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        priceVolumeBuffer = new TOutput[parameters.Period];
        volumeBuffer = new TOutput[parameters.Period];
        bufferIndex = 0;
        count = 0;
        priceVolumeSum = TOutput.Zero;
        volumeSum = TOutput.Zero;
        currentValue = TOutput.Zero;
    }

    #endregion

    #region State

    private readonly TOutput[] priceVolumeBuffer;
    private readonly TOutput[] volumeBuffer;
    private int bufferIndex;
    private int count;
    private TOutput priceVolumeSum;
    private TOutput volumeSum;
    private TOutput currentValue;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= Period && volumeSum > TOutput.Zero;

    /// <summary>
    /// Gets the current VWMA value
    /// </summary>
    public TOutput Value => IsReady ? currentValue : TOutput.Zero;

    /// <summary>
    /// Gets the sum of (price × volume) for the current period
    /// </summary>
    public TOutput PriceVolumeSum => priceVolumeSum;

    /// <summary>
    /// Gets the sum of volumes for the current period
    /// </summary>
    public TOutput VolumeSum => volumeSum;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract price and volume from input
            var (price, volume) = ExtractPriceAndVolume(input);
            
            // Calculate price × volume
            TOutput priceVolume = price * volume;

            // If buffer is full, subtract the oldest values from the sums
            if (count >= Period)
            {
                priceVolumeSum -= priceVolumeBuffer[bufferIndex];
                volumeSum -= volumeBuffer[bufferIndex];
            }

            // Add new values to buffer and sums
            priceVolumeBuffer[bufferIndex] = priceVolume;
            volumeBuffer[bufferIndex] = volume;
            priceVolumeSum += priceVolume;
            volumeSum += volume;

            // Move to next position in circular buffer
            bufferIndex = (bufferIndex + 1) % Period;

            // Increment count until we reach the period
            if (count < Period)
            {
                count++;
            }

            // Calculate and store the VWMA
            if (IsReady)
            {
                currentValue = priceVolumeSum / volumeSum;
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { currentValue });
                }
            }

            // Output value (zero if not ready)
            var outputValue = IsReady ? currentValue : TOutput.Zero;
            OnNext_PopulateOutput(outputValue, output, ref outputIndex, ref outputSkip);
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
    private (TOutput price, TOutput volume) ExtractPriceAndVolume(TInput input)
    {
        // Handle different input types
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Try reflection for dynamic property access
        var closeProperty = inputType.GetProperty("Close");
        var volumeProperty = inputType.GetProperty("Volume");
        
        if (closeProperty != null && volumeProperty != null)
        {
            var closeValue = closeProperty.GetValue(boxed);
            var volumeValue = volumeProperty.GetValue(boxed);
            
            return (
                ConvertToOutput(closeValue!),
                ConvertToOutput(volumeValue!)
            );
        }
        
        // Try Price/Volume properties
        var priceProperty = inputType.GetProperty("Price");
        volumeProperty = inputType.GetProperty("Volume");
        
        if (priceProperty != null && volumeProperty != null)
        {
            var priceValue = priceProperty.GetValue(boxed);
            var volumeValue = volumeProperty.GetValue(boxed);
            
            return (
                ConvertToOutput(priceValue!),
                ConvertToOutput(volumeValue!)
            );
        }
        
        // If input is a tuple-like structure, try to extract fields
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
                        ConvertToOutput(item1!),
                        ConvertToOutput(item2!)
                    );
                }
                
                // Try properties Item1, Item2
                var item1Property = inputType.GetProperty("Item1");
                var item2Property = inputType.GetProperty("Item2");
                
                if (item1Property != null && item2Property != null)
                {
                    var item1 = item1Property.GetValue(boxed);
                    var item2 = item2Property.GetValue(boxed);
                    
                    return (
                        ConvertToOutput(item1!),
                        ConvertToOutput(item2!)
                    );
                }
            }
        }
        
        throw new InvalidOperationException(
            $"Unable to extract price and volume from input type {inputType.Name}. " +
            "Input must have Close/Volume or Price/Volume properties, or be a (price, volume) tuple.");
    }

    /// <summary>
    /// Convert input type to output type
    /// </summary>
    private static TOutput ConvertToOutput(object input)
    {
        // Handle common conversions efficiently
        if (input is TOutput directOutput)
        {
            return directOutput;
        }
        
        return input switch
        {
            double d => TOutput.CreateChecked(d),
            float f => TOutput.CreateChecked(f),
            decimal m => TOutput.CreateChecked(m),
            int i => TOutput.CreateChecked(i),
            long l => TOutput.CreateChecked(l),
            _ => TOutput.CreateChecked(Convert.ToDouble(input))
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clears and resets the indicator state
    /// </summary>
    public override void Clear() 
    { 
        base.Clear();
        Array.Clear(priceVolumeBuffer, 0, priceVolumeBuffer.Length);
        Array.Clear(volumeBuffer, 0, volumeBuffer.Length);
        bufferIndex = 0;
        count = 0;
        priceVolumeSum = TOutput.Zero;
        volumeSum = TOutput.Zero;
        currentValue = TOutput.Zero;
    }

    #endregion
}