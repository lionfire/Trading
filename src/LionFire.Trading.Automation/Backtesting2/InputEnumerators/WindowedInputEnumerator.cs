using CircularBuffer;
using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;
#if UNUSED // ChunkingInputEnumerator is more efficient

public interface IWindowedInputEnumerator
{
    int Capacity { get; }
    void GrowCapacityIfNeeded(int minimumCapacity, bool discardContentsOnGrow = false);
}


public sealed class WindowedInputEnumerator<T> : InputEnumeratorBase<T>, IWindowedInputEnumerator, IReadOnlyValuesWindow<T>
{
    #region Parameters

    public int Capacity => OutputBuffer.Capacity;

    uint IReadOnlyValuesWindow.Capacity => throw new NotImplementedException();

    public bool IsFull => throw new NotImplementedException();

    public uint Size => throw new NotImplementedException();

    public T this[int index] => throw new NotImplementedException();

    public void GrowCapacityIfNeeded(int minimumCapacity, bool discardContentsOnGrow = false)
    {
        if (!discardContentsOnGrow) throw new NotImplementedException();
        if (Capacity < minimumCapacity)
        {
            if (discardContentsOnGrow)
            {
                OutputBuffer = new(minimumCapacity);
            }
            else
            {
                OutputBuffer = new(minimumCapacity, OutputBuffer.ToArray());
            }
        }
    }

    #endregion

    #region Lifecycle

    public WindowedInputEnumerator(IHistoricalTimeSeries<T> series, int memory) : base(series)
    {
        OutputBuffer = new CircularBuffer<T>(memory);
    }

    #endregion

    #region State

    private CircularBuffer<T> OutputBuffer;

    #endregion

    #region Methods

    #region Consumer

    public override ValueTask MoveNextAsync()
    {
        OutputBuffer.PushFront(InputBuffer[InputBufferIndex++]);
        return ValueTask.CompletedTask;
    }

    #endregion

    #endregion

}

#endif
