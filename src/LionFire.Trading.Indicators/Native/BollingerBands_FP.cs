using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Bollinger Bands using Simple Moving Average and standard deviation
/// Optimized for streaming updates
/// </summary>
public class BollingerBands_FP<TInput, TOutput>
    : BollingerBandsBase<TInput, TOutput>
    , IIndicator2<BollingerBands_FP<TInput, TOutput>, PBollingerBands<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly Queue<TOutput> priceWindow;
    private TOutput sum;
    private TOutput currentMiddleBand;
    private TOutput currentUpperBand;
    private TOutput currentLowerBand;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput UpperBand => currentUpperBand;
    
    public override TOutput MiddleBand => currentMiddleBand;
    
    public override TOutput LowerBand => currentLowerBand;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    #endregion

    #region Lifecycle

    public static BollingerBands_FP<TInput, TOutput> Create(PBollingerBands<TInput, TOutput> p)
        => new BollingerBands_FP<TInput, TOutput>(p);

    public BollingerBands_FP(PBollingerBands<TInput, TOutput> parameters) : base(parameters)
    {
        priceWindow = new Queue<TOutput>(Parameters.Period);
        sum = TOutput.Zero;
        currentMiddleBand = MissingOutputValue;
        currentUpperBand = MissingOutputValue;
        currentLowerBand = MissingOutputValue;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput type through decimal as intermediate
            var price = TOutput.CreateChecked(Convert.ToDecimal(input));
            LastPrice = price;
            
            // Update the sliding window
            if (priceWindow.Count >= Parameters.Period)
            {
                // Remove oldest price from sum
                sum -= priceWindow.Dequeue();
            }
            
            // Add new price
            priceWindow.Enqueue(price);
            sum += price;
            dataPointsReceived++;
            
            // Calculate bands if we have enough data
            if (IsReady)
            {
                CalculateBands();
            }
            
            // Output the band values
            var upper = IsReady ? currentUpperBand : MissingOutputValue;
            var middle = IsReady ? currentMiddleBand : MissingOutputValue;
            var lower = IsReady ? currentLowerBand : MissingOutputValue;
            
            // Output in the order: Upper, Middle, Lower
            if (outputSkip > 0)
            {
                outputSkip--;
                if (outputSkip > 0) outputSkip--;
                if (outputSkip > 0) outputSkip--;
            }
            else if (output != null)
            {
                if (outputIndex < output.Length) output[outputIndex++] = upper;
                if (outputIndex < output.Length) output[outputIndex++] = middle;
                if (outputIndex < output.Length) output[outputIndex++] = lower;
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

    #region Bollinger Bands Calculation

    private void CalculateBands()
    {
        var period = TOutput.CreateChecked(Parameters.Period);
        
        // Calculate Simple Moving Average (Middle Band)
        currentMiddleBand = sum / period;
        
        // Calculate Standard Deviation
        var variance = TOutput.Zero;
        foreach (var price in priceWindow)
        {
            var diff = price - currentMiddleBand;
            variance += diff * diff;
        }
        variance = variance / period;
        
        // Calculate standard deviation using Newton-Raphson method for square root
        var stdDev = CalculateSquareRoot(variance);
        
        // Calculate bands
        var bandDistance = stdDev * Parameters.StandardDeviations;
        currentUpperBand = currentMiddleBand + bandDistance;
        currentLowerBand = currentMiddleBand - bandDistance;
    }
    
    /// <summary>
    /// Calculate square root using Newton-Raphson method
    /// This is more efficient than converting to double and back
    /// </summary>
    private TOutput CalculateSquareRoot(TOutput value)
    {
        if (value == TOutput.Zero) return TOutput.Zero;
        
        // Initial guess: value / 2
        var two = TOutput.CreateChecked(2);
        var guess = value / two;
        var epsilon = TOutput.CreateChecked(0.0001); // Precision threshold
        
        // Newton-Raphson iterations
        for (int i = 0; i < 10; i++) // Maximum 10 iterations
        {
            var nextGuess = (guess + value / guess) / two;
            var diff = nextGuess - guess;
            if (diff < epsilon && diff > -epsilon) // Converged
            {
                return nextGuess;
            }
            guess = nextGuess;
        }
        
        return guess;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        priceWindow.Clear();
        sum = TOutput.Zero;
        currentMiddleBand = MissingOutputValue;
        currentUpperBand = MissingOutputValue;
        currentLowerBand = MissingOutputValue;
        LastPrice = MissingOutputValue;
        dataPointsReceived = 0;
    }

    #endregion
}