using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Chandelier Exit using efficient ATR calculation
/// Optimized for streaming updates with rolling highest/lowest tracking
/// </summary>
public class ChandelierExit_FP<TPrice, TOutput>
    : ChandelierExitBase<TPrice, TOutput>
    , IIndicator2<ChandelierExit_FP<TPrice, TOutput>, PChandelierExit<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private TOutput currentATR;
    private TOutput exitLong;
    private TOutput exitShort;
    private TOutput highestHigh;
    private TOutput lowestLow;
    private TPrice? previousClose;

    // ATR calculation using Wilder's smoothing
    private TOutput atrSum;
    private int dataPointsReceived = 0;
    private bool isATRReady = false;

    // Rolling window for highest/lowest tracking
    private readonly TOutput[] highBuffer;
    private readonly TOutput[] lowBuffer;
    private int bufferIndex = 0;
    private int bufferCount = 0;

    #endregion

    #region Properties

    public override TOutput ExitLong => exitLong;

    public override TOutput ExitShort => exitShort;

    public override TOutput CurrentATR => currentATR;

    public override TOutput HighestHigh => highestHigh;

    public override TOutput LowestLow => lowestLow;

    public override bool IsReady => isATRReady && bufferCount >= Parameters.Period;

    #endregion

    #region Lifecycle

    public static ChandelierExit_FP<TPrice, TOutput> Create(PChandelierExit<TPrice, TOutput> p)
        => new ChandelierExit_FP<TPrice, TOutput>(p);

    public ChandelierExit_FP(PChandelierExit<TPrice, TOutput> parameters) : base(parameters)
    {
        currentATR = TOutput.Zero;
        atrSum = TOutput.Zero;
        exitLong = MissingOutputValue;
        exitShort = MissingOutputValue;
        highestHigh = TOutput.Zero;
        lowestLow = TOutput.Zero;

        // Initialize rolling window buffers
        highBuffer = new TOutput[parameters.Period];
        lowBuffer = new TOutput[parameters.Period];
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            ProcessBar(input);

            var outputValue = IsReady ? exitLong : MissingOutputValue;

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

        var currentHigh = TOutput.CreateChecked(Convert.ToDecimal(hlc.High));
        var currentLow = TOutput.CreateChecked(Convert.ToDecimal(hlc.Low));

        // Calculate True Range
        var trueRange = CalculateTrueRange(hlc, previousClose);

        // Update ATR using Wilder's smoothing
        UpdateATR(trueRange);

        // Update rolling window for highest high / lowest low
        UpdateHighLowBuffers(currentHigh, currentLow);

        if (isATRReady && bufferCount >= Parameters.Period)
        {
            // Calculate Chandelier Exit values
            // Exit Long = Highest High - ATR × Multiplier (trailing stop for longs)
            exitLong = highestHigh - (AtrMultiplier * currentATR);

            // Exit Short = Lowest Low + ATR × Multiplier (trailing stop for shorts)
            exitShort = lowestLow + (AtrMultiplier * currentATR);
        }

        previousClose = hlc.Close;
    }

    private void UpdateATR(TOutput trueRange)
    {
        if (dataPointsReceived <= Parameters.Period)
        {
            // Initial period: accumulate true ranges
            atrSum += trueRange;

            if (dataPointsReceived == Parameters.Period)
            {
                // Calculate initial ATR as simple average
                currentATR = atrSum / TOutput.CreateChecked(Parameters.Period);
                isATRReady = true;
            }
        }
        else
        {
            // Use Wilder's smoothing: ATR = ((n-1) * PrevATR + TR) / n
            var n = TOutput.CreateChecked(Parameters.Period);
            var nMinus1 = n - TOutput.One;
            currentATR = (nMinus1 * currentATR + trueRange) / n;
        }
    }

    private void UpdateHighLowBuffers(TOutput high, TOutput low)
    {
        // Add to circular buffer
        highBuffer[bufferIndex] = high;
        lowBuffer[bufferIndex] = low;

        bufferIndex = (bufferIndex + 1) % Parameters.Period;
        if (bufferCount < Parameters.Period)
        {
            bufferCount++;
        }

        // Recalculate highest high and lowest low
        // Note: For production, could optimize with a more efficient data structure
        // like a monotonic deque, but this is sufficient for typical use cases
        highestHigh = TOutput.Zero;
        lowestLow = TOutput.CreateChecked(decimal.MaxValue);

        for (int i = 0; i < bufferCount; i++)
        {
            if (highBuffer[i] > highestHigh)
            {
                highestHigh = highBuffer[i];
            }
            if (lowBuffer[i] < lowestLow)
            {
                lowestLow = lowBuffer[i];
            }
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;

        currentATR = TOutput.Zero;
        exitLong = MissingOutputValue;
        exitShort = MissingOutputValue;
        highestHigh = TOutput.Zero;
        lowestLow = TOutput.Zero;
        previousClose = null;

        atrSum = TOutput.Zero;
        dataPointsReceived = 0;
        isATRReady = false;

        Array.Clear(highBuffer, 0, highBuffer.Length);
        Array.Clear(lowBuffer, 0, lowBuffer.Length);
        bufferIndex = 0;
        bufferCount = 0;
    }

    #endregion
}
