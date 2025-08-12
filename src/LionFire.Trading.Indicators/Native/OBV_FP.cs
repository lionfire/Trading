using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of On Balance Volume (OBV) indicator
/// OBV tracks cumulative volume flow based on price direction
/// </summary>
public class OBV_FP<TInput, TOutput>
    : OBVBase<TInput, TOutput>
    , IIndicator2<OBV_FP<TInput, TOutput>, POBV<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private TOutput cumulativeVolume = TOutput.Zero;
    private TOutput lastChange = TOutput.Zero;
    private TOutput? previousPrice;
    private bool hasData = false;

    #endregion

    #region Properties

    public override TOutput CurrentValue => cumulativeVolume;
    
    public override TOutput LastChange => lastChange;
    
    public override bool IsReady => hasData;

    #endregion

    #region Lifecycle

    public static OBV_FP<TInput, TOutput> Create(POBV<TInput, TOutput> p)
        => new OBV_FP<TInput, TOutput>(p);

    public OBV_FP(POBV<TInput, TOutput> parameters) : base(parameters)
    {
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract price and volume from input
            // This implementation assumes TInput has Close and Volume properties
            // For more flexibility, we could use reflection or interfaces
            var (price, volume) = ExtractPriceAndVolume(input);
            
            if (previousPrice.HasValue)
            {
                // Calculate OBV change based on price direction
                TOutput volumeChange = TOutput.Zero;
                
                if (price > previousPrice.Value)
                {
                    // Price went up - add volume (buying pressure)
                    volumeChange = volume;
                }
                else if (price < previousPrice.Value)
                {
                    // Price went down - subtract volume (selling pressure)
                    volumeChange = -volume;
                }
                // If price unchanged, volume change is 0
                
                lastChange = volumeChange;
                cumulativeVolume += volumeChange;
            }
            else
            {
                // First data point - initialize OBV with volume
                cumulativeVolume = volume;
                lastChange = TOutput.Zero;
            }
            
            previousPrice = price;
            hasData = true;
            
            var outputValue = IsReady ? cumulativeVolume : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length)
            {
                output[outputIndex++] = outputValue;
            }
        }
        
        // Notify observers if any
        if (subject != null && output != null && outputIndex > 0)
        {
            var results = new TOutput[outputIndex];
            Array.Copy(output, results, outputIndex);
            subject.OnNext(results);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extracts price and volume from the input data structure
    /// This method handles different input types (Bar, TimedBar, PriceVolume, etc.)
    /// </summary>
    private (TOutput price, TOutput volume) ExtractPriceAndVolume(TInput input)
    {
        // Handle different input types
        switch (input)
        {
            case TimedBar timedBar:
                return (TOutput.CreateChecked(timedBar.Close), TOutput.CreateChecked(timedBar.Volume));
            
            default:
                // Try reflection for dynamic property access
                var inputType = typeof(TInput);
                
                // Look for Close and Volume properties
                var closeProperty = inputType.GetProperty("Close");
                var volumeProperty = inputType.GetProperty("Volume");
                
                if (closeProperty != null && volumeProperty != null)
                {
                    var boxed = (object)input;
                    var closeValue = closeProperty.GetValue(boxed);
                    var volumeValue = volumeProperty.GetValue(boxed);
                    
                    return (
                        TOutput.CreateChecked(Convert.ToDecimal(closeValue!)),
                        TOutput.CreateChecked(Convert.ToDecimal(volumeValue!))
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
                            var boxed = (object)input;
                            var item1 = fields[0].GetValue(boxed);
                            var item2 = fields[1].GetValue(boxed);
                            
                            return (
                                TOutput.CreateChecked(Convert.ToDecimal(item1!)),
                                TOutput.CreateChecked(Convert.ToDecimal(item2!))
                            );
                        }
                    }
                }
                
                throw new InvalidOperationException(
                    $"Unable to extract price and volume from input type {inputType.Name}. " +
                    "Input must have Close and Volume properties or be a (price, volume) tuple.");
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        cumulativeVolume = TOutput.Zero;
        lastChange = TOutput.Zero;
        previousPrice = null;
        hasData = false;
    }

    #endregion
}