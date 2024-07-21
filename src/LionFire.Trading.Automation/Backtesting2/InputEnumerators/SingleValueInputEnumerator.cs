using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;

public sealed class SingleValueInputEnumerator<T> : InputEnumeratorBase<T>
{
    #region Lifecycle

    public SingleValueInputEnumerator(IHistoricalTimeSeries<T> series) : base(series, 0)
    {
    }

    #endregion

    #region IReadOnlyValuesWindow<T>

    public override T this[int index] => index == 0 ? InputBuffer[InputBufferIndex] : throw new ArgumentOutOfRangeException();

    public override uint Capacity => 1;

    public override bool IsFull => true;

    public override uint Size => 1;

    #endregion

}

