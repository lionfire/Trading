using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Aroon indicator
/// Optimized for streaming updates with efficient period tracking
/// </summary>
public class Aroon_FP<TPrice, TOutput>
    : AroonBase<TPrice, TOutput>
    , IIndicator2<Aroon_FP<TPrice, TOutput>, PAroon<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    // Circular buffers for high and low prices
    private readonly CircularBuffer<TOutput> highBuffer;
    private readonly CircularBuffer<TOutput> lowBuffer;
    
    private TOutput currentAroonUp;
    private TOutput currentAroonDown;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput AroonUp => currentAroonUp;
    
    public override TOutput AroonDown => currentAroonDown;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    #endregion

    #region Lifecycle

    public static Aroon_FP<TPrice, TOutput> Create(PAroon<TPrice, TOutput> p)
        => new Aroon_FP<TPrice, TOutput>(p);

    public Aroon_FP(PAroon<TPrice, TOutput> parameters) : base(parameters)
    {
        highBuffer = new CircularBuffer<TOutput>(parameters.Period);
        lowBuffer = new CircularBuffer<TOutput>(parameters.Period);
        
        currentAroonUp = TOutput.CreateChecked(50); // Default neutral value
        currentAroonDown = TOutput.CreateChecked(50); // Default neutral value
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput type
            var high = TOutput.CreateChecked(Convert.ToDecimal(input.High));
            var low = TOutput.CreateChecked(Convert.ToDecimal(input.Low));
            
            // Add to circular buffers
            highBuffer.Add(high);
            lowBuffer.Add(low);
            
            dataPointsReceived++;
            
            // Calculate Aroon values if we have enough data
            if (dataPointsReceived >= Parameters.Period)
            {
                CalculateAroonValues();
            }
            
            // Output values
            var outputAroonUp = IsReady ? currentAroonUp : MissingOutputValue;
            var outputAroonDown = IsReady ? currentAroonDown : MissingOutputValue;
            var outputAroonOscillator = IsReady ? AroonOscillator : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length - 2)
            {
                output[outputIndex++] = outputAroonUp;
                output[outputIndex++] = outputAroonDown;
                output[outputIndex++] = outputAroonOscillator;
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

    #region Aroon Calculation

    private void CalculateAroonValues()
    {
        // Find the period since the highest high and lowest low
        var periodsSinceHigh = FindPeriodsSinceHighestHigh();
        var periodsSinceLow = FindPeriodsSinceLowestLow();
        
        // Calculate Aroon Up and Down
        // Aroon Up = ((Period - Periods since highest high) / Period) × 100
        // Aroon Down = ((Period - Periods since lowest low) / Period) × 100
        var period = TOutput.CreateChecked(Parameters.Period);
        var hundred = TOutput.CreateChecked(100);
        
        var periodsSinceHighOutput = TOutput.CreateChecked(periodsSinceHigh);
        var periodsSinceLowOutput = TOutput.CreateChecked(periodsSinceLow);
        
        currentAroonUp = ((period - periodsSinceHighOutput) / period) * hundred;
        currentAroonDown = ((period - periodsSinceLowOutput) / period) * hundred;
        
        // Ensure values are within bounds [0, 100]
        var zero = TOutput.Zero;
        if (currentAroonUp < zero) currentAroonUp = zero;
        if (currentAroonUp > hundred) currentAroonUp = hundred;
        if (currentAroonDown < zero) currentAroonDown = zero;
        if (currentAroonDown > hundred) currentAroonDown = hundred;
    }
    
    private int FindPeriodsSinceHighestHigh()
    {
        if (highBuffer.Count == 0)
            return Parameters.Period;
        
        var highestHigh = highBuffer.Max();
        var periodsSince = 0;
        
        // Find the most recent occurrence of the highest high
        // (search from most recent to oldest)
        for (int i = 0; i < highBuffer.Count; i++)
        {
            var value = highBuffer.GetFromEnd(i);
            if (value.Equals(highestHigh))
            {
                return periodsSince;
            }
            periodsSince++;
        }
        
        return periodsSince;
    }
    
    private int FindPeriodsSinceLowestLow()
    {
        if (lowBuffer.Count == 0)
            return Parameters.Period;
        
        var lowestLow = lowBuffer.Min();
        var periodsSince = 0;
        
        // Find the most recent occurrence of the lowest low
        // (search from most recent to oldest)
        for (int i = 0; i < lowBuffer.Count; i++)
        {
            var value = lowBuffer.GetFromEnd(i);
            if (value.Equals(lowestLow))
            {
                return periodsSince;
            }
            periodsSince++;
        }
        
        return periodsSince;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        highBuffer.Clear();
        lowBuffer.Clear();
        currentAroonUp = TOutput.CreateChecked(50);
        currentAroonDown = TOutput.CreateChecked(50);
        dataPointsReceived = 0;
    }

    #endregion

    #region Circular Buffer Helper Class

    private class CircularBuffer<T>
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

        public T GetFromEnd(int indexFromEnd)
        {
            if (indexFromEnd >= count)
                throw new ArgumentOutOfRangeException(nameof(indexFromEnd));
            
            var actualIndex = (head - 1 - indexFromEnd + capacity) % capacity;
            return buffer[actualIndex];
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
    }

    #endregion
}