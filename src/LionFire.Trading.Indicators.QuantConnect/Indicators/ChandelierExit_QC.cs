using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Chandelier Exit indicator.
/// Uses QuantConnect's ATR for volatility calculation while implementing
/// the Chandelier Exit logic (highest/lowest tracking + ATR offset).
/// </summary>
/// <remarks>
/// Note: QuantConnect does not have a built-in Chandelier Exit indicator.
/// This implementation uses QC's ATR and implements the exit logic manually.
/// </remarks>
public class ChandelierExit_QC<TPrice, TOutput>
    : IChandelierExit<TPrice, TOutput>
    , IIndicator2<ChandelierExit_QC<TPrice, TOutput>, PChandelierExit<TPrice, TOutput>, HLC<TPrice>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TPrice>>>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    static ChandelierExit_QC()
    {
        if (typeof(TOutput) == typeof(double)) { ConvertToOutput = (v) => (TOutput)(object)Convert.ToDouble(v); }
        else if (typeof(TOutput) == typeof(float)) { ConvertToOutput = (v) => (TOutput)(object)Convert.ToSingle(v); }
        else if (typeof(TOutput) == typeof(decimal)) { ConvertToOutput = (v) => (TOutput)(object)v; }
        else ConvertToOutput = _ => throw new NotSupportedException($"Not implemented: conversion from decimal to {typeof(TOutput).FullName}");
    }

    public static Func<decimal, TOutput> ConvertToOutput;

    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "ExitLong", ValueType = typeof(TOutput) },
            new() { Name = "ExitShort", ValueType = typeof(TOutput) }
        ];

    public static List<OutputSlot> Outputs(PChandelierExit<TPrice, TOutput> p)
        => [
            new() { Name = "ExitLong", ValueType = typeof(TOutput) },
            new() { Name = "ExitShort", ValueType = typeof(TOutput) }
        ];

    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Parameters

    public readonly PChandelierExit<TPrice, TOutput> Parameters;

    public int Period => Parameters.Period;
    public TOutput AtrMultiplier => Parameters.AtrMultiplier;
    public int MaxLookback => Parameters.Period;

    #endregion

    #region State

    private readonly AverageTrueRange qcAtr;
    private readonly decimal[] highBuffer;
    private readonly decimal[] lowBuffer;
    private int bufferIndex = 0;
    private int bufferCount = 0;

    private TOutput exitLong;
    private TOutput exitShort;
    private TOutput currentATR;
    private TOutput highestHigh;
    private TOutput lowestLow;

    // Time tracking for QC
    private static readonly DateTime DefaultEndTime = new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan BarPeriod = new TimeSpan(0, 1, 0);
    private DateTime endTime = DefaultEndTime;

    protected System.Reactive.Subjects.Subject<IReadOnlyList<TOutput>>? subject;

    #endregion

    #region Properties

    public TOutput ExitLong => exitLong;
    public TOutput ExitShort => exitShort;
    public TOutput CurrentATR => currentATR;
    public TOutput HighestHigh => highestHigh;
    public TOutput LowestLow => lowestLow;
    public bool IsReady => qcAtr.IsReady && bufferCount >= Parameters.Period;

    #endregion

    #region Lifecycle

    public static ChandelierExit_QC<TPrice, TOutput> Create(PChandelierExit<TPrice, TOutput> p)
        => new ChandelierExit_QC<TPrice, TOutput>(p);

    public ChandelierExit_QC(PChandelierExit<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

        if (parameters.Period < 2)
            throw new ArgumentException("Period must be at least 2", nameof(parameters.Period));

        if (parameters.AtrMultiplier <= TOutput.Zero)
            throw new ArgumentException("AtrMultiplier must be greater than zero");

        // Initialize QuantConnect ATR with Wilder's smoothing
        qcAtr = new AverageTrueRange("ATR", parameters.Period, global::QuantConnect.Indicators.MovingAverageType.Wilders);

        // Initialize buffers for highest/lowest tracking
        highBuffer = new decimal[parameters.Period];
        lowBuffer = new decimal[parameters.Period];

        exitLong = MissingOutputValue;
        exitShort = MissingOutputValue;
        currentATR = TOutput.Zero;
        highestHigh = TOutput.Zero;
        lowestLow = TOutput.Zero;
    }

    #endregion

    #region IObservable Implementation

    public IDisposable Subscribe(IObserver<IReadOnlyList<TOutput>> observer)
    {
        subject ??= new();
        return subject.Subscribe(observer);
    }

    #endregion

    #region IObserver Implementation

    public void OnNext(IReadOnlyList<HLC<TPrice>> value)
    {
        TOutput[]? output = null;
        var s = subject;

        if (s != null && !s.HasObservers)
        {
            subject = null;
            s = null;
        }
        else if (s != null)
        {
            output = new TOutput[value.Count];
        }

        OnBarBatch(value, output, 0, 0);

        if (s != null && output != null)
        {
            s.OnNext(output);
        }
    }

    public void OnNext(HLC<TPrice> value) => OnNext(new[] { value });

    public void OnCompleted() => subject?.OnCompleted();

    public void OnError(Exception error) => subject?.OnError(error);

    #endregion

    #region Event Handling

    public void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
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

    private void ProcessBar(HLC<TPrice> hlc)
    {
        var high = Convert.ToDecimal(hlc.High);
        var low = Convert.ToDecimal(hlc.Low);
        var close = Convert.ToDecimal(hlc.Close);

        // Update QC ATR
        var tradeBar = new TradeBar(
            time: endTime,
            symbol: global::QuantConnect.Symbol.None,
            open: close, // Open not used for ATR
            high: high,
            low: low,
            close: close,
            volume: 0,
            period: BarPeriod);

        qcAtr.Update(tradeBar);
        endTime += BarPeriod;

        // Update rolling window for highest/lowest
        highBuffer[bufferIndex] = high;
        lowBuffer[bufferIndex] = low;
        bufferIndex = (bufferIndex + 1) % Parameters.Period;
        if (bufferCount < Parameters.Period)
        {
            bufferCount++;
        }

        // Calculate highest high and lowest low
        decimal maxHigh = decimal.MinValue;
        decimal minLow = decimal.MaxValue;

        for (int i = 0; i < bufferCount; i++)
        {
            if (highBuffer[i] > maxHigh) maxHigh = highBuffer[i];
            if (lowBuffer[i] < minLow) minLow = lowBuffer[i];
        }

        highestHigh = ConvertToOutput(maxHigh);
        lowestLow = ConvertToOutput(minLow);

        if (qcAtr.IsReady && bufferCount >= Parameters.Period)
        {
            currentATR = ConvertToOutput(qcAtr.Current.Price);

            // Calculate Chandelier Exit values
            var atrOffset = AtrMultiplier * currentATR;

            // Exit Long = Highest High - ATR × Multiplier
            exitLong = highestHigh - atrOffset;

            // Exit Short = Lowest Low + ATR × Multiplier
            exitShort = lowestLow + atrOffset;
        }
    }

    #endregion

    #region Methods

    public void Clear()
    {
        subject?.OnCompleted();
        subject = null;

        qcAtr.Reset();
        Array.Clear(highBuffer, 0, highBuffer.Length);
        Array.Clear(lowBuffer, 0, lowBuffer.Length);
        bufferIndex = 0;
        bufferCount = 0;
        endTime = DefaultEndTime;

        exitLong = MissingOutputValue;
        exitShort = MissingOutputValue;
        currentATR = TOutput.Zero;
        highestHigh = TOutput.Zero;
        lowestLow = TOutput.Zero;
    }

    #endregion
}
