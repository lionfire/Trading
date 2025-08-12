using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Choppiness Index using circular buffer
/// for optimal memory usage and streaming updates.
/// Choppiness Index = 100 × LOG10(Sum of True Range / Max Range) / LOG10(Period)
/// </summary>
public class ChoppinessIndex_FP<TInput, TOutput>
    : ChoppinessIndexBase<TInput, TOutput>
    , IIndicator2<ChoppinessIndex_FP<TInput, TOutput>, PChoppinessIndex<TInput, TOutput>, HLC<TInput>, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly TOutput[] trueRangeBuffer;
    private readonly TOutput[] highBuffer;
    private readonly TOutput[] lowBuffer;
    private int bufferIndex = 0;
    private int dataPointsReceived = 0;
    private TOutput currentChoppinessIndex;
    private HLC<TInput>? previousBar;
    private bool hasPreviousBar = false;
    
    // Running calculations for efficiency
    private TOutput sumTrueRange;
    private TOutput maxHigh;
    private TOutput minLow;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentChoppinessIndex;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    public override TOutput TrueRangeSum => sumTrueRange;

    public override TOutput MaxRange => maxHigh - minLow;

    #endregion

    #region Lifecycle

    public static ChoppinessIndex_FP<TInput, TOutput> Create(PChoppinessIndex<TInput, TOutput> p)
        => new ChoppinessIndex_FP<TInput, TOutput>(p);

    public ChoppinessIndex_FP(PChoppinessIndex<TInput, TOutput> parameters) : base(parameters)
    {
        trueRangeBuffer = new TOutput[Parameters.Period];
        highBuffer = new TOutput[Parameters.Period];
        lowBuffer = new TOutput[Parameters.Period];
        currentChoppinessIndex = TOutput.CreateChecked(50); // Default neutral value
        sumTrueRange = TOutput.Zero;
        maxHigh = TOutput.Zero;
        minLow = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TInput>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            var (high, low, close) = ExtractHLC(input);
            
            // Calculate True Range
            TOutput trueRange;
            if (hasPreviousBar)
            {
                trueRange = CalculateTrueRange(input, previousBar.Value);
            }
            else
            {
                trueRange = CalculateFirstTrueRange(input);
            }
            
            UpdateChoppinessIndex(trueRange, high, low);
            
            // Update state
            previousBar = input;
            hasPreviousBar = true;
            dataPointsReceived++;
            
            var outputValue = IsReady ? currentChoppinessIndex : MissingOutputValue;
            
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

    #region Choppiness Index Calculation

    private void UpdateChoppinessIndex(TOutput trueRange, TOutput high, TOutput low)
    {
        // If buffer is full, subtract the old values before adding new ones
        if (dataPointsReceived >= Parameters.Period)
        {
            var oldTrueRange = trueRangeBuffer[bufferIndex];
            var oldHigh = highBuffer[bufferIndex];
            var oldLow = lowBuffer[bufferIndex];
            
            // Update running sum of true range
            sumTrueRange = sumTrueRange - oldTrueRange + trueRange;
            
            // Update max/min by recalculating from the buffer
            // This is necessary because we might be removing the current max or min
            UpdateMaxMinAfterRemoval(oldHigh, oldLow, high, low);
        }
        else
        {
            // Still building up the initial period
            sumTrueRange += trueRange;
            
            // Update max/min for the growing period
            if (dataPointsReceived == 0)
            {
                maxHigh = high;
                minLow = low;
            }
            else
            {
                maxHigh = TOutput.Max(maxHigh, high);
                minLow = TOutput.Min(minLow, low);
            }
        }
        
        // Store new values in circular buffer
        trueRangeBuffer[bufferIndex] = trueRange;
        highBuffer[bufferIndex] = high;
        lowBuffer[bufferIndex] = low;
        
        // Advance buffer index (circular)
        bufferIndex = (bufferIndex + 1) % Parameters.Period;
        
        // Calculate Choppiness Index if we have enough data
        if (dataPointsReceived >= Parameters.Period)
        {
            var maxRange = maxHigh - minLow;
            currentChoppinessIndex = CalculateChoppinessIndex(sumTrueRange, maxRange);
        }
    }
    
    private void UpdateMaxMinAfterRemoval(TOutput oldHigh, TOutput oldLow, TOutput newHigh, TOutput newLow)
    {
        // If we're not removing the current max/min, just update with the new value
        if (oldHigh != maxHigh && oldLow != minLow)
        {
            maxHigh = TOutput.Max(maxHigh, newHigh);
            minLow = TOutput.Min(minLow, newLow);
        }
        else
        {
            // We need to recalculate max/min from the entire buffer
            maxHigh = newHigh;
            minLow = newLow;
            
            for (int i = 0; i < Parameters.Period; i++)
            {
                if (i != bufferIndex) // Skip the current position which will be updated with new values
                {
                    maxHigh = TOutput.Max(maxHigh, highBuffer[i]);
                    minLow = TOutput.Min(minLow, lowBuffer[i]);
                }
            }
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        // Clear buffers
        Array.Clear(trueRangeBuffer, 0, trueRangeBuffer.Length);
        Array.Clear(highBuffer, 0, highBuffer.Length);
        Array.Clear(lowBuffer, 0, lowBuffer.Length);
        
        // Reset state
        bufferIndex = 0;
        dataPointsReceived = 0;
        currentChoppinessIndex = TOutput.CreateChecked(50);
        previousBar = null;
        hasPreviousBar = false;
        sumTrueRange = TOutput.Zero;
        maxHigh = TOutput.Zero;
        minLow = TOutput.Zero;
    }

    #endregion
}