﻿using Baseline.Dates;
using CircularBuffer;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.IO;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TInput = System.Double;
using TOutput = System.Double;

namespace LionFire.Trading.Indicators;

public class SimpleMovingAverage
    : SingleInputIndicatorBase<SimpleMovingAverage, uint, double, double>
    , IIndicator2<SimpleMovingAverage, uint, double, double>
{
    #region Static

    public static IReadOnlyList<InputSlot> InputSlots()
        => [new () {
                    Name = "Source",
                    Type = typeof(double),
                }];
    public static IReadOnlyList<OutputSlot> Outputs()
            => [new () {
                     Name = "Average",
                    Type = typeof(double),
                }];

    //public static IOComponent Characteristics(uint parameter)
    //{
    //    return new IOComponent
    //    {
    //        InputSignals = new List<InputSlot>
    //        {
    //            new InputSlot
    //            {
    //                Name = "Source",
    //                Type = typeof(double),
    //            }
    //        },
    //        Outputs = new List<OutputSlot>
    //        {
    //            new OutputSlot
    //            {
    //                Name = "Average",
    //                Type = typeof(double),
    //            }
    //        },
    //    };
    //}

    #endregion

    public uint Options { get; init; }

    #region Parameters

    #region Derived

    public uint Period => Options;
    public override uint MaxLookback => Options;

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
    public override bool IsReady => buffer.IsFull;

    #endregion

    // MOVE to base class
  

    public override int OnNext(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        int outputCount = 0;
        foreach (var input in inputs)
        {
            if (buffer.IsFull) { sum -= buffer.Back(); }

            sum += input;
            buffer.PushFront(input);

            if (output != null)
            {
                if (outputSkip > 0) { outputSkip--; }
                else
                {
                    output[outputIndex] = buffer.IsFull ? sum / Period : double.NaN;
                    outputCount++;
                }
            }
        }
        return outputCount;
    }

    #region Methods

    public override void Clear()
    {
        base.Clear();
        buffer.Clear();
        sum = 0.0;
    }

    #endregion

    #region OLD
    // REFACTOR - genericize?
    //public static IEnumerable<double> Compute(uint period, IEnumerable<double> inputs)
    //{
    //    var x = new SimpleMovingAverage(period);
    //    var result = new List<double>();
    //    var d = x.Subscribe(v => result.Add(v));
    //    x.OnInput(inputs);
    //    d.Dispose();
    //    return result;
    //}
    #endregion

    //#region InputSignal Handling

    //// TODO: Can this be moved to a base class somehow?

    //public override async ValueTask<(object, int)> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var x = (IHistoricalTimeSeries<double>)sources[0];

    //    var d = await x.Get(start, endExclusive).ConfigureAwait(false);

    //    if (!d.IsSuccess || d.Items?.Any() != true) throw new Exception("Failed to get data");

    //    return (d.Items, d.Items.Count);
    //}

    //public override void OnNextFromArray(object inputData, int index)
    //{
    //    var x = (IReadOnlyList<double>)inputData;
    //    OnNext(x[index]);
    //}

    //#endregion
}

