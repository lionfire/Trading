using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Williams %R indicator
/// Optimized for streaming updates with circular buffer optimization
/// </summary>
public class WilliamsR_FP<TPrice, TOutput>
    : WilliamsRBase<TPrice, TOutput>
    , IIndicator2<WilliamsR_FP<TPrice, TOutput>, PWilliamsR<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    // Circular buffers for high, low, and close prices
    private readonly CircularBuffer<TOutput> highBuffer;
    private readonly CircularBuffer<TOutput> lowBuffer;
    
    private TOutput currentWilliamsR;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentWilliamsR;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    #endregion

    #region Lifecycle

    public static WilliamsR_FP<TPrice, TOutput> Create(PWilliamsR<TPrice, TOutput> p)
        => new WilliamsR_FP<TPrice, TOutput>(p);

    public WilliamsR_FP(PWilliamsR<TPrice, TOutput> parameters) : base(parameters)
    {
        highBuffer = new CircularBuffer<TOutput>(parameters.Period);
        lowBuffer = new CircularBuffer<TOutput>(parameters.Period);
        
        currentWilliamsR = TOutput.CreateChecked(-50); // Default neutral value
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
            var close = TOutput.CreateChecked(Convert.ToDecimal(input.Close));
            
            // Add to circular buffers
            highBuffer.Add(high);
            lowBuffer.Add(low);
            
            dataPointsReceived++;
            
            // Calculate Williams %R if we have enough data
            if (dataPointsReceived >= Parameters.Period)
            {
                currentWilliamsR = CalculateWilliamsR(close);
            }
            
            var outputValue = IsReady ? currentWilliamsR : MissingOutputValue;
            
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

    #region Williams %R Calculation

    private TOutput CalculateWilliamsR(TOutput close)
    {
        if (highBuffer.Count < Parameters.Period || lowBuffer.Count < Parameters.Period)
        {
            return TOutput.CreateChecked(-50); // Default neutral value
        }
        
        var highestHigh = highBuffer.Max();
        var lowestLow = lowBuffer.Min();
        
        // Avoid division by zero
        var range = highestHigh - lowestLow;
        if (range == TOutput.Zero)
        {
            return TOutput.Zero; // No range means no oscillation
        }
        
        // Williams %R = ((Highest High - Close) / (Highest High - Lowest Low)) * -100
        var williamsR = ((highestHigh - close) / range) * TOutput.CreateChecked(-100);
        
        // Ensure Williams %R is within bounds [-100, 0]
        var negHundred = TOutput.CreateChecked(-100);
        var zero = TOutput.Zero;
        
        if (williamsR < negHundred) williamsR = negHundred;
        if (williamsR > zero) williamsR = zero;
        
        return williamsR;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        highBuffer.Clear();
        lowBuffer.Clear();
        currentWilliamsR = TOutput.CreateChecked(-50);
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
                var index = (head - count + i + capacity) % capacity;
                yield return buffer[index];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    #endregion
}