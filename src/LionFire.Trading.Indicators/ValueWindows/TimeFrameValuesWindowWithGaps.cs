
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

    public TimeFrameValuesWindowWithGaps(int period, TimeFrame timeFrame) : this(period, timeFrame, DefaultDefault)
    {
    }
    public TimeFrameValuesWindowWithGaps(int period, TimeFrame timeFrame, T missingValue) : base(period, timeFrame)
    {
        MissingValue = missingValue;
    }

    #endregion

    protected override void OnMissingBar(DateTime openTime)
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
