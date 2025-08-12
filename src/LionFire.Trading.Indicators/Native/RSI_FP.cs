using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of RSI using Wilder's smoothing method
/// Optimized for streaming updates
/// </summary>
public class RSI_FP<TPrice, TOutput>
    : RSIBase<TPrice, TOutput>
    , IIndicator2<RSI_FP<TPrice, TOutput>, PRSI<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private TOutput averageGain;
    private TOutput averageLoss;
    private TOutput previousPrice;
    private TOutput currentRSI;
    private int dataPointsReceived = 0;
    private bool hasPreviousPrice = false;
    
    // For initial period calculation
    private TOutput sumGains;
    private TOutput sumLosses;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentRSI;
    
    public override bool IsReady => dataPointsReceived > Parameters.Period;

    #endregion

    #region Lifecycle

    public static RSI_FP<TPrice, TOutput> Create(PRSI<TPrice, TOutput> p)
        => new RSI_FP<TPrice, TOutput>(p);

    public RSI_FP(PRSI<TPrice, TOutput> parameters) : base(parameters)
    {
        currentRSI = TOutput.CreateChecked(50); // Default neutral RSI
        averageGain = TOutput.Zero;
        averageLoss = TOutput.Zero;
        sumGains = TOutput.Zero;
        sumLosses = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput type through decimal as intermediate
            var price = TOutput.CreateChecked(Convert.ToDecimal(input));
            
            if (hasPreviousPrice)
            {
                var change = price - previousPrice;
                var gain = change > TOutput.Zero ? change : TOutput.Zero;
                var loss = change < TOutput.Zero ? -change : TOutput.Zero;
                
                UpdateRSI(gain, loss);
            }
            
            previousPrice = price;
            hasPreviousPrice = true;
            dataPointsReceived++;
            
            var outputValue = IsReady ? currentRSI : MissingOutputValue;
            
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

    #region RSI Calculation

    private void UpdateRSI(TOutput gain, TOutput loss)
    {
        if (dataPointsReceived <= Parameters.Period)
        {
            // Initial period: accumulate gains and losses
            sumGains += gain;
            sumLosses += loss;
            
            if (dataPointsReceived == Parameters.Period)
            {
                // Calculate initial averages
                var period = TOutput.CreateChecked(Parameters.Period);
                averageGain = sumGains / period;
                averageLoss = sumLosses / period;
                CalculateRSI();
            }
        }
        else
        {
            // Use Wilder's smoothing: Average = ((n-1) * PrevAverage + CurrentValue) / n
            var n = TOutput.CreateChecked(Parameters.Period);
            var nMinus1 = n - TOutput.One;
            
            averageGain = (nMinus1 * averageGain + gain) / n;
            averageLoss = (nMinus1 * averageLoss + loss) / n;
            
            CalculateRSI();
        }
    }
    
    private void CalculateRSI()
    {
        if (averageLoss == TOutput.Zero)
        {
            // If there are no losses, RSI is 100
            currentRSI = TOutput.CreateChecked(100);
        }
        else
        {
            // RSI = 100 - (100 / (1 + RS))
            // where RS = Average Gain / Average Loss
            var rs = averageGain / averageLoss;
            var hundred = TOutput.CreateChecked(100);
            currentRSI = hundred - (hundred / (TOutput.One + rs));
        }
        
        // Ensure RSI is within bounds [0, 100]
        var zero = TOutput.Zero;
        var hundred100 = TOutput.CreateChecked(100);
        if (currentRSI < zero) currentRSI = zero;
        if (currentRSI > hundred100) currentRSI = hundred100;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        previousPrice = TOutput.Zero;
        currentRSI = TOutput.CreateChecked(50);
        averageGain = TOutput.Zero;
        averageLoss = TOutput.Zero;
        sumGains = TOutput.Zero;
        sumLosses = TOutput.Zero;
        dataPointsReceived = 0;
        hasPreviousPrice = false;
    }

    #endregion
}