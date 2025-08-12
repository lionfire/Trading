using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of ADX (Average Directional Index) using Wilder's smoothing method
/// Optimized for streaming updates with circular buffers
/// </summary>
public class ADX_FP<TInput, TOutput>
    : ADXBase<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    // State tracking
    private int dataPointsReceived = 0;
    private bool hasPreviousBar = false;
    private HLC<TInput> previousBar;

    // Current values
    private TOutput currentADX;
    private TOutput currentPlusDI;
    private TOutput currentMinusDI;

    // True Range calculation
    private TOutput averageTrueRange;
    private TOutput sumTrueRange;

    // Directional Movement calculation  
    private TOutput averagePlusDM;
    private TOutput averageMinusDM;
    private TOutput sumPlusDM;
    private TOutput sumMinusDM;

    // DX values for ADX calculation
    private TOutput averageDX;
    private TOutput sumDX;
    private readonly TOutput[] dxBuffer;
    private int dxBufferIndex;
    private int dxCount;

    #endregion

    #region Properties

    public override TOutput ADX => IsReady ? currentADX : MissingOutputValue;

    public override TOutput PlusDI => IsReady ? currentPlusDI : MissingOutputValue;

    public override TOutput MinusDI => IsReady ? currentMinusDI : MissingOutputValue;

    public override bool IsReady => dataPointsReceived > (Parameters.Period * 2); // Need extra period for ADX calculation

    #endregion

    #region Lifecycle

    public static ADX_FP<TInput, TOutput> Create(PADX<TInput, TOutput> p)
        => new ADX_FP<TInput, TOutput>(p);

    public ADX_FP(PADX<TInput, TOutput> parameters) : base(parameters)
    {
        // Initialize buffers
        dxBuffer = new TOutput[parameters.Period];
        dxBufferIndex = 0;
        dxCount = 0;

        // Initialize values
        currentADX = TOutput.Zero;
        currentPlusDI = TOutput.Zero;
        currentMinusDI = TOutput.Zero;
        
        averageTrueRange = TOutput.Zero;
        averagePlusDM = TOutput.Zero;
        averageMinusDM = TOutput.Zero;
        averageDX = TOutput.Zero;
        
        sumTrueRange = TOutput.Zero;
        sumPlusDM = TOutput.Zero;
        sumMinusDM = TOutput.Zero;
        sumDX = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TInput>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            if (hasPreviousBar)
            {
                ProcessBar(input);
            }

            previousBar = input;
            hasPreviousBar = true;
            dataPointsReceived++;

            // Output current values
            var adxValue = IsReady ? currentADX : MissingOutputValue;
            var plusDIValue = IsReady ? currentPlusDI : MissingOutputValue;
            var minusDIValue = IsReady ? currentMinusDI : MissingOutputValue;

            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex + 2 < output.Length)
            {
                output[outputIndex++] = adxValue;
                output[outputIndex++] = plusDIValue;
                output[outputIndex++] = minusDIValue;
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

    #region ADX Calculation

    private void ProcessBar(HLC<TInput> currentBar)
    {
        // Convert inputs to TOutput
        var high = TOutput.CreateChecked(Convert.ToDecimal(currentBar.High));
        var low = TOutput.CreateChecked(Convert.ToDecimal(currentBar.Low));
        var close = TOutput.CreateChecked(Convert.ToDecimal(currentBar.Close));
        
        var prevHigh = TOutput.CreateChecked(Convert.ToDecimal(previousBar.High));
        var prevLow = TOutput.CreateChecked(Convert.ToDecimal(previousBar.Low));
        var prevClose = TOutput.CreateChecked(Convert.ToDecimal(previousBar.Close));

        // Calculate True Range
        var tr1 = high - low;
        var tr2 = Abs(high - prevClose);
        var tr3 = Abs(low - prevClose);
        var trueRange = Max(tr1, Max(tr2, tr3));

        // Calculate Directional Movement
        var highDiff = high - prevHigh;
        var lowDiff = prevLow - low;

        var plusDM = (highDiff > lowDiff && highDiff > TOutput.Zero) ? highDiff : TOutput.Zero;
        var minusDM = (lowDiff > highDiff && lowDiff > TOutput.Zero) ? lowDiff : TOutput.Zero;

        // Update smoothed averages
        UpdateAverages(trueRange, plusDM, minusDM);

        // Calculate Directional Indicators
        if (averageTrueRange != TOutput.Zero)
        {
            var hundred = TOutput.CreateChecked(100);
            currentPlusDI = (averagePlusDM / averageTrueRange) * hundred;
            currentMinusDI = (averageMinusDM / averageTrueRange) * hundred;

            // Calculate DX (Directional Index)
            var diSum = currentPlusDI + currentMinusDI;
            var diDiff = Abs(currentPlusDI - currentMinusDI);
            
            if (diSum != TOutput.Zero)
            {
                var dx = (diDiff / diSum) * hundred;
                UpdateADX(dx);
            }
        }
    }

    private void UpdateAverages(TOutput trueRange, TOutput plusDM, TOutput minusDM)
    {
        if (dataPointsReceived <= Parameters.Period)
        {
            // Initial period: accumulate sums
            sumTrueRange += trueRange;
            sumPlusDM += plusDM;
            sumMinusDM += minusDM;

            if (dataPointsReceived == Parameters.Period)
            {
                // Calculate initial averages
                var period = TOutput.CreateChecked(Parameters.Period);
                averageTrueRange = sumTrueRange / period;
                averagePlusDM = sumPlusDM / period;
                averageMinusDM = sumMinusDM / period;
            }
        }
        else
        {
            // Use Wilder's smoothing: Average = ((n-1) * PrevAverage + CurrentValue) / n
            var n = TOutput.CreateChecked(Parameters.Period);
            var nMinus1 = n - TOutput.One;

            averageTrueRange = (nMinus1 * averageTrueRange + trueRange) / n;
            averagePlusDM = (nMinus1 * averagePlusDM + plusDM) / n;
            averageMinusDM = (nMinus1 * averageMinusDM + minusDM) / n;
        }
    }

    private void UpdateADX(TOutput dx)
    {
        // Use circular buffer for DX values
        if (dxCount >= Parameters.Period)
        {
            sumDX -= dxBuffer[dxBufferIndex];
        }

        dxBuffer[dxBufferIndex] = dx;
        sumDX += dx;

        dxBufferIndex = (dxBufferIndex + 1) % Parameters.Period;

        if (dxCount < Parameters.Period)
        {
            dxCount++;
        }

        // Calculate ADX once we have enough DX values
        if (dxCount >= Parameters.Period)
        {
            if (dataPointsReceived <= Parameters.Period * 2)
            {
                // Initial ADX calculation (simple average of DX values)
                var period = TOutput.CreateChecked(Parameters.Period);
                currentADX = sumDX / period;
            }
            else
            {
                // Use Wilder's smoothing for ADX
                var n = TOutput.CreateChecked(Parameters.Period);
                var nMinus1 = n - TOutput.One;
                currentADX = (nMinus1 * currentADX + dx) / n;
            }
        }
    }

    #endregion

    #region Helper Methods

    private static TOutput Max(TOutput a, TOutput b)
    {
        return a > b ? a : b;
    }

    private static TOutput Abs(TOutput value)
    {
        return value >= TOutput.Zero ? value : -value;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        dataPointsReceived = 0;
        hasPreviousBar = false;
        
        currentADX = TOutput.Zero;
        currentPlusDI = TOutput.Zero;
        currentMinusDI = TOutput.Zero;
        
        averageTrueRange = TOutput.Zero;
        averagePlusDM = TOutput.Zero;
        averageMinusDM = TOutput.Zero;
        averageDX = TOutput.Zero;
        
        sumTrueRange = TOutput.Zero;
        sumPlusDM = TOutput.Zero;
        sumMinusDM = TOutput.Zero;
        sumDX = TOutput.Zero;
        
        Array.Clear(dxBuffer, 0, dxBuffer.Length);
        dxBufferIndex = 0;
        dxCount = 0;
    }

    #endregion
}