using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;
using Serilog.Configuration;

namespace LionFire.Trading.Automation;

public interface IChunkingInputEnumerator
{
    int WindowSize { get; }
    int LookbackRequired { get; }
    void GrowLookback(int minimumLookback);
}

public class ChunkingInputEnumerator<T> : InputEnumeratorBase<T>
{
    // OPTIMIZE idea: see if reversing the array at load time is faster.
    // OPTIMIZE idea: benchmark different chunk sizes (i.e. short vs long chunk, and a portion of those chunks.)

    public ChunkingInputEnumerator(IHistoricalTimeSeries<T> series, int lookback) : base(series)
    {
        LookbackRequired = lookback;
    }

    protected ArraySegment<T> PreviousChunk;

    public TimeFrame TimeFrame => Series.TimeFrame;
    public int LookbackRequired { get; private set; }
    public int WindowSize
    {
        get => LookbackRequired + 1;
        set => LookbackRequired = value - 1;
    }
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
            if (InputBuffer.Array == null) throw new InvalidOperationException("No data");
            if (InputBufferIndex - index >= 0)
            {
                return InputBuffer[InputBufferIndex - index];
            }
            else
            {
                int previousChunkActualIndex = PreviousChunk.Count - 1 - index - InputBufferIndex - 1;

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

