using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Fisher Transform
/// Optimized for streaming updates with circular buffers
/// </summary>
public class FisherTransform_FP<TPrice, TOutput>
    : FisherTransformBase<TPrice, TOutput>
    , IIndicator2<FisherTransform_FP<TPrice, TOutput>, PFisherTransform<TPrice, TOutput>, HL<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    // Circular buffers for median prices (high + low) / 2
    private readonly CircularBuffer<TOutput> medianPrices;
    
    // Current Fisher and previous Fisher (trigger) values
    private TOutput currentFisher;
    private TOutput currentTrigger;
    private TOutput currentValue; // Intermediate smoothed value
    
    private int dataPointsReceived = 0;

    // Constants for calculations
    private readonly TOutput half = TOutput.CreateChecked(0.5);
    private readonly TOutput smoothingFactor = TOutput.CreateChecked(0.33);
    private readonly TOutput smoothingComplement = TOutput.CreateChecked(0.67);
    private readonly TOutput limitValue = TOutput.CreateChecked(0.999); // To avoid ln(infinity)
    private readonly TOutput minLimitValue;

    #endregion

    #region Properties

    public override TOutput Fisher => currentFisher;
    
    public override TOutput Trigger => currentTrigger;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    #endregion

    #region Lifecycle

    public static FisherTransform_FP<TPrice, TOutput> Create(PFisherTransform<TPrice, TOutput> p)
        => new FisherTransform_FP<TPrice, TOutput>(p);

    public FisherTransform_FP(PFisherTransform<TPrice, TOutput> parameters) : base(parameters)
    {
        medianPrices = new CircularBuffer<TOutput>(parameters.Period);
        minLimitValue = -limitValue;
        
        currentFisher = TOutput.Zero;
        currentTrigger = TOutput.Zero;
        currentValue = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HL<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput types
            var high = TOutput.CreateChecked(Convert.ToDecimal(input.High));
            var low = TOutput.CreateChecked(Convert.ToDecimal(input.Low));
            
            // Calculate median price: (High + Low) / 2
            var medianPrice = (high + low) * half;
            
            // Add to circular buffer
            medianPrices.Add(medianPrice);
            dataPointsReceived++;
            
            // Calculate Fisher Transform if we have enough data
            if (dataPointsReceived >= Parameters.Period)
            {
                // Store previous Fisher as trigger
                currentTrigger = currentFisher;
                
                // Calculate normalized value and Fisher Transform
                var normalizedValue = CalculateNormalizedValue(medianPrice);
                UpdateValue(normalizedValue);
                currentFisher = CalculateFisherTransform(currentValue);
            }
            
            // Output values
            var outputFisher = IsReady ? currentFisher : MissingOutputValue;
            var outputTrigger = IsReady ? currentTrigger : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length - 1)
            {
                output[outputIndex++] = outputFisher;
                output[outputIndex++] = outputTrigger;
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

    #region Fisher Transform Calculation

    private TOutput CalculateNormalizedValue(TOutput currentPrice)
    {
        // Find the highest and lowest values over the period
        var highest = medianPrices.Max();
        var lowest = medianPrices.Min();
        
        var range = highest - lowest;
        
        if (range == TOutput.Zero)
        {
            return TOutput.Zero; // If no range, return zero
        }
        
        // Normalize to -1 to 1 range
        // normalized = 2 * ((price - min) / (max - min)) - 1
        var normalized = (TOutput.CreateChecked(2) * ((currentPrice - lowest) / range)) - TOutput.One;
        
        // Clamp to prevent extreme values
        if (normalized > limitValue) normalized = limitValue;
        if (normalized < minLimitValue) normalized = minLimitValue;
        
        return normalized;
    }
    
    private void UpdateValue(TOutput normalizedValue)
    {
        // Smooth the normalized value
        // Value = 0.33 × 2 × ((1+x)/(1-x)) + 0.67 × previous Value
        var x = normalizedValue;
        var numerator = TOutput.One + x;
        var denominator = TOutput.One - x;
        
        // Avoid division by zero
        if (denominator == TOutput.Zero || TOutput.Abs(denominator) < TOutput.CreateChecked(0.0001))
        {
            // If denominator is too close to zero, use previous value
            return;
        }
        
        var ratio = numerator / denominator;
        var smoothedComponent = smoothingFactor * TOutput.CreateChecked(2) * ratio;
        var previousComponent = smoothingComplement * currentValue;
        
        currentValue = smoothedComponent + previousComponent;
    }
    
    private TOutput CalculateFisherTransform(TOutput value)
    {
        // Fisher Transform: 0.5 × ln((1+Value)/(1-Value))
        var numerator = TOutput.One + value;
        var denominator = TOutput.One - value;
        
        // Clamp value to avoid extreme calculations
        if (value > limitValue) value = limitValue;
        if (value < minLimitValue) value = minLimitValue;
        
        numerator = TOutput.One + value;
        denominator = TOutput.One - value;
        
        // Avoid division by zero and negative logarithms
        if (denominator <= TOutput.Zero || numerator <= TOutput.Zero)
        {
            return currentFisher; // Return previous value if calculation is invalid
        }
        
        var ratio = numerator / denominator;
        
        // Convert to decimal for logarithm calculation, then back to TOutput
        var ratioDecimal = Convert.ToDecimal(ratio);
        if (ratioDecimal <= 0m)
        {
            return currentFisher; // Return previous value if ratio is invalid
        }
        
        var lnValue = (decimal)Math.Log((double)ratioDecimal);
        var fisherValue = TOutput.CreateChecked(lnValue) * half;
        
        return fisherValue;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        medianPrices.Clear();
        currentFisher = TOutput.Zero;
        currentTrigger = TOutput.Zero;
        currentValue = TOutput.Zero;
        dataPointsReceived = 0;
    }

    #endregion

    #region Circular Buffer Helper Class

    private class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        private readonly int capacity;
        private int head = 0;
        private int count = 0;

        public CircularBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
        }

        public int Count => count;

        public void Add(T item)
        {
            buffer[head] = item;
            head = (head + 1) % capacity;
            if (count < capacity)
                count++;
        }

        public T Last()
        {
            if (count == 0)
                throw new InvalidOperationException("Buffer is empty");
            
            var index = (head - 1 + capacity) % capacity;
            return buffer[index];
        }

        public T Max()
        {
            if (count == 0)
                throw new InvalidOperationException("Buffer is empty");
            
            var max = buffer[0];
            for (int i = 1; i < count; i++)
            {
                if (Comparer<T>.Default.Compare(buffer[i], max) > 0)
                    max = buffer[i];
            }
            return max;
        }

        public T Min()
        {
            if (count == 0)
                throw new InvalidOperationException("Buffer is empty");
            
            var min = buffer[0];
            for (int i = 1; i < count; i++)
            {
                if (Comparer<T>.Default.Compare(buffer[i], min) < 0)
                    min = buffer[i];
            }
            return min;
        }

        public void Clear()
        {
            head = 0;
            count = 0;
            Array.Clear(buffer, 0, capacity);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return buffer[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    #endregion
}