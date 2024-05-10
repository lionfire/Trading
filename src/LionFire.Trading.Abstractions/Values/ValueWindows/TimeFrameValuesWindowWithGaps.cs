
using System;

namespace LionFire.Trading.ValueWindows;

public sealed class TimeFrameValuesWindowWithGaps<T> : TimeFrameValuesWindow<T>
{
    #region (static)

    static T DefaultDefault => TradingValueUtils<T>.MissingValue;

    #endregion

    #region Parameters

    public T MissingValue { get; init; }

    #endregion

    #region Lifecycle

    public TimeFrameValuesWindowWithGaps(uint period, TimeSpan timeSpan, DateTimeOffset? nextOpenTime = null) : this(period, timeSpan, DefaultDefault, nextOpenTime)
    {
    }
    public TimeFrameValuesWindowWithGaps(uint period, TimeFrame timeFrame, DateTimeOffset? nextOpenTime = null) : this(period, timeFrame, DefaultDefault, nextOpenTime)
    {
    }
    public TimeFrameValuesWindowWithGaps(uint period, TimeSpan timeSpan, T missingValue, DateTimeOffset? nextOpenTime = null) : base(period, timeSpan, nextOpenTime)
    {
        MissingValue = missingValue;
    }
    public TimeFrameValuesWindowWithGaps(uint period, TimeFrame timeFrame, T missingValue, DateTimeOffset? nextOpenTime = null) : base(period, timeFrame, nextOpenTime)
    {
        MissingValue = missingValue;
    }

    #endregion

    protected override void OnMissingBar(DateTimeOffset openTime)
    {
        AddFillerBar();
    }

    private void AddFillerBar()
    {
        OnAddingFillerBar?.Invoke();

        values.PushFront(MissingValue);
        LastOpenTime += TimeSpan;
    }



    public Action? OnAddingFillerBar { get; set; }
}
