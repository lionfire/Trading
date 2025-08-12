using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Fibonacci Retracement
/// Optimized for streaming updates with efficient swing point tracking
/// </summary>
public class FibonacciRetracement_FP<TPrice, TOutput>
    : FibonacciRetracementBase<TPrice, TOutput>
    , IIndicator2<FibonacciRetracement_FP<TPrice, TOutput>, PFibonacciRetracement<HLC<TPrice>, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly Queue<TOutput> highWindow;
    private readonly Queue<TOutput> lowWindow;
    private TOutput currentSwingHigh;
    private TOutput currentSwingLow;
    private int dataPointsReceived = 0;

    // Cached Fibonacci levels
    private TOutput level000;
    private TOutput level236;
    private TOutput level382;
    private TOutput level500;
    private TOutput level618;
    private TOutput level786;
    private TOutput level1000;
    private TOutput level1618;
    private TOutput level2618;

    #endregion

    #region Properties

    public override TOutput SwingHigh => currentSwingHigh;
    
    public override TOutput SwingLow => currentSwingLow;
    
    public override TOutput Level000 => level000;
    
    public override TOutput Level236 => level236;
    
    public override TOutput Level382 => level382;
    
    public override TOutput Level500 => level500;
    
    public override TOutput Level618 => level618;
    
    public override TOutput Level786 => level786;
    
    public override TOutput Level1000 => level1000;
    
    public override TOutput Level1618 => level1618;
    
    public override TOutput Level2618 => level2618;
    
    public override bool IsReady => dataPointsReceived >= Parameters.LookbackPeriod;

    #endregion

    #region Lifecycle

    public static FibonacciRetracement_FP<TPrice, TOutput> Create(PFibonacciRetracement<HLC<TPrice>, TOutput> p)
        => new FibonacciRetracement_FP<TPrice, TOutput>(p);

    public FibonacciRetracement_FP(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters) : base(parameters)
    {
        highWindow = new Queue<TOutput>(Parameters.LookbackPeriod);
        lowWindow = new Queue<TOutput>(Parameters.LookbackPeriod);
        
        InitializeValues();
    }

    private void InitializeValues()
    {
        currentSwingHigh = MissingOutputValue;
        currentSwingLow = MissingOutputValue;
        level000 = MissingOutputValue;
        level236 = MissingOutputValue;
        level382 = MissingOutputValue;
        level500 = MissingOutputValue;
        level618 = MissingOutputValue;
        level786 = MissingOutputValue;
        level1000 = MissingOutputValue;
        level1618 = MissingOutputValue;
        level2618 = MissingOutputValue;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            ProcessBar(input);
            
            // Output the Fibonacci levels
            var outputValues = GetOutputValues();
            
            if (outputSkip > 0)
            {
                // Skip the appropriate number of output values
                for (int i = 0; i < outputValues.Length && outputSkip > 0; i++)
                {
                    outputSkip--;
                }
            }
            else if (output != null)
            {
                // Write output values
                foreach (var value in outputValues)
                {
                    if (outputIndex < output.Length)
                        output[outputIndex++] = value;
                }
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

    #region Processing

    private void ProcessBar(HLC<TPrice> hlcBar)
    {
        // Convert TPrice to TOutput
        var high = TOutput.CreateChecked(Convert.ToDecimal(hlcBar.High));
        var low = TOutput.CreateChecked(Convert.ToDecimal(hlcBar.Low));
        
        // Update sliding windows
        if (highWindow.Count >= Parameters.LookbackPeriod)
        {
            highWindow.Dequeue();
            lowWindow.Dequeue();
        }
        
        highWindow.Enqueue(high);
        lowWindow.Enqueue(low);
        dataPointsReceived++;
        
        // Calculate swing high and low if we have enough data
        if (IsReady)
        {
            UpdateSwingPoints();
            CalculateFibonacciLevels();
        }
    }

    private void UpdateSwingPoints()
    {
        // Find the highest high and lowest low in the window
        currentSwingHigh = highWindow.Max();
        currentSwingLow = lowWindow.Min();
    }

    private void CalculateFibonacciLevels()
    {
        if (currentSwingHigh == MissingOutputValue || currentSwingLow == MissingOutputValue)
        {
            InitializeValues();
            return;
        }

        // Standard retracement levels
        level000 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 0.000);  // 0.0% (swing low)
        level236 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 0.236);  // 23.6%
        level382 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 0.382);  // 38.2%
        level500 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 0.500);  // 50.0%
        level618 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 0.618);  // 61.8% (golden ratio)
        level786 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 0.786);  // 78.6%
        level1000 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 1.000); // 100.0% (swing high)

        // Extension levels (if enabled)
        if (Parameters.IncludeExtensionLevels)
        {
            level1618 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 1.618); // 161.8% extension
            level2618 = CalculateFibonacciLevel(currentSwingHigh, currentSwingLow, 2.618); // 261.8% extension
        }
        else
        {
            level1618 = MissingOutputValue;
            level2618 = MissingOutputValue;
        }
    }

    private TOutput[] GetOutputValues()
    {
        if (!IsReady)
        {
            // Return missing values for all levels
            var missingValues = new TOutput[Parameters.IncludeExtensionLevels ? 9 : 7];
            Array.Fill(missingValues, MissingOutputValue);
            return missingValues;
        }

        if (Parameters.IncludeExtensionLevels)
        {
            return [level000, level236, level382, level500, level618, level786, level1000, level1618, level2618];
        }
        else
        {
            return [level000, level236, level382, level500, level618, level786, level1000];
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        highWindow.Clear();
        lowWindow.Clear();
        
        InitializeValues();
        dataPointsReceived = 0;
    }

    #endregion
}