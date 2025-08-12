using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Volume Weighted Moving Average (VWMA)
/// Wraps the QuantConnect VolumeWeightedMovingAverage indicator with LionFire interfaces
/// </summary>
public class VWMA_QC<TInput, TOutput> 
    : VWMABase<VWMA_QC<TInput, TOutput>, TInput, TOutput>
    , IIndicator2<VWMA_QC<TInput, TOutput>, PVWMA<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly dynamic? qcIndicator;
    private TOutput currentValue;
    private TOutput priceVolumeSum;
    private TOutput volumeSum;
    private int dataPointsReceived = 0;
    private readonly bool isQuantConnectAvailable;

    #endregion

    #region Properties

    public override TOutput Value => IsReady ? currentValue : TOutput.Zero;
    
    public override TOutput PriceVolumeSum => priceVolumeSum;
    
    public override TOutput VolumeSum => volumeSum;
    
    public override bool IsReady => isQuantConnectAvailable 
        ? (qcIndicator?.IsReady == true) 
        : dataPointsReceived >= MaxLookback;

    #endregion

    #region Lifecycle

    public static VWMA_QC<TInput, TOutput> Create(PVWMA<TInput, TOutput> p)
        => new VWMA_QC<TInput, TOutput>(p);

    public VWMA_QC(PVWMA<TInput, TOutput> parameters) : base(parameters)
    {
        try
        {
            // Try to create QuantConnect VolumeWeightedMovingAverage indicator
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            var vwmaType = qcAssembly.GetType("QuantConnect.Indicators.VolumeWeightedMovingAverage");
            
            if (vwmaType != null)
            {
                qcIndicator = Activator.CreateInstance(vwmaType, 
                    $"VWMA({parameters.Period})", 
                    parameters.Period);
                isQuantConnectAvailable = true;
            }
            else
            {
                // QuantConnect doesn't have VolumeWeightedMovingAverage, use fallback
                isQuantConnectAvailable = false;
            }
        }
        catch
        {
            // Fall back to manual calculation if QuantConnect is not available
            isQuantConnectAvailable = false;
        }
        
        // Initialize state
        currentValue = TOutput.Zero;
        priceVolumeSum = TOutput.Zero;
        volumeSum = TOutput.Zero;
        
        // If QuantConnect is not available, we'll need to implement the calculation manually
        if (!isQuantConnectAvailable)
        {
            priceVolumeBuffer = new TOutput[parameters.Period];
            volumeBuffer = new TOutput[parameters.Period];
            bufferIndex = 0;
            count = 0;
        }
    }

    #endregion

    #region Manual calculation fields (when QuantConnect not available)
    
    private readonly TOutput[]? priceVolumeBuffer;
    private readonly TOutput[]? volumeBuffer;
    private int bufferIndex;
    private int count;

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            if (isQuantConnectAvailable && qcIndicator != null)
            {
                ProcessQuantConnectInput(input);
            }
            else
            {
                ProcessManualInput(input);
            }
            
            dataPointsReceived++;
            
            // Output values
            var outputValue = IsReady ? currentValue : TOutput.Zero;
            
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

    #region QuantConnect Processing

    private void ProcessQuantConnectInput(TInput input)
    {
        try
        {
            // Extract price and volume
            var (price, volume) = ExtractPriceAndVolume(input);
            
            // Create QuantConnect data point with price and volume
            var qcDataPoint = CreateQuantConnectVolumeDataPoint(ConvertToDecimal(price), ConvertToDecimal(volume));
            
            // Update the QuantConnect indicator
            qcIndicator!.Update(qcDataPoint);
            
            // Get the result and convert to TOutput
            if (qcIndicator.IsReady)
            {
                var qcValue = (decimal)qcIndicator.Current.Value;
                currentValue = TOutput.CreateChecked(qcValue);
                
                // Try to get internal sums if available (might not be exposed)
                try
                {
                    priceVolumeSum = TOutput.CreateChecked((decimal)(qcIndicator.PriceVolumeSum ?? 0m));
                    volumeSum = TOutput.CreateChecked((decimal)(qcIndicator.VolumeSum ?? 0m));
                }
                catch
                {
                    // Internal sums not available, keep current values
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error processing input in VWMA_QC: {ex.Message}", ex);
        }
    }

    #endregion

    #region Manual Processing (Fallback)

    private void ProcessManualInput(TInput input)
    {
        // Extract price and volume from input
        var (price, volume) = ExtractPriceAndVolume(input);
        
        // Calculate price × volume
        TOutput priceVolume = price * volume;

        // If buffer is full, subtract the oldest values from the sums
        if (count >= Period)
        {
            priceVolumeSum -= priceVolumeBuffer![bufferIndex];
            volumeSum -= volumeBuffer![bufferIndex];
        }

        // Add new values to buffer and sums
        priceVolumeBuffer![bufferIndex] = priceVolume;
        volumeBuffer![bufferIndex] = volume;
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
        if (count >= Period && volumeSum > TOutput.Zero)
        {
            currentValue = priceVolumeSum / volumeSum;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extracts price and volume from the input data structure
    /// </summary>
    private (TOutput price, TOutput volume) ExtractPriceAndVolume(TInput input)
    {
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Try Close/Volume properties first
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
        
        // If input is a tuple-like structure
        if (inputType.IsGenericType)
        {
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
        
        throw new InvalidOperationException(
            $"Unable to extract price and volume from input type {inputType.Name}. " +
            "Input must have Close/Volume or Price/Volume properties, or be a (price, volume) tuple.");
    }

    /// <summary>
    /// Convert input type to output type
    /// </summary>
    private static TOutput ConvertToOutput(object input)
    {
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

    /// <summary>
    /// Convert input type to decimal for QuantConnect processing
    /// </summary>
    private static decimal ConvertToDecimal(TOutput input)
    {
        return input switch
        {
            decimal d => d,
            double db => (decimal)db,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => Convert.ToDecimal(input)
        };
    }

    /// <summary>
    /// Create a QuantConnect-compatible data point object with volume
    /// </summary>
    private dynamic CreateQuantConnectVolumeDataPoint(decimal price, decimal volume)
    {
        try
        {
            // Try to create a TradeBar or similar volume-aware data point
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Common");
            var tradeBarType = qcAssembly.GetType("QuantConnect.Data.Market.TradeBar");
            
            if (tradeBarType != null)
            {
                var now = DateTime.UtcNow;
                // Create minimal TradeBar: symbol, time, open, high, low, close, volume
                var tradeBar = Activator.CreateInstance(tradeBarType);
                
                // Set properties using reflection
                SetProperty(tradeBar, "Time", now);
                SetProperty(tradeBar, "Open", price);
                SetProperty(tradeBar, "High", price);
                SetProperty(tradeBar, "Low", price);
                SetProperty(tradeBar, "Close", price);
                SetProperty(tradeBar, "Volume", volume);
                
                return tradeBar;
            }
            
            // Fallback: create anonymous object
            return new { 
                Time = DateTime.UtcNow, 
                Value = price, 
                Volume = volume,
                Close = price,
                Open = price,
                High = price,
                Low = price
            };
        }
        catch
        {
            // Final fallback
            return new { 
                Time = DateTime.UtcNow, 
                Value = price, 
                Volume = volume 
            };
        }
    }

    /// <summary>
    /// Helper to set properties via reflection
    /// </summary>
    private static void SetProperty(object obj, string propertyName, object value)
    {
        try
        {
            var property = obj.GetType().GetProperty(propertyName);
            property?.SetValue(obj, value);
        }
        catch
        {
            // Ignore if property doesn't exist or can't be set
        }
    }

    #endregion

    #region Cleanup

    public override void Clear()
    {
        base.Clear();
        qcIndicator?.Reset();
        currentValue = TOutput.Zero;
        priceVolumeSum = TOutput.Zero;
        volumeSum = TOutput.Zero;
        dataPointsReceived = 0;
        
        if (!isQuantConnectAvailable)
        {
            if (priceVolumeBuffer != null)
                Array.Clear(priceVolumeBuffer, 0, priceVolumeBuffer.Length);
            if (volumeBuffer != null)
                Array.Clear(volumeBuffer, 0, volumeBuffer.Length);
            bufferIndex = 0;
            count = 0;
        }
    }

    #endregion
}