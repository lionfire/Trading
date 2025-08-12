using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Triple Exponential Moving Average (TEMA) indicator.
/// Wraps QuantConnect's TripleExponentialMovingAverage for compatibility.
/// </summary>
public class TEMA_QC<TPrice, TOutput> : SingleInputIndicatorBase<TEMA_QC<TPrice, TOutput>, PTEMA<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<TEMA_QC<TPrice, TOutput>, PTEMA<TPrice, TOutput>, TPrice, TOutput>,
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
    public static TEMA_QC<TPrice, TOutput> Create(PTEMA<TPrice, TOutput> p) => new TEMA_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the TEMA indicator
    /// </summary>
    public TEMA_QC(PTEMA<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        
        // Calculate smoothing factor
        SmoothingFactor = parameters.GetEffectiveSmoothingFactor();
        
        // Initialize QuantConnect TEMA indicator
        // Note: This would normally wrap QuantConnect.Indicators.TripleExponentialMovingAverage
        // For now, we'll implement a compatible version that can be replaced with actual QC wrapper
        
        InitializeQuantConnectTEMA(parameters.Period);
    }

    #endregion

    #region State

    // QuantConnect TEMA state (placeholder - would be actual QC indicator in real implementation)
    private decimal[] ema1Buffer;
    private decimal[] ema2Buffer; 
    private decimal[] ema3Buffer;
    private int bufferIndex;
    private int sampleCount;
    private decimal ema1Value;
    private decimal ema2Value;
    private decimal ema3Value;
    private decimal temaValue;
    private decimal multiplier;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => sampleCount >= Period * 3;

    /// <summary>
    /// Gets the current TEMA value
    /// </summary>
    public TOutput Value => IsReady ? ConvertToOutput(temaValue) : default(TOutput)!;

    /// <summary>
    /// First EMA value (EMA of input prices)
    /// </summary>
    public TOutput EMA1 => sampleCount >= Period ? ConvertToOutput(ema1Value) : default(TOutput)!;

    /// <summary>
    /// Second EMA value (EMA of EMA1)
    /// </summary>
    public TOutput EMA2 => sampleCount >= Period * 2 ? ConvertToOutput(ema2Value) : default(TOutput)!;

    /// <summary>
    /// Third EMA value (EMA of EMA2)
    /// </summary>
    public TOutput EMA3 => IsReady ? ConvertToOutput(ema3Value) : default(TOutput)!;

    #endregion

    #region Implementation

    /// <summary>
    /// Initialize the QuantConnect TEMA components
    /// </summary>
    private void InitializeQuantConnectTEMA(int period)
    {
        // Initialize buffers for warm-up period
        ema1Buffer = new decimal[period];
        ema2Buffer = new decimal[period];
        ema3Buffer = new decimal[period];
        bufferIndex = 0;
        sampleCount = 0;
        
        // Calculate multiplier for EMA
        multiplier = 2m / (period + 1);
        
        // Initialize values
        ema1Value = 0m;
        ema2Value = 0m;
        ema3Value = 0m;
        temaValue = 0m;
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to decimal for QC compatibility
            decimal value = ConvertToDecimal(input);
            
            // Process through QuantConnect TEMA logic
            ProcessQuantConnectTEMA(value);
            
            // Output current values
            if (IsReady)
            {
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { Value, EMA1, EMA2, EMA3 });
                }
                
                OnNext_PopulateOutput(Value, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Process input through QuantConnect-style TEMA calculation
    /// </summary>
    private void ProcessQuantConnectTEMA(decimal value)
    {
        sampleCount++;
        
        // Calculate EMA1
        if (sampleCount == 1)
        {
            ema1Value = value;
        }
        else
        {
            ema1Value = (value - ema1Value) * multiplier + ema1Value;
        }
        
        // Calculate EMA2 once we have EMA1
        if (sampleCount == 1)
        {
            ema2Value = ema1Value;
        }
        else
        {
            ema2Value = (ema1Value - ema2Value) * multiplier + ema2Value;
        }
        
        // Calculate EMA3 once we have EMA2
        if (sampleCount == 1)
        {
            ema3Value = ema2Value;
        }
        else
        {
            ema3Value = (ema2Value - ema3Value) * multiplier + ema3Value;
        }
        
        // Calculate TEMA: 3 × EMA1 - 3 × EMA2 + EMA3
        temaValue = 3m * ema1Value - 3m * ema2Value + ema3Value;
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
        
        // Reset QuantConnect TEMA state
        Array.Clear(ema1Buffer, 0, ema1Buffer.Length);
        Array.Clear(ema2Buffer, 0, ema2Buffer.Length);
        Array.Clear(ema3Buffer, 0, ema3Buffer.Length);
        bufferIndex = 0;
        sampleCount = 0;
        ema1Value = 0m;
        ema2Value = 0m;
        ema3Value = 0m;
        temaValue = 0m;
    }

    /// <summary>
    /// Convert input type to decimal for QuantConnect compatibility
    /// </summary>
    private static decimal ConvertToDecimal(TPrice input)
    {
        // Handle common conversions
        if (typeof(TPrice) == typeof(decimal))
        {
            return (decimal)(object)input;
        }
        else if (typeof(TPrice) == typeof(double))
        {
            return (decimal)(double)(object)input;
        }
        else if (typeof(TPrice) == typeof(float))
        {
            return (decimal)(float)(object)input;
        }
        else
        {
            // Use generic conversion for other types
            return Convert.ToDecimal(input);
        }
    }

    /// <summary>
    /// Convert decimal result to output type
    /// </summary>
    private static TOutput ConvertToOutput(decimal value)
    {
        // Handle common conversions
        if (typeof(TOutput) == typeof(decimal))
        {
            return (TOutput)(object)value;
        }
        else if (typeof(TOutput) == typeof(double))
        {
            return (TOutput)(object)(double)value;
        }
        else if (typeof(TOutput) == typeof(float))
        {
            return (TOutput)(object)(float)value;
        }
        else
        {
            // Use generic conversion for other types
            return TOutput.CreateChecked(value);
        }
    }

    #endregion
}