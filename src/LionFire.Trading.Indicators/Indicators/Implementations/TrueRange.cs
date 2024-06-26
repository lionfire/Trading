﻿#if TODO
using Baseline.Dates;
using CircularBuffer;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace LionFire.Trading.Indicators;

public class SimpleMovingAverage
    : SingleInputIndicatorBase<SimpleMovingAverage, uint, double, double>
    , IIndicator<SimpleMovingAverage, uint, double, double>
{
    #region Static

    public static IndicatorCharacteristics Characteristics(uint parameter)
    {
        return new IndicatorCharacteristics
        {
            Inputs = new List<IndicatorInputCharacteristics>
            {
                new IndicatorInputCharacteristics
                {
                    Name = "Source",
                    Type = typeof(double),
                }
            },
            Outputs = new List<IndicatorOutputCharacteristics>
            {
                new IndicatorOutputCharacteristics
                {
                    Name = "Average",
                    Type = typeof(double),
                }
            },
        };
    }

    #endregion

    public uint Options { get; init; }

    #region Parameters

    #region Derived

    public uint Period => Options;
    public override uint Lookback => Options;

    #endregion

    #endregion

    #region Lifecycle

    public SimpleMovingAverage(uint period)
    {
        Options = period;
        buffer = new((int)period);
    }
    public static SimpleMovingAverage Create(uint p) => new SimpleMovingAverage(p);

    #endregion

    #region State

    CircularBuffer<double> buffer;
    double sum = 0.0;

    #endregion

    public override void OnNext(IReadOnlyList<double> inputs)
    {
        var s = subject;
        List<double>? output;
        if (s != null && !s.HasObservers)
        {
            subject = null;
            s = null;
            output = null;
        }
        else
        {
            output = new List<double>(inputs.Count);
        }

        foreach (var input in inputs)
        {
            if (buffer.IsFull) { sum -= buffer.Back(); }

            sum += input;
            buffer.PushFront(input);

            if (output != null)
            {
                if (buffer.IsFull)
                {
                    output.Add(sum / Period);
                }
                else
                {
                    output.Add(double.NaN);
                }
            }
        }
        if (output != null && s != null)
        {
            s.OnNext(output);
        }
    }

    #region Methods

    public override void Clear()
    {
        base.Clear();
        buffer.Clear();
        sum = 0.0;
    }

    #endregion
}

#endif