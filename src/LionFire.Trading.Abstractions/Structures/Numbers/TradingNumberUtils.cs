using System.Numerics;

namespace LionFire.Trading;

public static class TradingValueUtils<T>
{
    public static T MissingValue { get; set; }
    public static T Zero { get; set; }

    static TradingValueUtils()
    {
        if (typeof(T).IsAssignableTo(typeof(INumber<>).MakeGenericType(typeof(T))))
        {
            MissingValue = (T)(object)typeof(TradingNumberUtils<>).MakeGenericType(typeof(T)).GetProperty(nameof(TradingNumberUtils<int>.MissingValue))!.GetValue(null)!;

            if (typeof(T) == typeof(double))
            {
                Zero = (T)(object)0.0;
            }
            else if (typeof(T) == typeof(decimal))
            {
                Zero = (T)(object)0.0M;
            }
            else
            {
                Zero = (T)(object)typeof(INumberBase<>).MakeGenericType(typeof(T)).GetProperty(nameof(INumber<int>.Zero))!.GetValue(null)!;
            }
        }
        else
        {
            MissingValue = default!;
            Zero = default!;
        }
    }
}

public static class TradingNumberUtils<T>
    where T : INumber<T>
{
    public static T MissingValue
    {
        get
        {
            if (typeof(T) == typeof(double)) return (T)(object)double.NaN;
            else if (typeof(T) == typeof(float)) return (T)(object)float.NaN;
            else if (typeof(T) == typeof(decimal)) return (T)(object)decimal.MinValue;
            else return default!;
        }
    }
}
