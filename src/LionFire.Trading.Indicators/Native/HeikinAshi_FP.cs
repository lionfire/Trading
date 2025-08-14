using LionFire.Trading;
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Heikin-Ashi (Average Bar) indicator.
/// Transforms regular candlesticks to Heikin-Ashi candles for smoother trend visualization.
/// Optimized for streaming updates with efficient state management.
/// </summary>
public class HeikinAshi_FP<TInput, TOutput> : HeikinAshiBase<HeikinAshi_FP<TInput, TOutput>, TInput, TOutput>,
    IIndicator2<HeikinAshi_FP<TInput, TOutput>, PHeikinAshi<TInput, TOutput>, TInput, TOutput>,
    IHeikinAshi<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Heikin-Ashi indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "HA_Open", ValueType = typeof(TOutput) },
            new() { Name = "HA_High", ValueType = typeof(TOutput) },
            new() { Name = "HA_Low", ValueType = typeof(TOutput) },
            new() { Name = "HA_Close", ValueType = typeof(TOutput) }
        ];

    /// <summary>
    /// Gets the output slots for the Heikin-Ashi indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PHeikinAshi<TInput, TOutput> p)
        => [
            new() { Name = "HA_Open", ValueType = typeof(TOutput) },
            new() { Name = "HA_High", ValueType = typeof(TOutput) },
            new() { Name = "HA_Low", ValueType = typeof(TOutput) },
            new() { Name = "HA_Close", ValueType = typeof(TOutput) }
        ];

    #endregion

    #region Fields

    private TOutput currentHA_Open;
    private TOutput currentHA_High;
    private TOutput currentHA_Low;
    private TOutput currentHA_Close;
    private TOutput previousHA_Open;
    private TOutput previousHA_Close;
    private bool hasFirstBar = false;
    private bool isReady = false;

    #endregion

    #region Properties

    public override TOutput HA_Open => IsReady ? currentHA_Open : default(TOutput)!;
    public override TOutput HA_High => IsReady ? currentHA_High : default(TOutput)!;
    public override TOutput HA_Low => IsReady ? currentHA_Low : default(TOutput)!;
    public override TOutput HA_Close => IsReady ? currentHA_Close : default(TOutput)!;

    public override bool IsReady => isReady;

    #endregion

    #region Lifecycle

    public static HeikinAshi_FP<TInput, TOutput> Create(PHeikinAshi<TInput, TOutput> p) 
        => new HeikinAshi_FP<TInput, TOutput>(p);

    public HeikinAshi_FP(PHeikinAshi<TInput, TOutput> parameters) : base(parameters)
    {
        currentHA_Open = default(TOutput)!;
        currentHA_High = default(TOutput)!;
        currentHA_Low = default(TOutput)!;
        currentHA_Close = default(TOutput)!;
        previousHA_Open = default(TOutput)!;
        previousHA_Close = default(TOutput)!;
        hasFirstBar = false;
        isReady = false;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract OHLC data from input
            var (open, high, low, close) = ExtractOHLC(input);
            
            if (!hasFirstBar)
            {
                // First candle: HA_Open = (Open + Close) / 2
                var two = TOutput.CreateChecked(2);
                var four = TOutput.CreateChecked(4);
                
                currentHA_Open = (open + close) / two;
                currentHA_Close = (open + high + low + close) / four;
                currentHA_High = TOutput.Max(high, TOutput.Max(currentHA_Open, currentHA_Close));
                currentHA_Low = TOutput.Min(low, TOutput.Min(currentHA_Open, currentHA_Close));
                
                // Store for next calculation
                previousHA_Open = currentHA_Open;
                previousHA_Close = currentHA_Close;
                
                hasFirstBar = true;
                isReady = true;
            }
            else
            {
                // Subsequent candles: HA_Open = (Previous HA_Open + Previous HA_Close) / 2
                var two = TOutput.CreateChecked(2);
                var four = TOutput.CreateChecked(4);
                
                currentHA_Open = (previousHA_Open + previousHA_Close) / two;
                currentHA_Close = (open + high + low + close) / four;
                currentHA_High = TOutput.Max(high, TOutput.Max(currentHA_Open, currentHA_Close));
                currentHA_Low = TOutput.Min(low, TOutput.Min(currentHA_Open, currentHA_Close));
                
                // Store for next calculation
                previousHA_Open = currentHA_Open;
                previousHA_Close = currentHA_Close;
            }
            
            // Output the Heikin-Ashi values
            OutputCurrentValues(output, ref outputIndex, ref outputSkip);
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

    #region Helper Methods

    /// <summary>
    /// Helper method to output current Heikin-Ashi values to the output buffer
    /// </summary>
    private void OutputCurrentValues(TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        var values = new[] { HA_Open, HA_High, HA_Low, HA_Close };
        
        foreach (var value in values)
        {
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (outputBuffer != null && outputIndex < outputBuffer.Length)
            {
                outputBuffer[outputIndex++] = value;
            }
        }
    }

    /// <summary>
    /// Extracts OHLC data from input using reflection
    /// </summary>
    protected virtual (TOutput open, TOutput high, TOutput low, TOutput close) ExtractOHLC(TInput input)
    {
        // Handle different input types
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Look for OHLC properties - try different naming conventions
        var openProperty = inputType.GetProperty("Open") ?? 
                          inputType.GetProperty("OpenPrice") ?? 
                          inputType.GetProperty("O");
        var highProperty = inputType.GetProperty("High") ?? 
                          inputType.GetProperty("HighPrice") ?? 
                          inputType.GetProperty("H");
        var lowProperty = inputType.GetProperty("Low") ?? 
                         inputType.GetProperty("LowPrice") ?? 
                         inputType.GetProperty("L");
        var closeProperty = inputType.GetProperty("Close") ?? 
                           inputType.GetProperty("ClosePrice") ?? 
                           inputType.GetProperty("C");
        
        if (openProperty != null && highProperty != null && lowProperty != null && closeProperty != null)
        {
            var openValue = openProperty.GetValue(boxed);
            var highValue = highProperty.GetValue(boxed);
            var lowValue = lowProperty.GetValue(boxed);
            var closeValue = closeProperty.GetValue(boxed);
            
            return (
                TOutput.CreateChecked(Convert.ToDecimal(openValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(highValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(lowValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(closeValue!))
            );
        }
        
        // If the input is itself a numeric type, treat it as close price
        // and use the same value for all OHLC components
        if (input is IConvertible convertible)
        {
            var value = TOutput.CreateChecked(Convert.ToDecimal(input));
            return (value, value, value, value);
        }
        
        throw new InvalidOperationException(
            $"Unable to extract OHLC data from input type {inputType.Name}. " +
            "Input must have Open, High, Low, and Close properties (or OpenPrice, HighPrice, LowPrice, ClosePrice), " +
            "or be a numeric type.");
    }

    #endregion

    #region Methods

    public override void Clear() 
    { 
        subject?.OnCompleted();
        subject = null;
        
        currentHA_Open = default(TOutput)!;
        currentHA_High = default(TOutput)!;
        currentHA_Low = default(TOutput)!;
        currentHA_Close = default(TOutput)!;
        previousHA_Open = default(TOutput)!;
        previousHA_Close = default(TOutput)!;
        hasFirstBar = false;
        isReady = false;
    }

    #endregion
}