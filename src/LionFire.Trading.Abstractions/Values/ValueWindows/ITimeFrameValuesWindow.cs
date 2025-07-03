using System.Numerics;

namespace LionFire.Trading.ValueWindows;

public interface ITimeFrameValuesWindow : IValuesWindow
{
    DateTimeOffset FirstOpenTime { get; }
    bool HasNextExpectedOpenTime { get; }
    DateTimeOffset LastOpenTime { get; }
    DateTimeOffset NextExpectedOpenTime { get; }

    TimeSpan TimeSpan { get; }
}

public enum PriceSubscriptionDirection
{
    Unspecified = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    UpOrDown = Up | Down,
}

public interface IReadOnlyValuesWindow<TValue> : IReadOnlyValuesWindow
{
    TValue this[int index] { get; }

}
public interface IReadOnlyValuesWindow<TValue, TPrecision> : IReadOnlyValuesWindow<TValue>
    where TPrecision : struct, INumber<TPrecision>
{
    void SubscribeToPrice(TPrecision triggerValue, Action<TValue> onReached, PriceSubscriptionDirection direction = PriceSubscriptionDirection.UpOrDown);
}

//public interface IReadOnlyValuesWindow<T, TPrecision> : IReadOnlyValuesWindow<T>
//    where T : IHasPrecision
//{
//    IDisposable SubscribeToPrice(TPrecision triggerValue, Action<(TPrecision oldValue, TPrecision newValue)> onReached);
//}

//public interface ILookbackSubscription : IDisposable
//{
//    ValueTask Lookback(int size);
//    ValueTask<int> Lookback();

//}

public interface IReadOnlyTimeFrameValuesWindow<T> : IReadOnlyValuesWindow<T>
{
    (DateTimeOffset lastOpenTime, IList<ArraySegment<T>> arraySegments) ReversedValuesBufferWithTime { get; }
    (DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime, T[] reverseValues) Values { get; }
    (DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime, T[] reverseValues) ValuesReverse { get; }

    IEnumerable<T>? TryGetReverseValues(DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime);
    IEnumerable<T>? TryGetValues(DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime);
}

public interface ITimeFrameValuesWindow<T> : IValuesWindow<T>, ITimeFrameValuesWindow, IReadOnlyTimeFrameValuesWindow<T>
{
    T PopBack();
    uint Push(T value, DateTimeOffset openTime, bool front);
    uint PushBack(IReadOnlyList<T> values);
    uint PushBack(T value);
    uint PushFront(IReadOnlyList<T> values);
    uint PushFront(T value);
}
