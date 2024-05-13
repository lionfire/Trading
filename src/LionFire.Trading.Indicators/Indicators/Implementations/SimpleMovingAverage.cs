using Baseline.Dates;
using CircularBuffer;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.IO;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TInput = System.Double;
using TOutput = System.Double;

namespace LionFire.Trading.Indicators;

public class PSimpleMovingAverage<TOutput> : IndicatorParameters<SimpleMovingAverage, TOutput>
{
    public int Period { get; set; }

    public PSimpleMovingAverage()
    {
        if (typeof(TOutput) != typeof(double)) throw new NotImplementedException("TOutput must be double"); // TODO - remove this limitation
    }
    public static implicit operator PSimpleMovingAverage<TOutput>(int period)
    {
        return new PSimpleMovingAverage<TOutput> { Period = period };
    }
}

// TODO: Generic TOutput
public class SimpleMovingAverage
    : SingleInputIndicatorBase<SimpleMovingAverage, PSimpleMovingAverage<double>, double, double>
    , IIndicator2<SimpleMovingAverage, PSimpleMovingAverage<double>, double, double>
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

    public int Options { get; init; }

    #region Parameters

    #region Derived

    public int Period => Options;
    public override int MaxLookback => Options;

    #endregion

    #endregion

    #region Lifecycle

    public SimpleMovingAverage(PSimpleMovingAverage<double> parameters)
    {
        Options = parameters.Period;
        buffer = new(parameters.Period);
    }

    public static SimpleMovingAverage Create(PSimpleMovingAverage<double> p) => new SimpleMovingAverage(p.Period);

    #endregion

    #region State

    CircularBuffer<double> buffer;
    double sum = 0.0;
    public override bool IsReady => buffer.IsFull;

    #endregion

    // MOVE to base class


    public override void OnNext(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        // OPTIMIZE - is there a way to avoid repeated bounds checking on the output array?
        // var maxOutputIndex = outputIndex + inputs.Count - outputSkip;
        // https://blog.tedd.no/2020/06/01/faster-c-array-access/

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
                    output[outputIndex++] = buffer.IsFull ? sum / Period : double.NaN;
                }
            }
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

