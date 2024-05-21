using System.Diagnostics;
using System.Linq;

namespace LionFire.Trading.ValueWindows;

public class TimeFrameValuesWindow<T> : ValuesWindowBase<T>, ITimeFrameValuesWindow<T>
{
    #region Parameters

    public TimeSpan TimeSpan { get; }

    #endregion

    #region Lifecycle

    public TimeFrameValuesWindow(uint period, TimeSpan timeSpan, DateTimeOffset? nextOpenTime = null) : base(period)
    {
        TimeSpan = timeSpan;
        LastOpenTime = nextOpenTime == null ? DateTimeOffset.MinValue : nextOpenTime.Value - timeSpan;
    }
    public TimeFrameValuesWindow(uint period, TimeFrame timeFrame, DateTimeOffset? nextOpenTime = null) : base(period)
    {
        if (timeFrame.TimeSpan < TimeSpan.Zero) throw new NotImplementedException("TODO: Use timeFrame.AddBars");
        TimeSpan = timeFrame.TimeSpan;
        LastOpenTime = nextOpenTime == null ? DateTimeOffset.MinValue : timeFrame.AddBars(nextOpenTime.Value, -1);
    }

    #endregion

    #region State

    public DateTimeOffset LastOpenTime { get; protected set; }

    #region Derived

    public DateTimeOffset FirstOpenTime => LastOpenTime - TimeSpan * Math.Min(TotalValuesSeen, values.Capacity);
    public DateTimeOffset NextExpectedOpenTime => LastOpenTime + TimeSpan;
    public bool HasNextExpectedOpenTime => LastOpenTime != DateTimeOffset.MinValue;

    #endregion

    #endregion

    #region Values accessors

    // TODO ENH Convenience: a new struct type, or Extension methods to get (Value, DateTimeOffset OpenTime) tuples from these accessors

    /// <summary>
    /// Values in reverse chronological order
    /// </summary>
    public (DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime, T[] reverseValues) ValuesReverse => (FirstOpenTime, LastOpenTime, values.ToArray());
    public T this[int index] => values[index];

    /// <summary>
    /// Values in reverse chronological order, with raw access to the buffer
    /// </summary>
    public (DateTimeOffset lastOpenTime, IList<ArraySegment<T>> arraySegments) ReversedValuesBufferWithTime => (LastOpenTime, values.ToArraySegments());

    /// <summary>
    /// Values in chronological order
    /// </summary>
    public (DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime, T[] reverseValues) Values => (FirstOpenTime, LastOpenTime, values.ToReversedArray());

    public IEnumerable<T>? TryGetValues(DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime)
    {
        return TryGetReverseValues(firstOpenTime, lastOpenTime)?.Reverse();
    }

    // OPTIMIZE: direct buffer access
    public IEnumerable<T>? TryGetReverseValues(DateTimeOffset firstOpenTime, DateTimeOffset lastOpenTime)
    {
        if (firstOpenTime < FirstOpenTime) return null;
        if (lastOpenTime > LastOpenTime) return null;

        var endDiff = LastOpenTime - lastOpenTime;

        var modulus = endDiff.Ticks % TimeSpan.Ticks;
        if (modulus != 0)
        {
            throw new ArgumentException($"{nameof(lastOpenTime)} must fall exactly on a TimeFrame.TimeSpan.  lastOpenTime: {lastOpenTime}, TimeSpan: {TimeSpan}, modulus: {modulus}");
            // Alternate idea: allow time values off step with TimeSpan and
            //  - use Math.Floor() to get skipEndBars.
            //  - use Math.Ceiling() to get takeBars.
        }

        var skipEndBars = (int)(endDiff.Ticks / TimeSpan.Ticks);

        var takeBars = (int)((lastOpenTime.UtcTicks - firstOpenTime.Ticks) / TimeSpan.Ticks);

        return values.ToArray().Skip(skipEndBars).Take(takeBars);
    }

    #endregion

    #region Methods

    /// <summary>
    /// If overriding this, either add a bar or throw a different exception.
    /// </summary>
    /// <param name="openTime"></param>
    /// <exception cref="Exception"></exception>
    protected virtual void OnMissingBar(DateTimeOffset openTime)
    {
        throw new Exception($"Expected {NextExpectedOpenTime} but got {openTime}");
    }

    public uint PushFront(IReadOnlyList<T> values)
    {
        uint newBars = 0;
        foreach (var value in values)
        {
            newBars += Push(value, NextExpectedOpenTime, front: true);
        }
        return newBars;
    }
    public uint PushBack(IReadOnlyList<T> values)
    {
        uint newBars = 0;
        foreach (var value in values)
        {
            newBars += Push(value, NextExpectedOpenTime, front: false);
        }
        return newBars;
    }

    public uint PushFront(T value) => Push(value, NextExpectedOpenTime, front: true);
    public uint PushBack(T value) => Push(value, NextExpectedOpenTime, front: false);
    public uint Push(T value, DateTimeOffset openTime, bool front)
    {
        uint newBars = 0;

        if (openTime <= LastOpenTime)
        {
            //Debug.WriteLine("TimeFrameValuesWindow: k.OpenTime <= LastOpenTime");
            return 0;
        }

        while (HasNextExpectedOpenTime && openTime > NextExpectedOpenTime)
        {
            newBars++; // Derived class either throws or adds a bar
            OnMissingBar(openTime);
        }

        newBars++;
        LastOpenTime += TimeSpan;

        if (front) values.PushFront(value);
        else values.PushBack(value);

        TotalValuesSeen += newBars;
        return newBars;
    }

    public T PopBack()
    {
        var value = values.Back();
        return value;
    }
    // ENH: PopFront for completeness, but we currently have no use case for popping with forward mode.

    #endregion

}
