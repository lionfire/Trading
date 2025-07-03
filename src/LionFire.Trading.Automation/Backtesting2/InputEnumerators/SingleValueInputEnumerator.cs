using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;
using System.Numerics;

namespace LionFire.Trading.Automation;

public sealed class SingleValueInputEnumerator<TValue, TPrecision> : InputEnumeratorBase<TValue, TPrecision>
    where TValue : notnull
    where TPrecision : struct, INumber<TPrecision>
{
    #region Lifecycle

    public SingleValueInputEnumerator(IHistoricalTimeSeries<TValue> series) : base(series, 0)
    {
    }

    #endregion

    #region IReadOnlyValuesWindow<T>

    public override TValue this[int index] => index == 0 ? InputBuffer[InputBufferCursorIndex] : throw new ArgumentOutOfRangeException();

    public override uint Capacity => 1;

    //public override bool IsFull => true; // UNUSED

    public override uint Size => 1;

    #endregion

}

