using LionFire.Structures;
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Parabolic SAR using the complete algorithm
/// Optimized for streaming updates with proper trend tracking and reversal logic
/// 
/// Parabolic SAR Algorithm:
/// 1. Initial trend determination from first two bars
/// 2. Track extreme point (EP) - highest high in uptrend, lowest low in downtrend  
/// 3. Acceleration factor (AF) starts at initial value, increases when new extreme point is reached
/// 4. SAR calculation: SAR(tomorrow) = SAR(today) + AF * (EP - SAR(today))
/// 5. SAR must not penetrate prior period's price range in same trend direction
/// 6. When price crosses SAR, trend reverses and calculation resets
/// </summary>
public class ParabolicSAR_FP<TPrice, TOutput>
    : ParabolicSARBase<TPrice, TOutput>
    , IIndicator2<ParabolicSAR_FP<TPrice, TOutput>, PParabolicSAR<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private TOutput currentSAR;
    private TOutput extremePoint;
    private TOutput currentAF;
    private bool isLong;
    private bool hasReversed;
    private int dataPointsReceived = 0;
    
    // Previous bars needed for SAR calculations
    private HLC<TPrice> previousBar;
    private HLC<TPrice> currentBar;
    private TOutput previousSAR;
    private bool isInitialized = false;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentSAR;
    
    public override bool IsLong => isLong;
    
    public override bool HasReversed => hasReversed;
    
    public override TOutput CurrentAccelerationFactor => currentAF;
    
    public override bool IsReady => dataPointsReceived >= 2 && isInitialized;

    #endregion

    #region Lifecycle

    public static ParabolicSAR_FP<TPrice, TOutput> Create(PParabolicSAR<TPrice, TOutput> p)
        => new ParabolicSAR_FP<TPrice, TOutput>(p);

    public ParabolicSAR_FP(PParabolicSAR<TPrice, TOutput> parameters) : base(parameters)
    {
        currentSAR = TOutput.Zero;
        extremePoint = TOutput.Zero;
        currentAF = Parameters.AccelerationFactor;
        isLong = true; // Start with assumption of uptrend
        hasReversed = false;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            hasReversed = false; // Reset reversal flag each bar
            
            var high = TOutput.CreateChecked(Convert.ToDecimal(input.High));
            var low = TOutput.CreateChecked(Convert.ToDecimal(input.Low));
            
            if (dataPointsReceived == 0)
            {
                // First bar - store for next calculation
                currentBar = input;
                dataPointsReceived++;
                
                if (outputSkip > 0)
                {
                    outputSkip--;
                }
                else if (output != null && outputIndex < output.Length)
                {
                    output[outputIndex++] = MissingOutputValue;
                }
                continue;
            }
            
            if (dataPointsReceived == 1)
            {
                // Second bar - initialize trend and first SAR
                previousBar = currentBar;
                currentBar = input;
                
                var prevHigh = TOutput.CreateChecked(Convert.ToDecimal(previousBar.High));
                var prevLow = TOutput.CreateChecked(Convert.ToDecimal(previousBar.Low));
                
                // Determine initial trend based on relationship between first two bars
                if (high > prevHigh)
                {
                    // Uptrend
                    isLong = true;
                    extremePoint = high;
                    currentSAR = prevLow; // Start SAR at previous low
                }
                else
                {
                    // Downtrend
                    isLong = false;
                    extremePoint = low;
                    currentSAR = prevHigh; // Start SAR at previous high
                }
                
                currentAF = Parameters.AccelerationFactor;
                isInitialized = true;
                dataPointsReceived++;
                
                var outputValue = currentSAR;
                
                if (outputSkip > 0)
                {
                    outputSkip--;
                }
                else if (output != null && outputIndex < output.Length)
                {
                    output[outputIndex++] = outputValue;
                }
                continue;
            }
            
            // Normal processing for subsequent bars
            previousBar = currentBar;
            currentBar = input;
            previousSAR = currentSAR;
            
            // Check for trend reversal first
            bool trendReversed = false;
            if (isLong && low <= currentSAR)
            {
                // Uptrend reversal - price crossed below SAR
                trendReversed = true;
                hasReversed = true;
                isLong = false;
                currentSAR = extremePoint; // SAR becomes the previous extreme point
                extremePoint = low;
                currentAF = Parameters.AccelerationFactor;
            }
            else if (!isLong && high >= currentSAR)
            {
                // Downtrend reversal - price crossed above SAR
                trendReversed = true;
                hasReversed = true;
                isLong = true;
                currentSAR = extremePoint; // SAR becomes the previous extreme point
                extremePoint = high;
                currentAF = Parameters.AccelerationFactor;
            }
            
            if (!trendReversed)
            {
                // Continue existing trend
                
                // Update extreme point and acceleration factor if needed
                if (isLong && high > extremePoint)
                {
                    extremePoint = high;
                    if (currentAF < Parameters.MaxAccelerationFactor)
                    {
                        currentAF += Parameters.AccelerationFactor;
                        if (currentAF > Parameters.MaxAccelerationFactor)
                        {
                            currentAF = Parameters.MaxAccelerationFactor;
                        }
                    }
                }
                else if (!isLong && low < extremePoint)
                {
                    extremePoint = low;
                    if (currentAF < Parameters.MaxAccelerationFactor)
                    {
                        currentAF += Parameters.AccelerationFactor;
                        if (currentAF > Parameters.MaxAccelerationFactor)
                        {
                            currentAF = Parameters.MaxAccelerationFactor;
                        }
                    }
                }
                
                // Calculate new SAR
                currentSAR = previousSAR + currentAF * (extremePoint - previousSAR);
                
                // Apply SAR rules to prevent penetration of previous price ranges
                var prevHigh = TOutput.CreateChecked(Convert.ToDecimal(previousBar.High));
                var prevLow = TOutput.CreateChecked(Convert.ToDecimal(previousBar.Low));
                
                if (isLong)
                {
                    // In uptrend, SAR cannot be above the low of current or previous period
                    var maxSAR = TOutput.Min(low, prevLow);
                    if (currentSAR > maxSAR)
                    {
                        currentSAR = maxSAR;
                    }
                }
                else
                {
                    // In downtrend, SAR cannot be below the high of current or previous period
                    var minSAR = TOutput.Max(high, prevHigh);
                    if (currentSAR < minSAR)
                    {
                        currentSAR = minSAR;
                    }
                }
            }
            
            dataPointsReceived++;
            
            var finalOutputValue = IsReady ? currentSAR : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length)
            {
                output[outputIndex++] = finalOutputValue;
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

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        currentSAR = TOutput.Zero;
        extremePoint = TOutput.Zero;
        currentAF = Parameters.AccelerationFactor;
        isLong = true;
        hasReversed = false;
        dataPointsReceived = 0;
        isInitialized = false;
        previousBar = default;
        currentBar = default;
        previousSAR = TOutput.Zero;
    }

    #endregion
}