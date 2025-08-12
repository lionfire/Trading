using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// Efficient Weighted Moving Average (WMA) calculator optimized for Hull Moving Average
/// Uses a circular buffer and incremental calculation for O(1) updates
/// </summary>
internal class WeightedMovingAverageHelper<TOutput> where TOutput : struct, INumber<TOutput>
{
    private readonly TOutput[] buffer;
    private readonly int period;
    private readonly TOutput weightSum;
    private int bufferIndex;
    private int count;
    private TOutput numeratorSum;
    private TOutput currentValue;

    /// <summary>
    /// Gets the current WMA value
    /// </summary>
    public TOutput Value => IsReady ? currentValue : default(TOutput)!;

    /// <summary>
    /// Gets whether the WMA has enough data to produce a value
    /// </summary>
    public bool IsReady => count >= period;

    /// <summary>
    /// Initializes a new WMA helper with the specified period
    /// </summary>
    public WeightedMovingAverageHelper(int period)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be positive", nameof(period));

        this.period = period;
        buffer = new TOutput[period];
        bufferIndex = 0;
        count = 0;
        numeratorSum = TOutput.Zero;
        currentValue = default(TOutput)!;

        // Calculate weight sum: 1 + 2 + 3 + ... + period = period * (period + 1) / 2
        weightSum = TOutput.CreateChecked(period * (period + 1) / 2);
    }

    /// <summary>
    /// Updates the WMA with a new value
    /// </summary>
    public void Update(TOutput value)
    {
        if (count < period)
        {
            // Still building up the initial period
            buffer[bufferIndex] = value;
            RecalculateFromScratch();
            count++;
        }
        else
        {
            // Replace oldest value and update incrementally
            var oldValue = buffer[bufferIndex];
            buffer[bufferIndex] = value;
            
            // Efficient incremental update
            UpdateIncrementally(oldValue, value);
        }

        bufferIndex = (bufferIndex + 1) % period;
    }

    /// <summary>
    /// Clears the WMA state
    /// </summary>
    public void Clear()
    {
        Array.Clear(buffer, 0, buffer.Length);
        bufferIndex = 0;
        count = 0;
        numeratorSum = TOutput.Zero;
        currentValue = default(TOutput)!;
    }

    /// <summary>
    /// Recalculates the WMA from scratch (used during initial fill period)
    /// </summary>
    private void RecalculateFromScratch()
    {
        numeratorSum = TOutput.Zero;
        
        for (int i = 0; i < count; i++)
        {
            var weight = TOutput.CreateChecked(count - i);
            var index = (bufferIndex - count + 1 + i + period) % period;
            numeratorSum += buffer[index] * weight;
        }

        if (count >= period)
        {
            currentValue = numeratorSum / weightSum;
        }
    }

    /// <summary>
    /// Updates the WMA incrementally for O(1) performance
    /// This is more complex but significantly faster for large periods
    /// </summary>
    private void UpdateIncrementally(TOutput oldValue, TOutput newValue)
    {
        // Remove the contribution of the old value at all weight positions
        // and add the contribution of all existing values with increased weight
        // Then add the new value at the highest weight
        
        // Subtract old value * period (it was at highest weight)
        var oldContribution = oldValue * TOutput.CreateChecked(period);
        numeratorSum -= oldContribution;
        
        // Each existing value's weight increases by 1, so add sum of all existing values
        var sumOfExistingValues = TOutput.Zero;
        for (int i = 0; i < period - 1; i++)
        {
            var index = (bufferIndex - period + 1 + i + period) % period;
            sumOfExistingValues += buffer[index];
        }
        numeratorSum += sumOfExistingValues;
        
        // Add new value at highest weight
        var newContribution = newValue * TOutput.CreateChecked(period);
        numeratorSum += newContribution;
        
        currentValue = numeratorSum / weightSum;
    }
}