using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;
using Serilog.Configuration;

namespace LionFire.Trading.Automation;

public interface IChunkingInputEnumerator : IInputEnumerator
{
#if UNUSED
    int WindowSize { get; }
#endif

    /// <summary>
    /// Input enumerators are shared by consumers who require different lookback amounts.  Invoke this to ensure that the lookback is grown to accommodate any particular consumer. 
    /// </summary>
    /// <param name="minimumLookback"></param>
    void GrowLookback(int minimumLookback);

}

public class ChunkingInputEnumerator<T> : InputEnumeratorBase<T>, IChunkingInputEnumerator
{
    // OPTIMIZE idea: see if reversing the array at load time is faster.
    // OPTIMIZE idea: benchmark different chunk sizes (i.e. short vs long chunk, and a portion of those chunks.)

    public ChunkingInputEnumerator(IHistoricalTimeSeries<T> series, int lookback) : base(series, lookback)
    {

    }

    protected ArraySegment<T> PreviousChunk;
    protected override bool HasPreviousChunk => PreviousChunk.Count > 0;

    public TimeFrame TimeFrame => Series.TimeFrame;

#if UNUSED
    public int WindowSize
    {
        get => LookbackRequired + 1;
        set => LookbackRequired = value - 1;
    }
#endif
    public void GrowLookback(int minimumLookback)
    {
        if (LookbackRequired < minimumLookback)
        {
            LookbackRequired = minimumLookback;
        }
    }

    #region IReadOnlyValuesWindow<T>

    public override uint Capacity
    {
        get
        {
            uint count = 0;
            if (InputBuffer.Array != null) { count += (uint)InputBuffer.Count; }
            if (PreviousChunk.Array != null) { count += (uint)PreviousChunk.Count; }
            return count;
        }
    }

    public override bool IsFull => InputBuffer.Array != null && PreviousChunk.Array != null;
    public override uint Size => Capacity - (uint)InputBufferIndex;

    public override T this[int index]
    {
        get
        {
            //if (InputBuffer.Array == null) throw new InvalidOperationException("No data"); // OPTIMIZE HOTSPOT - commented this
            var inputBufferIndex = InputBufferIndex - index;
            if (inputBufferIndex >= 0 && inputBufferIndex < InputBuffer.Count)
            {
                return InputBuffer[inputBufferIndex];
            }
            else
            {
                //int previousChunkActualIndex = PreviousChunk.Count - 1 - index - InputBufferIndex - 1;
                int previousChunkActualIndex = PreviousChunk.Count + inputBufferIndex;

                if (PreviousChunk.Array == null || previousChunkActualIndex > PreviousChunk.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return PreviousChunk[previousChunkActualIndex];
            }
        }
    }

    #endregion

    public override void MoveNext()
    {
        base.MoveNext();

        #region MEMORY OPTIMIZATION - discard unneeded array

        if (InputBufferIndex >= LookbackRequired)
        {
            PreviousChunk = default;
        }

        #endregion
    }
    public override void MoveNext(int count)
    {
        base.MoveNext(count);

        #region MEMORY OPTIMIZATION - discard unneeded array

        if (InputBufferIndex >= LookbackRequired)
        {
            PreviousChunk = default;
        }

        #endregion
    }

    protected override ValueTask _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        PreviousChunk = InputBuffer;
        return base._PreloadRange(start, endExclusive);
    }
}

