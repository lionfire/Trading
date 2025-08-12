using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Triple Exponential Moving Average (TEMA) indicator.
/// TEMA reduces lag compared to traditional EMAs by applying exponential smoothing three times.
/// Formula: TEMA = 3 × EMA1 - 3 × EMA2 + EMA3
/// Where: EMA1 = EMA(Price, Period), EMA2 = EMA(EMA1, Period), EMA3 = EMA(EMA2, Period)
/// </summary>
public class TEMA_FP<TPrice, TOutput> : SingleInputIndicatorBase<TEMA_FP<TPrice, TOutput>, PTEMA<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<TEMA_FP<TPrice, TOutput>, PTEMA<TPrice, TOutput>, TPrice, TOutput>,
    ITEMA<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the TEMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "TEMA",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA1",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA2",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA3",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the TEMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PTEMA<TPrice, TOutput> p)
        => [new() {
                Name = "TEMA",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA1",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA2",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "EMA3",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The TEMA parameters
    /// </summary>
    public readonly PTEMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for TEMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The smoothing factor (multiplier) used in EMA calculations
    /// </summary>
    public TOutput SmoothingFactor { get; private set; }

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period * 3; // Three levels of EMA need warm-up

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new TEMA indicator instance
    /// </summary>
    public static TEMA_FP<TPrice, TOutput> Create(PTEMA<TPrice, TOutput> p) => new TEMA_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the TEMA indicator
    /// </summary>
    public TEMA_FP(PTEMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        
        // Calculate smoothing factor
        SmoothingFactor = parameters.GetEffectiveSmoothingFactor();
        
        // Initialize EMA state
        ema1Value = TOutput.Zero;
        ema2Value = TOutput.Zero;
        ema3Value = TOutput.Zero;
        temaValue = TOutput.Zero;
        
        // Initialize buffers for SMA seed calculation
        seedBuffer = new TOutput[parameters.Period];
        seedBufferIndex = 0;
        seedCount = 0;
        seedSum = TOutput.Zero;
        
        // EMA state tracking
        ema1Ready = false;
        ema2Ready = false;
        ema3Ready = false;
        isReady = false;
    }

    #endregion

    #region State

    // EMA values
    private TOutput ema1Value;
    private TOutput ema2Value;
    private TOutput ema3Value;
    private TOutput temaValue;
    
    // For initial SMA seed calculation
    private readonly TOutput[] seedBuffer;
    private int seedBufferIndex;
    private int seedCount;
    private TOutput seedSum;
    
    // Readiness tracking
    private bool ema1Ready;
    private bool ema2Ready;
    private bool ema3Ready;
    private bool isReady;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => isReady;

    /// <summary>
    /// Gets the current TEMA value
    /// </summary>
    public TOutput Value => IsReady ? temaValue : default(TOutput)!;

    /// <summary>
    /// First EMA value (EMA of input prices)
    /// </summary>
    public TOutput EMA1 => ema1Ready ? ema1Value : default(TOutput)!;

    /// <summary>
    /// Second EMA value (EMA of EMA1)
    /// </summary>
    public TOutput EMA2 => ema2Ready ? ema2Value : default(TOutput)!;

    /// <summary>
    /// Third EMA value (EMA of EMA2)
    /// </summary>
    public TOutput EMA3 => ema3Ready ? ema3Value : default(TOutput)!;

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

            // Process through cascading EMAs
            ProcessInput(value);
            
            // Output current values
            if (IsReady)
            {
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { temaValue, ema1Value, ema2Value, ema3Value });
                }
                
                OnNext_PopulateOutput(temaValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Process a single input value through the cascading EMA calculations
    /// </summary>
    private void ProcessInput(TOutput value)
    {
        // Process EMA1 (EMA of input prices)
        if (!ema1Ready)
        {
            // Build initial SMA seed for EMA1
            if (seedCount >= Period)
            {
                seedSum -= seedBuffer[seedBufferIndex];
            }

            seedBuffer[seedBufferIndex] = value;
            seedSum += value;
            seedBufferIndex = (seedBufferIndex + 1) % Period;

            if (seedCount < Period)
            {
                seedCount++;
            }

            if (seedCount >= Period)
            {
                ema1Value = seedSum / TOutput.CreateChecked(Period);
                ema1Ready = true;
                
                // Clear seed buffer to save memory
                Array.Clear(seedBuffer, 0, seedBuffer.Length);
            }
        }
        else
        {
            // Apply EMA formula to EMA1
            TOutput oneMinusMultiplier = TOutput.One - SmoothingFactor;
            ema1Value = (value * SmoothingFactor) + (ema1Value * oneMinusMultiplier);
        }

        // Process EMA2 (EMA of EMA1) - only if EMA1 is ready
        if (ema1Ready)
        {
            if (!ema2Ready)
            {
                // Initialize EMA2 with first EMA1 value
                ema2Value = ema1Value;
                ema2Ready = true;
            }
            else
            {
                // Apply EMA formula to EMA2
                TOutput oneMinusMultiplier = TOutput.One - SmoothingFactor;
                ema2Value = (ema1Value * SmoothingFactor) + (ema2Value * oneMinusMultiplier);
            }
        }

        // Process EMA3 (EMA of EMA2) - only if EMA2 is ready
        if (ema2Ready)
        {
            if (!ema3Ready)
            {
                // Initialize EMA3 with first EMA2 value
                ema3Value = ema2Value;
                ema3Ready = true;
            }
            else
            {
                // Apply EMA formula to EMA3
                TOutput oneMinusMultiplier = TOutput.One - SmoothingFactor;
                ema3Value = (ema2Value * SmoothingFactor) + (ema3Value * oneMinusMultiplier);
            }
        }

        // Calculate TEMA once all three EMAs are ready
        if (ema1Ready && ema2Ready && ema3Ready)
        {
            // TEMA = 3 × EMA1 - 3 × EMA2 + EMA3
            var three = TOutput.CreateChecked(3);
            temaValue = (three * ema1Value) - (three * ema2Value) + ema3Value;
            isReady = true;
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
        
        // Clear EMA values
        ema1Value = TOutput.Zero;
        ema2Value = TOutput.Zero;
        ema3Value = TOutput.Zero;
        temaValue = TOutput.Zero;
        
        // Clear seed buffer
        Array.Clear(seedBuffer, 0, seedBuffer.Length);
        seedBufferIndex = 0;
        seedCount = 0;
        seedSum = TOutput.Zero;
        
        // Reset readiness flags
        ema1Ready = false;
        ema2Ready = false;
        ema3Ready = false;
        isReady = false;
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