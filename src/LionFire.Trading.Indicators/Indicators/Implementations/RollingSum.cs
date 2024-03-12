using System.Numerics;

namespace LionFire.Trading.Indicators;

public class RollingSum<T> : IIndicator<uint, T,T>
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
}

