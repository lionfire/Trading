using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Exponential Moving Average (EMA) indicator.
/// Uses SMA for initial seed value and then applies exponential smoothing.
/// Formula: EMA = (Price - Previous EMA) × Multiplier + Previous EMA
/// </summary>
public class EMA_FP<TPrice, TOutput> : SingleInputIndicatorBase<EMA_FP<TPrice, TOutput>, PEMA<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<EMA_FP<TPrice, TOutput>, PEMA<TPrice, TOutput>, TPrice, TOutput>,
    IEMA<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the EMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "EMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the EMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PEMA<TPrice, TOutput> p)
        => [new() {
                Name = "EMA",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The EMA parameters
    /// </summary>
    public readonly PEMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for EMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The smoothing factor (multiplier) used in EMA calculation
    /// </summary>
    public TOutput SmoothingFactor { get; private set; }

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new EMA indicator instance
    /// </summary>
    public static EMA_FP<TPrice, TOutput> Create(PEMA<TPrice, TOutput> p) => new EMA_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the EMA indicator
    /// </summary>
    public EMA_FP(PEMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        
        // Calculate smoothing factor
        SmoothingFactor = parameters.GetEffectiveSmoothingFactor();
        
        // Initialize buffers for SMA seed calculation
        seedBuffer = new TOutput[parameters.Period];
        seedBufferIndex = 0;
        seedCount = 0;
        seedSum = TOutput.Zero;
        emaValue = TOutput.Zero;
        isSeeded = false;
    }

    #endregion

    #region State

    // For initial SMA seed calculation
    private readonly TOutput[] seedBuffer;
    private int seedBufferIndex;
    private int seedCount;
    private TOutput seedSum;
    
    // EMA state
    private TOutput emaValue;
    private bool isSeeded;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => isSeeded || seedCount >= Period;

    /// <summary>
    /// Gets the current EMA value
    /// </summary>
    public TOutput Value => IsReady ? emaValue : default(TOutput)!;

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

            if (!isSeeded)
            {
                // We're still building the initial SMA seed
                
                // If buffer is full, subtract the oldest value from the sum
                if (seedCount >= Period)
                {
                    seedSum -= seedBuffer[seedBufferIndex];
                }

                // Add new value to buffer and sum
                seedBuffer[seedBufferIndex] = value;
                seedSum += value;

                // Move to next position in circular buffer
                seedBufferIndex = (seedBufferIndex + 1) % Period;

                // Increment count until we reach the period
                if (seedCount < Period)
                {
                    seedCount++;
                }

                // Check if we have enough data to calculate the seed SMA
                if (seedCount >= Period)
                {
                    // Calculate initial SMA as the seed value
                    emaValue = seedSum / TOutput.CreateChecked(Period);
                    isSeeded = true;
                    
                    // We can clear the seed buffer now to save memory
                    Array.Clear(seedBuffer, 0, seedBuffer.Length);
                }
                
                // Output current value if ready
                if (IsReady)
                {
                    if (subject != null)
                    {
                        subject.OnNext(new List<TOutput> { emaValue });
                    }

                    OnNext_PopulateOutput(emaValue, output, ref outputIndex, ref outputSkip);
                }
                else
                {
                    // Output NaN while warming up (so consumers can filter invalid values)
                    OnNext_PopulateOutput(TOutput.CreateChecked(double.NaN), output, ref outputIndex, ref outputSkip);
                }
            }
            else
            {
                // Apply EMA formula: EMA = (Price - Previous EMA) × Multiplier + Previous EMA
                // Which can be rewritten as: EMA = Price × Multiplier + Previous EMA × (1 - Multiplier)
                
                TOutput oneMinusMultiplier = TOutput.One - SmoothingFactor;
                emaValue = (value * SmoothingFactor) + (emaValue * oneMinusMultiplier);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { emaValue });
                }
                
                OnNext_PopulateOutput(emaValue, output, ref outputIndex, ref outputSkip);
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
        Array.Clear(seedBuffer, 0, seedBuffer.Length);
        seedBufferIndex = 0;
        seedCount = 0;
        seedSum = TOutput.Zero;
        emaValue = TOutput.Zero;
        isSeeded = false;
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