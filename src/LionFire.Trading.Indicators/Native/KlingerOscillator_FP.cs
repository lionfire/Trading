using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Klinger Oscillator indicator.
/// Uses optimized EMA calculations for efficient O(1) computation of Volume Force, Klinger, and Signal values.
/// </summary>
public class KlingerOscillator_FP<TInput, TOutput> : KlingerOscillatorBase<TInput, TOutput>,
    IIndicator2<KlingerOscillator_FP<TInput, TOutput>, PKlingerOscillator<TInput, TOutput>, TInput, TOutput>,
    IKlingerOscillator<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Klinger Oscillator indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "Klinger",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Klinger Oscillator indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PKlingerOscillator<TInput, TOutput> p)
        => [
            new() {
                Name = "Klinger",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion

    #region Fields

    private readonly TOutput fastAlpha;
    private readonly TOutput slowAlpha;
    private readonly TOutput signalAlpha;
    
    private int count;
    private TOutput fastEma;
    private TOutput slowEma;
    private TOutput klingerValue;
    private TOutput signalValue;
    private TOutput volumeForceValue;
    private TOutput cumulativeMovement;
    private TOutput previousTrend;
    private bool firstValue;
    
    // Previous HLCV values for trend calculation
    private TOutput prevHigh, prevLow, prevClose;
    private bool hasPreviousHLCV;

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= SlowPeriod + SignalPeriod - 1;

    /// <summary>
    /// Gets the current Klinger Oscillator value (Fast EMA - Slow EMA of Volume Force)
    /// </summary>
    public override TOutput Klinger => IsReady ? klingerValue : default(TOutput)!;

    /// <summary>
    /// Gets the current Signal line value (EMA of Klinger line)
    /// </summary>
    public override TOutput Signal => IsReady ? signalValue : default(TOutput)!;

    /// <summary>
    /// Gets the current Volume Force value
    /// </summary>
    public override TOutput VolumeForce => IsReady ? volumeForceValue : default(TOutput)!;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Klinger Oscillator indicator instance
    /// </summary>
    public static KlingerOscillator_FP<TInput, TOutput> Create(PKlingerOscillator<TInput, TOutput> p) 
        => new KlingerOscillator_FP<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Klinger Oscillator indicator
    /// </summary>
    public KlingerOscillator_FP(PKlingerOscillator<TInput, TOutput> parameters) : base(parameters)
    {
        // Initialize EMA smoothing factors (alpha = 2 / (n + 1))
        fastAlpha = TOutput.CreateChecked(2.0 / (Parameters.FastPeriod + 1));
        slowAlpha = TOutput.CreateChecked(2.0 / (Parameters.SlowPeriod + 1));
        signalAlpha = TOutput.CreateChecked(2.0 / (Parameters.SignalPeriod + 1));
        
        // Initialize state
        count = 0;
        fastEma = TOutput.Zero;
        slowEma = TOutput.Zero;
        klingerValue = TOutput.Zero;
        signalValue = TOutput.Zero;
        volumeForceValue = TOutput.Zero;
        cumulativeMovement = TOutput.Zero;
        previousTrend = TOutput.Zero;
        firstValue = true;
        hasPreviousHLCV = false;
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of HLCV inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract HLCV data
            var (high, low, close, volume) = ExtractHLCV(input);
            
            // Calculate Volume Force and update Klinger
            ProcessHLCV(high, low, close, volume);
            
            count++;

            // Output values if ready
            if (IsReady)
            {
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { klingerValue, signalValue });
                }
                
                OnNext_PopulateOutput(klingerValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Processes HLCV data to calculate Volume Force and update Klinger Oscillator
    /// </summary>
    private void ProcessHLCV(TOutput high, TOutput low, TOutput close, TOutput volume)
    {
        if (!hasPreviousHLCV)
        {
            // First bar: initialize previous values
            prevHigh = high;
            prevLow = low;
            prevClose = close;
            hasPreviousHLCV = true;
            
            // Initialize cumulative movement with daily movement
            var initialDailyMovement = CalculateDailyMovement(high, low);
            cumulativeMovement = initialDailyMovement;
            previousTrend = TOutput.One; // Assume positive trend initially
            
            return;
        }

        // Calculate trend direction
        var trend = CalculateTrend(high, low, close, prevHigh, prevLow, prevClose);
        
        // Calculate Daily Movement (DM)
        var dailyMovement = CalculateDailyMovement(high, low);
        
        // Update Cumulative Movement (CM)
        if (trend == previousTrend)
        {
            // Same trend: add daily movement to cumulative movement
            cumulativeMovement += dailyMovement;
        }
        else
        {
            // Trend change: reset cumulative movement to previous cumulative movement + daily movement
            cumulativeMovement = cumulativeMovement + dailyMovement;
        }
        
        // Calculate Volume Force
        volumeForceValue = CalculateVolumeForce(volume, trend, dailyMovement, cumulativeMovement);
        
        // Update EMAs of Volume Force
        UpdateEMAs(volumeForceValue);
        
        // Calculate Klinger line (Fast EMA - Slow EMA)
        klingerValue = fastEma - slowEma;
        
        // Update Signal line (EMA of Klinger)
        UpdateSignalLine();
        
        // Store current values as previous for next iteration
        prevHigh = high;
        prevLow = low;
        prevClose = close;
        previousTrend = trend;
    }

    /// <summary>
    /// Updates the fast and slow EMAs with the new Volume Force value
    /// </summary>
    private void UpdateEMAs(TOutput volumeForce)
    {
        if (firstValue)
        {
            // Initialize EMAs with the first Volume Force value
            fastEma = volumeForce;
            slowEma = volumeForce;
            firstValue = false;
        }
        else
        {
            // EMA formula: EMA = alpha * value + (1 - alpha) * previousEMA
            // Optimized: EMA = previousEMA + alpha * (value - previousEMA)
            fastEma = fastEma + fastAlpha * (volumeForce - fastEma);
            slowEma = slowEma + slowAlpha * (volumeForce - slowEma);
        }
    }

    /// <summary>
    /// Updates the Signal line (EMA of Klinger line)
    /// </summary>
    private void UpdateSignalLine()
    {
        if (count >= SlowPeriod)
        {
            if (count == SlowPeriod)
            {
                // Initialize Signal EMA with first Klinger value
                signalValue = klingerValue;
            }
            else
            {
                // Update Signal EMA
                signalValue = signalValue + signalAlpha * (klingerValue - signalValue);
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
        else if (outputBuffer != null && outputIndex < outputBuffer.Length) 
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
        subject?.OnCompleted();
        subject = null;
        
        count = 0;
        fastEma = TOutput.Zero;
        slowEma = TOutput.Zero;
        klingerValue = TOutput.Zero;
        signalValue = TOutput.Zero;
        volumeForceValue = TOutput.Zero;
        cumulativeMovement = TOutput.Zero;
        previousTrend = TOutput.Zero;
        firstValue = true;
        hasPreviousHLCV = false;
        
        prevHigh = TOutput.Zero;
        prevLow = TOutput.Zero;
        prevClose = TOutput.Zero;
    }

    #endregion
}