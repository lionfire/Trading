using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Hull Moving Average (HMA) indicator.
/// Uses efficient Weighted Moving Average calculations with optimized circular buffers.
/// 
/// Hull Moving Average calculation:
/// 1. WMA1 = WMA(Price, Period/2) - Fast WMA
/// 2. WMA2 = WMA(Price, Period) - Slow WMA  
/// 3. Raw HMA values = 2 × WMA1 - WMA2
/// 4. Final HMA = WMA(Raw HMA values, SQRT(Period))
/// </summary>
public class HullMovingAverage_FP<TPrice, TOutput> : SingleInputIndicatorBase<HullMovingAverage_FP<TPrice, TOutput>, PHullMovingAverage<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<HullMovingAverage_FP<TPrice, TOutput>, PHullMovingAverage<TPrice, TOutput>, TPrice, TOutput>,
    IHullMovingAverage<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Hull Moving Average indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "HMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Hull Moving Average indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PHullMovingAverage<TPrice, TOutput> p)
        => [new() {
                Name = "HMA",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The HMA parameters
    /// </summary>
    public readonly PHullMovingAverage<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Hull Moving Average calculation
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
    /// Creates a new Hull Moving Average indicator instance
    /// </summary>
    public static HullMovingAverage_FP<TPrice, TOutput> Create(PHullMovingAverage<TPrice, TOutput> p) => new HullMovingAverage_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Hull Moving Average indicator
    /// </summary>
    public HullMovingAverage_FP(PHullMovingAverage<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        
        // Initialize WMA calculators
        wma1 = new WeightedMovingAverageHelper<TOutput>(Parameters.WMA1Period);
        wma2 = new WeightedMovingAverageHelper<TOutput>(Parameters.WMA2Period);
        hullWma = new WeightedMovingAverageHelper<TOutput>(Parameters.HullWMAPeriod);
        
        currentValue = default(TOutput)!;
    }

    #endregion

    #region State

    private readonly WeightedMovingAverageHelper<TOutput> wma1; // Fast WMA (Period/2)
    private readonly WeightedMovingAverageHelper<TOutput> wma2; // Slow WMA (Period)
    private readonly WeightedMovingAverageHelper<TOutput> hullWma; // Final Hull WMA (SQRT(Period))
    private TOutput currentValue;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => hullWma.IsReady;

    /// <summary>
    /// Gets the current Hull Moving Average value
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

            // Update both WMA calculators with the new input
            wma1.Update(value);
            wma2.Update(value);

            // Calculate raw Hull value if both WMAs have enough data
            if (wma1.IsReady && wma2.IsReady)
            {
                // Raw HMA = 2 × WMA1 - WMA2
                var two = TOutput.CreateChecked(2);
                var rawHullValue = two * wma1.Value - wma2.Value;
                
                // Update the final Hull WMA with the raw value
                hullWma.Update(rawHullValue);
                
                // Store the result if ready
                if (hullWma.IsReady)
                {
                    currentValue = hullWma.Value;
                    
                    if (subject != null)
                    {
                        subject.OnNext(new List<TOutput> { currentValue });
                    }
                }
            }

            // Output the current value (or default if not ready)
            var outputValue = IsReady ? currentValue : default(TOutput)!;
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

    #region Methods

    /// <summary>
    /// Clears and resets the indicator state
    /// </summary>
    public override void Clear() 
    { 
        base.Clear();
        wma1.Clear();
        wma2.Clear();
        hullWma.Clear();
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