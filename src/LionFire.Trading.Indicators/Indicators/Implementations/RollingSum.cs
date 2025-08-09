#if FUTURE
using LionFire.Trading.ValueWindows;
using System.Numerics;
using System.Reactive;

namespace LionFire.Trading.Indicators;

public class RollingSum<T> : IndicatorBase<RollingSum<T>, int, T, T>, IIndicator<int, T, T>
    where T : INumber<T>
{
    public static IndicatorCharacteristics Characteristics(uint parameter)
    {
        return new IndicatorCharacteristics
        {
            Inputs = new List<IndicatorInputCharacteristics>
            {
                new IndicatorInputCharacteristics
                {
                    Name = "ClosePrice",
                    Type = typeof(double),
                    //TimeIndexed = true,
                    //TimeGaps = false,
                }
            },
            Outputs = new List<IndicatorOutputCharacteristics>
            {
                new IndicatorOutputCharacteristics
                {
                    Name = "Sum",
                    Type = typeof(double),
                }
            },
        };
    }

    public RollingSum(int period, TimeFrame timeFrame)
    {
        window = new(period, timeFrame);
    }
    
    TimeFrameValuesWindow<T> window;

    public static IndicatorCharacteristics Characteristics(int parameter)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<T> Compute(uint period, IEnumerable<T> inputs)
    {
        var result = new List<T>();

        T sum = T.Zero;
        uint count = 0;

        foreach (var input in inputs)
        {
            count++;
            sum += input;
            if (count >= period)
            {
                result.Add(sum);
            }
            else
            {
                result.Add(T.Zero);
            }
        }
        return result;
    }

    public static IIndicator<int, T, T> Create(int p)
    {
        throw new NotImplementedException();
    }

    public override void OnNext(IEnumerable<T> value)
    {
        window.PushFront(value);
        throw new NotImplementedException();
    }
}


#endif