using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Stochastic Oscillator
/// Optimized for streaming updates with circular buffers
/// </summary>
public class StochasticFP<TPrice, TOutput>
    : StochasticBase<TPrice, TOutput>
    , IIndicator2<StochasticFP<TPrice, TOutput>, PStochastic<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    // Circular buffers for high, low, and close prices
    private readonly CircularBuffer<TOutput> highBuffer;
    private readonly CircularBuffer<TOutput> lowBuffer;
    private readonly CircularBuffer<TOutput> closeBuffer;
    
    // Buffer for raw %K values (before smoothing)
    private readonly CircularBuffer<TOutput> rawKBuffer;
    
    // Buffer for smoothed %K values (for %D calculation)
    private readonly CircularBuffer<TOutput> smoothedKBuffer;
    
    private TOutput currentPercentK;
    private TOutput currentPercentD;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput PercentK => currentPercentK;
    
    public override TOutput PercentD => currentPercentD;
    
    public override bool IsReady => dataPointsReceived >= MaxLookback;

    #endregion

    #region Lifecycle

    public static StochasticFP<TPrice, TOutput> Create(PStochastic<TPrice, TOutput> p)
        => new StochasticFP<TPrice, TOutput>(p);

    public StochasticFP(PStochastic<TPrice, TOutput> parameters) : base(parameters)
    {
        highBuffer = new CircularBuffer<TOutput>(parameters.FastPeriod);
        lowBuffer = new CircularBuffer<TOutput>(parameters.FastPeriod);
        closeBuffer = new CircularBuffer<TOutput>(parameters.FastPeriod);
        rawKBuffer = new CircularBuffer<TOutput>(parameters.SlowKPeriod);
        smoothedKBuffer = new CircularBuffer<TOutput>(parameters.SlowDPeriod);
        
        currentPercentK = TOutput.CreateChecked(50); // Default neutral value
        currentPercentD = TOutput.CreateChecked(50); // Default neutral value
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
            closeBuffer.Add(close);
            
            dataPointsReceived++;
            
            // Calculate raw %K if we have enough data
            if (dataPointsReceived >= Parameters.FastPeriod)
            {
                var rawK = CalculateRawK();
                rawKBuffer.Add(rawK);
                
                // Calculate smoothed %K if we have enough raw K values
                if (rawKBuffer.Count >= Parameters.SlowKPeriod)
                {
                    currentPercentK = CalculateSmoothedK();
                    smoothedKBuffer.Add(currentPercentK);
                    
                    // Calculate %D if we have enough smoothed K values
                    if (smoothedKBuffer.Count >= Parameters.SlowDPeriod)
                    {
                        currentPercentD = CalculateD();
                    }
                }
            }
            
            // Output values
            var outputK = IsReady ? currentPercentK : MissingOutputValue;
            var outputD = IsReady ? currentPercentD : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length - 1)
            {
                output[outputIndex++] = outputK;
                output[outputIndex++] = outputD;
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

    #region Stochastic Calculation

    private TOutput CalculateRawK()
    {
        // Find highest high and lowest low over the fast period
        var highestHigh = highBuffer.Max();
        var lowestLow = lowBuffer.Min();
        
        // Get the most recent close
        var currentClose = closeBuffer.Last();
        
        // Calculate raw %K
        // %K = ((Current Close - Lowest Low) / (Highest High - Lowest Low)) × 100
        var range = highestHigh - lowestLow;
        
        if (range == TOutput.Zero)
        {
            // If range is zero, return 50 (neutral)
            return TOutput.CreateChecked(50);
        }
        
        var rawK = ((currentClose - lowestLow) / range) * TOutput.CreateChecked(100);
        
        // Ensure %K is within bounds [0, 100]
        var zero = TOutput.Zero;
        var hundred = TOutput.CreateChecked(100);
        if (rawK < zero) rawK = zero;
        if (rawK > hundred) rawK = hundred;
        
        return rawK;
    }
    
    private TOutput CalculateSmoothedK()
    {
        // Simple Moving Average of raw %K values
        var sum = TOutput.Zero;
        var count = 0;
        
        foreach (var value in rawKBuffer)
        {
            sum += value;
            count++;
        }
        
        if (count == 0)
            return TOutput.CreateChecked(50);
            
        return sum / TOutput.CreateChecked(count);
    }
    
    private TOutput CalculateD()
    {
        // Simple Moving Average of smoothed %K values
        var sum = TOutput.Zero;
        var count = 0;
        
        foreach (var value in smoothedKBuffer)
        {
            sum += value;
            count++;
        }
        
        if (count == 0)
            return TOutput.CreateChecked(50);
            
        return sum / TOutput.CreateChecked(count);
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        highBuffer.Clear();
        lowBuffer.Clear();
        closeBuffer.Clear();
        rawKBuffer.Clear();
        smoothedKBuffer.Clear();
        currentPercentK = TOutput.CreateChecked(50);
        currentPercentD = TOutput.CreateChecked(50);
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