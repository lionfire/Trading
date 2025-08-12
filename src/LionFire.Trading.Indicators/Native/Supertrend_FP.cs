using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Supertrend using efficient ATR calculation
/// Optimized for streaming updates with trend persistence logic
/// </summary>
public class Supertrend_FP<TPrice, TOutput>
    : SupertrendBase<TPrice, TOutput>
    , IIndicator2<Supertrend_FP<TPrice, TOutput>, PSupertrend<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private TOutput currentSupertrend;
    private TOutput currentATR;
    private int currentTrendDirection = 1; // Start with uptrend assumption
    private TOutput finalUpperBand;
    private TOutput finalLowerBand;
    private TPrice? previousClose;
    
    // ATR calculation using Wilder's smoothing
    private TOutput atrSum;
    private int dataPointsReceived = 0;
    private bool isATRReady = false;
    
    // Band tracking
    private TOutput previousFinalUpperBand;
    private TOutput previousFinalLowerBand;
    private bool hasPreviousBands = false;

    #endregion

    #region Properties

    public override TOutput Value => currentSupertrend;
    
    public override int TrendDirection => currentTrendDirection;
    
    public override TOutput CurrentATR => currentATR;
    
    public override bool IsReady => isATRReady && dataPointsReceived > Parameters.AtrPeriod;

    #endregion

    #region Lifecycle

    public static Supertrend_FP<TPrice, TOutput> Create(PSupertrend<TPrice, TOutput> p)
        => new Supertrend_FP<TPrice, TOutput>(p);

    public Supertrend_FP(PSupertrend<TPrice, TOutput> parameters) : base(parameters)
    {
        currentSupertrend = TOutput.Zero;
        currentATR = TOutput.Zero;
        atrSum = TOutput.Zero;
        finalUpperBand = TOutput.Zero;
        finalLowerBand = TOutput.Zero;
        previousFinalUpperBand = TOutput.Zero;
        previousFinalLowerBand = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            ProcessBar(input);
            
            var outputValue = IsReady ? currentSupertrend : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length)
            {
                output[outputIndex++] = outputValue;
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

    private void ProcessBar(HLC<TPrice> hlc)
    {
        dataPointsReceived++;
        
        // Calculate True Range
        var trueRange = CalculateTrueRange(hlc, previousClose);
        
        // Update ATR using Wilder's smoothing
        UpdateATR(trueRange);
        
        if (isATRReady)
        {
            // Calculate basic bands
            var basicUpperBand = CalculateBasicUpperBand(hlc, currentATR);
            var basicLowerBand = CalculateBasicLowerBand(hlc, currentATR);
            
            // Calculate final bands with trend persistence
            CalculateFinalBands(basicUpperBand, basicLowerBand, hlc);
            
            // Determine Supertrend and trend direction
            CalculateSupertrendAndTrend(hlc);
            
            // Update previous values for next iteration
            previousFinalUpperBand = finalUpperBand;
            previousFinalLowerBand = finalLowerBand;
            hasPreviousBands = true;
        }
        
        previousClose = hlc.Close;
    }

    private void UpdateATR(TOutput trueRange)
    {
        if (dataPointsReceived <= Parameters.AtrPeriod)
        {
            // Initial period: accumulate true ranges
            atrSum += trueRange;
            
            if (dataPointsReceived == Parameters.AtrPeriod)
            {
                // Calculate initial ATR as simple average
                currentATR = atrSum / TOutput.CreateChecked(Parameters.AtrPeriod);
                isATRReady = true;
            }
        }
        else
        {
            // Use Wilder's smoothing: ATR = ((n-1) * PrevATR + TR) / n
            var n = TOutput.CreateChecked(Parameters.AtrPeriod);
            var nMinus1 = n - TOutput.One;
            currentATR = (nMinus1 * currentATR + trueRange) / n;
        }
    }

    private void CalculateFinalBands(TOutput basicUpperBand, TOutput basicLowerBand, HLC<TPrice> hlc)
    {
        var currentClose = TOutput.CreateChecked(Convert.ToDecimal(hlc.Close));
        
        if (!hasPreviousBands)
        {
            // First calculation
            finalUpperBand = basicUpperBand;
            finalLowerBand = basicLowerBand;
        }
        else
        {
            // Final Upper Band = basic upper band < prev final upper band OR prev close > prev final upper band 
            //                   ? basic upper band : prev final upper band
            finalUpperBand = (basicUpperBand < previousFinalUpperBand || 
                             TOutput.CreateChecked(Convert.ToDecimal(previousClose!.Value)) > previousFinalUpperBand)
                             ? basicUpperBand : previousFinalUpperBand;
            
            // Final Lower Band = basic lower band > prev final lower band OR prev close < prev final lower band 
            //                   ? basic lower band : prev final lower band
            finalLowerBand = (basicLowerBand > previousFinalLowerBand || 
                             TOutput.CreateChecked(Convert.ToDecimal(previousClose!.Value)) < previousFinalLowerBand)
                             ? basicLowerBand : previousFinalLowerBand;
        }
    }

    private void CalculateSupertrendAndTrend(HLC<TPrice> hlc)
    {
        var currentClose = TOutput.CreateChecked(Convert.ToDecimal(hlc.Close));
        
        // Determine trend direction based on close position relative to previous Supertrend
        if (!hasPreviousBands)
        {
            // First calculation: assume uptrend if close > lower band, downtrend otherwise
            if (currentClose > finalLowerBand)
            {
                currentTrendDirection = 1;  // Uptrend
                currentSupertrend = finalLowerBand;
            }
            else
            {
                currentTrendDirection = -1; // Downtrend
                currentSupertrend = finalUpperBand;
            }
        }
        else
        {
            // Check for trend change
            if (currentTrendDirection == 1) // Currently in uptrend
            {
                if (currentClose <= finalLowerBand)
                {
                    // Trend change to downtrend
                    currentTrendDirection = -1;
                    currentSupertrend = finalUpperBand;
                }
                else
                {
                    // Continue uptrend
                    currentSupertrend = finalLowerBand;
                }
            }
            else // Currently in downtrend
            {
                if (currentClose >= finalUpperBand)
                {
                    // Trend change to uptrend
                    currentTrendDirection = 1;
                    currentSupertrend = finalLowerBand;
                }
                else
                {
                    // Continue downtrend
                    currentSupertrend = finalUpperBand;
                }
            }
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        currentSupertrend = TOutput.Zero;
        currentATR = TOutput.Zero;
        currentTrendDirection = 1;
        finalUpperBand = TOutput.Zero;
        finalLowerBand = TOutput.Zero;
        previousFinalUpperBand = TOutput.Zero;
        previousFinalLowerBand = TOutput.Zero;
        previousClose = null;
        
        atrSum = TOutput.Zero;
        dataPointsReceived = 0;
        isATRReady = false;
        hasPreviousBands = false;
    }

    #endregion
}