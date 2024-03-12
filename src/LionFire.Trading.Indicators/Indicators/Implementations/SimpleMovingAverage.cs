using Baseline.Dates;
using CircularBuffer;
using LionFire.Trading.HistoricalData.Retrieval;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace LionFire.Trading.Indicators;

public class SimpleMovingAverage : IndicatorBase<SimpleMovingAverage, uint, double, double>, IIndicator<uint, double, double>
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
    public uint Period => Options;

    public SimpleMovingAverage(uint period)
    {
        Options = period;
        buffer = new((int)period);
    }
    public static IIndicator<uint, double, double> Create(uint p) => new SimpleMovingAverage(p);

    #region State

    CircularBuffer<double> buffer;
    double sum = 0.0;

    #endregion

    public override void OnNext(IEnumerable<double> inputs)
    {
        var s = subject;
        List<double>? result;
        if (s != null && !s.HasObservers)
        {
            subject = null;
            s = null;
            result = null;
        }
        else
        {
            result = new List<double>(inputs.Count());
        }

        foreach (var input in inputs)
        {
            if (buffer.IsFull) { sum -= buffer.Back(); }

            sum += input;
            buffer.PushFront(input);

            if (result != null)
            {
                if (buffer.IsFull)
                {
                    result.Add(sum / Period);
            }
                else
                {
                    result.Add(double.NaN);
                }
            }
        }
        if (result != null)
        {
            s!.OnNext(result);
        }
    }

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

}

