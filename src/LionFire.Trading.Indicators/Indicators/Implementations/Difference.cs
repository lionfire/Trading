using System.Reactive;

namespace LionFire.Trading.Indicators;

public class Difference<T> : IndicatorBase<Difference<T>, Unit, (T, T), T>, IIndicator<Unit, (T, T), T>
{
    public static IndicatorCharacteristics Characteristics(Unit parameter)
    {
        return new IndicatorCharacteristics
        {
            Inputs = new List<IndicatorInputCharacteristics>
            {
                new IndicatorInputCharacteristics
                {
                    Name = "Sources",
                    Type = typeof(T),
                }
            },
            Outputs = new List<IndicatorOutputCharacteristics>
            {
                new IndicatorOutputCharacteristics
                {
                    Name = "Difference",
                    Type = typeof(T),
                }
            },
        };
    }

    public static IIndicator<Unit, (T, T), T> Create(Unit p)
    {
        throw new NotImplementedException();
    }

    //public override void OnNext((T, T) value)
    //{
    //    throw new NotImplementedException();
    //}

    public override void OnNext(IEnumerable<(T, T)> value)
    {
        throw new NotImplementedException();
    }
}

