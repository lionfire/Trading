using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of CCI (Commodity Channel Index) indicator.
/// Uses circular buffers for efficient O(1) calculation of the typical price average and mean deviation.
/// 
/// CCI Calculation:
/// 1. Typical Price (TP) = (High + Low + Close) / 3
/// 2. Simple Moving Average of TP over Period
/// 3. Mean Deviation = Average of |TP - SMA(TP)| over Period  
/// 4. CCI = (Current TP - SMA(TP)) / (Constant * Mean Deviation)
///    where Constant is typically 0.015
/// </summary>
public class CCI_FP<TPrice, TOutput> : SingleInputIndicatorBase<CCI_FP<TPrice, TOutput>, PCCI<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IIndicator2<CCI_FP<TPrice, TOutput>, PCCI<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    ICCI<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the CCI indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "CCI",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the CCI indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PCCI<TPrice, TOutput> p)
        => [new() {
                Name = "CCI",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The CCI parameters
    /// </summary>
    public readonly PCCI<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for CCI calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The constant used in CCI calculation (typically 0.015)
    /// </summary>
    public double Constant => Parameters.Constant;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new CCI indicator instance
    /// </summary>
    public static CCI_FP<TPrice, TOutput> Create(PCCI<TPrice, TOutput> p) => new CCI_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the CCI indicator
    /// </summary>
    public CCI_FP(PCCI<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        typicalPriceBuffer = new TOutput[parameters.Period];
        bufferIndex = 0;
        count = 0;
        typicalPriceSum = TOutput.Zero;
        constantAsOutput = TOutput.CreateChecked(parameters.Constant);
    }

    #endregion

    #region State

    private readonly TOutput[] typicalPriceBuffer;
    private int bufferIndex;
    private int count;
    private TOutput typicalPriceSum;
    private TOutput currentValue;
    private readonly TOutput constantAsOutput;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= Period;

    /// <summary>
    /// Gets the current CCI value
    /// </summary>
    public TOutput Value => IsReady ? currentValue : default(TOutput)!;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of HLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Calculate typical price: (High + Low + Close) / 3
            TOutput typicalPrice = CalculateTypicalPrice(input);

            // If buffer is full, subtract the oldest typical price from the sum
            if (count >= Period)
            {
                typicalPriceSum -= typicalPriceBuffer[bufferIndex];
            }

            // Add new typical price to buffer and sum
            typicalPriceBuffer[bufferIndex] = typicalPrice;
            typicalPriceSum += typicalPrice;

            // Move to next position in circular buffer
            bufferIndex = (bufferIndex + 1) % Period;

            // Increment count until we reach the period
            if (count < Period)
            {
                count++;
            }

            // Calculate and store the CCI value
            if (IsReady)
            {
                // Calculate SMA of typical prices
                TOutput smaTypicalPrice = typicalPriceSum / TOutput.CreateChecked(Period);
                
                // Calculate mean deviation
                TOutput meanDeviation = CalculateMeanDeviation(smaTypicalPrice);
                
                // Calculate CCI: (Current TP - SMA(TP)) / (Constant * Mean Deviation)
                if (meanDeviation != TOutput.Zero)
                {
                    currentValue = (typicalPrice - smaTypicalPrice) / (constantAsOutput * meanDeviation);
                }
                else
                {
                    // Handle edge case where mean deviation is zero
                    currentValue = TOutput.Zero;
                }
                
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
        Array.Clear(typicalPriceBuffer, 0, typicalPriceBuffer.Length);
        bufferIndex = 0;
        count = 0;
        typicalPriceSum = TOutput.Zero;
        currentValue = default(TOutput)!;
    }

    /// <summary>
    /// Calculate typical price from HLC data: (High + Low + Close) / 3
    /// </summary>
    private TOutput CalculateTypicalPrice(HLC<TPrice> hlc)
    {
        TOutput high = ConvertToOutput(hlc.High);
        TOutput low = ConvertToOutput(hlc.Low);
        TOutput close = ConvertToOutput(hlc.Close);
        
        TOutput three = TOutput.CreateChecked(3);
        return (high + low + close) / three;
    }

    /// <summary>
    /// Calculate the mean deviation of typical prices from their SMA
    /// </summary>
    private TOutput CalculateMeanDeviation(TOutput smaTypicalPrice)
    {
        TOutput deviationSum = TOutput.Zero;
        
        // Sum absolute deviations for all values in the buffer
        for (int i = 0; i < Math.Min(count, Period); i++)
        {
            TOutput deviation = typicalPriceBuffer[i] - smaTypicalPrice;
            // Take absolute value
            if (deviation < TOutput.Zero)
            {
                deviation = -deviation;
            }
            deviationSum += deviation;
        }
        
        // Return average deviation
        return deviationSum / TOutput.CreateChecked(Period);
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