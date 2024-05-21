namespace LionFire.Trading.ValueWindows;

public interface ITimeFrameValuesWindow : IValuesWindow
{
    DateTimeOffset FirstOpenTime { get; }
    bool HasNextExpectedOpenTime { get; }
    DateTimeOffset LastOpenTime { get; }
    DateTimeOffset NextExpectedOpenTime { get; }

    TimeSpan TimeSpan { get; }
}

public interface IReadOnlyValuesWindow<T> : IReadOnlyValuesWindow
{
    T this[int index] { get; }
}

public interface IReadOnlyTimeFrameValuesWindow<T>: IReadOnlyValuesWindow<T>
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
