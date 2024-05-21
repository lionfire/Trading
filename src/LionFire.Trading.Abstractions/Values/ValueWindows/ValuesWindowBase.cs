using CircularBuffer;

namespace LionFire.Trading.ValueWindows;


// REVIEW - not sure this class needs to exist: just use CircularBuffer directly
public abstract class ValuesWindowBase<T> : IValuesWindow<T>
{
    #region Parameters

    #region Derived

    public uint Size => (uint)values.Capacity;

    #endregion

    #endregion


    #region Lifecycle

    public ValuesWindowBase(uint period)
    {
        values = new CircularBuffer<T>((int)period);
    }

    #endregion

    #region State

    protected CircularBuffer<T> values;

    /// <summary>
    /// A counter representing the total number of values seen.
    /// For example, if Capacity is 3 and 100 values are pushed through this class, this will return 100.
    /// This counter can be reset using Clear().
    /// </summary>
    public uint TotalValuesSeen { get; protected set; }

    #region Derived

    public uint Capacity => (uint)values.Capacity;
    public bool IsFull => values.IsFull;

    #endregion

    #endregion

    #region Accessors

    /// <summary>
    /// Values with raw access to the buffer
    /// </summary>
    public IList<ArraySegment<T>> ValuesBuffer => values.ToArraySegments();

    public T[] ToArray(uint length)
    {
        var array = new T[length];
        var segments = values.ToArraySegments();

        var count1 = Math.Min(length, segments[0].Count);
        var count2 = Math.Min(length - count1, segments[1].Count);

        Array.Copy(segments[0].Array!, segments[0].Offset, array, 0, count1);
        Array.Copy(segments[1].Array!, segments[1].Offset, array, count1, count2);

        return array;
    }

    #endregion

    #region Methods

    public void Clear()
    {
        values.Clear();
        TotalValuesSeen = 0;
    }

    #endregion

}
