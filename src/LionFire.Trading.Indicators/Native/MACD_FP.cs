using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of MACD (Moving Average Convergence Divergence) indicator.
/// Uses optimized EMA calculations for efficient O(1) computation of MACD, Signal, and Histogram values.
/// </summary>
public class MACD_FP<TPrice, TOutput> : SingleInputIndicatorBase<MACD_FP<TPrice, TOutput>, PMACD<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<MACD_FP<TPrice, TOutput>, PMACD<TPrice, TOutput>, TPrice, TOutput>,
    IMACD<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the MACD indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "MACD",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Histogram",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the MACD indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PMACD<TPrice, TOutput> p)
        => [
            new() {
                Name = "MACD",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Histogram",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion

    #region Parameters

    /// <summary>
    /// The MACD parameters
    /// </summary>
    public readonly PMACD<TPrice, TOutput> Parameters;

    /// <summary>
    /// The fast period for EMA calculation
    /// </summary>
    public int FastPeriod => Parameters.FastPeriod;

    /// <summary>
    /// The slow period for EMA calculation
    /// </summary>
    public int SlowPeriod => Parameters.SlowPeriod;

    /// <summary>
    /// The signal period for EMA calculation of the MACD line
    /// </summary>
    public int SignalPeriod => Parameters.SignalPeriod;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.SlowPeriod + Parameters.SignalPeriod - 1;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new MACD indicator instance
    /// </summary>
    public static MACD_FP<TPrice, TOutput> Create(PMACD<TPrice, TOutput> p) => new MACD_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the MACD indicator
    /// </summary>
    public MACD_FP(PMACD<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Parameters.Validate();
        
        // Initialize EMA smoothing factors (alpha = 2 / (n + 1))
        fastAlpha = TOutput.CreateChecked(2.0 / (Parameters.FastPeriod + 1));
        slowAlpha = TOutput.CreateChecked(2.0 / (Parameters.SlowPeriod + 1));
        signalAlpha = TOutput.CreateChecked(2.0 / (Parameters.SignalPeriod + 1));
        
        // Initialize state
        count = 0;
        fastEma = TOutput.Zero;
        slowEma = TOutput.Zero;
        macdValue = TOutput.Zero;
        signalValue = TOutput.Zero;
        histogramValue = TOutput.Zero;
        firstValue = true;
    }

    #endregion

    #region State

    private readonly TOutput fastAlpha;
    private readonly TOutput slowAlpha;
    private readonly TOutput signalAlpha;
    
    private int count;
    private TOutput fastEma;
    private TOutput slowEma;
    private TOutput macdValue;
    private TOutput signalValue;
    private TOutput histogramValue;
    private bool firstValue;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= SlowPeriod + SignalPeriod - 1;

    /// <summary>
    /// Gets the current MACD line value (Fast EMA - Slow EMA)
    /// </summary>
    public TOutput MACD => IsReady ? macdValue : default(TOutput)!;

    /// <summary>
    /// Gets the current Signal line value (EMA of MACD line)
    /// </summary>
    public TOutput Signal => IsReady ? signalValue : default(TOutput)!;

    /// <summary>
    /// Gets the current Histogram value (MACD - Signal)
    /// </summary>
    public TOutput Histogram => IsReady ? histogramValue : default(TOutput)!;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput
            TOutput value = ConvertToOutput(input);

            // Update EMAs
            UpdateEMAs(value);
            
            // Calculate MACD line (Fast EMA - Slow EMA)
            macdValue = fastEma - slowEma;
            
            // Update Signal line (EMA of MACD)
            UpdateSignalLine();
            
            // Calculate Histogram (MACD - Signal)
            histogramValue = macdValue - signalValue;

            count++;

            // Output values if ready
            if (IsReady)
            {
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { macdValue, signalValue, histogramValue });
                }
                
                OnNext_PopulateOutput(macdValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Updates the fast and slow EMAs with the new value
    /// </summary>
    private void UpdateEMAs(TOutput value)
    {
        if (firstValue)
        {
            // Initialize EMAs with the first value
            fastEma = value;
            slowEma = value;
            firstValue = false;
        }
        else
        {
            // EMA formula: EMA = alpha * value + (1 - alpha) * previousEMA
            // Optimized: EMA = previousEMA + alpha * (value - previousEMA)
            fastEma = fastEma + fastAlpha * (value - fastEma);
            slowEma = slowEma + slowAlpha * (value - slowEma);
        }
    }

    /// <summary>
    /// Updates the Signal line (EMA of MACD line)
    /// </summary>
    private void UpdateSignalLine()
    {
        if (count >= SlowPeriod)
        {
            if (count == SlowPeriod)
            {
                // Initialize Signal EMA with first MACD value
                signalValue = macdValue;
            }
            else
            {
                // Update Signal EMA
                signalValue = signalValue + signalAlpha * (macdValue - signalValue);
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
        count = 0;
        fastEma = TOutput.Zero;
        slowEma = TOutput.Zero;
        macdValue = TOutput.Zero;
        signalValue = TOutput.Zero;
        histogramValue = TOutput.Zero;
        firstValue = true;
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