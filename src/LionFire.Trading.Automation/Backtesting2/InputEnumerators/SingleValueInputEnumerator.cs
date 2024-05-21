using LionFire.Trading.Data;

namespace LionFire.Trading.Automation;

public sealed class SingleValueInputEnumerator<T> : InputEnumeratorBase<T>
{
    #region Lifecycle

    public SingleValueInputEnumerator(IHistoricalTimeSeries<T> series) : base(series)
    {
    }

    #endregion


}

