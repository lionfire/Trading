using LionFire.Trading;
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-style implementation of Supertrend indicator using QuantConnect's ATR.
/// Since QuantConnect doesn't have a native Supertrend, this is a custom implementation
/// using QuantConnect's ATR for consistency with their ecosystem.
/// </summary>
public class Supertrend_QC<TPrice, TOutput> 
    : SupertrendBase<TPrice, TOutput>
    , IIndicator2<Supertrend_QC<TPrice, TOutput>, PSupertrend<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly global::QuantConnect.Indicators.AverageTrueRange atrIndicator;
    private TOutput currentSupertrend;
    private int currentTrendDirection = 1; // Start with uptrend assumption
    private TOutput finalUpperBand;
    private TOutput finalLowerBand;
    private TPrice? previousClose;
    
    // Band tracking
    private TOutput previousFinalUpperBand;
    private TOutput previousFinalLowerBand;
    private bool hasPreviousBands = false;
    private int dataPointsReceived = 0;

    // QuantConnect TradeBar management
    private static readonly DateTime DefaultEndTime = new(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Period = new(0, 1, 0);
    private DateTime endTime = DefaultEndTime;

    #endregion

    #region Properties

    public override TOutput Value => currentSupertrend;
    
    public override int TrendDirection => currentTrendDirection;
    
    public override TOutput CurrentATR => atrIndicator.IsReady ? 
        TOutput.CreateChecked(Convert.ToDecimal(atrIndicator.Current.Value)) : TOutput.Zero;
    
    public override bool IsReady => atrIndicator.IsReady && dataPointsReceived > Parameters.AtrPeriod;

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Supertrend indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "Supertrend",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the Supertrend indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PSupertrend<TPrice, TOutput> p)
        => [new() {
                Name = "Supertrend",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Lifecycle

    public static Supertrend_QC<TPrice, TOutput> Create(PSupertrend<TPrice, TOutput> p)
        => new Supertrend_QC<TPrice, TOutput>(p);

    public Supertrend_QC(PSupertrend<TPrice, TOutput> parameters) : base(parameters)
    {
        // Initialize QuantConnect's ATR with Wilder's smoothing (default for Supertrend)
        atrIndicator = new global::QuantConnect.Indicators.AverageTrueRange(
            parameters.AtrPeriod, 
            QuantConnect.Indicators.MovingAverageType.Wilders);
            
        currentSupertrend = TOutput.Zero;
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
        
        // Create TradeBar for QuantConnect's ATR
        var tradeBar = new TradeBar(
            time: endTime,
            symbol: QuantConnect.Symbol.None,
            open: Convert.ToDecimal(hlc.Close), // ATR doesn't need open
            high: Convert.ToDecimal(hlc.High),
            low: Convert.ToDecimal(hlc.Low),
            close: Convert.ToDecimal(hlc.Close),
            volume: 0,
            period: Period);

        // Update QuantConnect's ATR
        atrIndicator.Update(tradeBar);
        endTime += Period;
        
        if (atrIndicator.IsReady)
        {
            var atrValue = TOutput.CreateChecked(Convert.ToDecimal(atrIndicator.Current.Value));
            
            // Calculate basic bands
            var basicUpperBand = CalculateBasicUpperBand(hlc, atrValue);
            var basicLowerBand = CalculateBasicLowerBand(hlc, atrValue);
            
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
        
        atrIndicator.Reset();
        
        currentSupertrend = TOutput.Zero;
        currentTrendDirection = 1;
        finalUpperBand = TOutput.Zero;
        finalLowerBand = TOutput.Zero;
        previousFinalUpperBand = TOutput.Zero;
        previousFinalLowerBand = TOutput.Zero;
        previousClose = null;
        
        dataPointsReceived = 0;
        hasPreviousBands = false;
        endTime = DefaultEndTime;
    }

    #endregion
}