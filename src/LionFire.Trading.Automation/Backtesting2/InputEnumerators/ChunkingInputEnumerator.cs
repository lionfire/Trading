using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;
using Serilog.Configuration;
using System.Numerics;

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

/// <summary>
/// For listening to market data with a lookback of greater than zero.
/// The current position may involve looking back past the beginning of the current buffer, so it needs to hold a second ArraySegment.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TPrecision"></typeparam>
public sealed class ChunkingInputEnumerator<TValue, TPrecision> : InputEnumeratorBase<TValue, TPrecision>, IChunkingInputEnumerator
    where TValue : notnull
    where TPrecision : struct, INumber<TPrecision>
{
    // OPTIMIZE idea: see if reversing the array at load time is faster.
    // OPTIMIZE idea: benchmark different chunk sizes (i.e. short vs long chunk, and a portion of those chunks.)

    public ChunkingInputEnumerator(IHistoricalTimeSeries<TValue> series, int lookback) : base(series, lookback)
    {

    }

    protected ArraySegment<TValue> PreviousChunk;
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

    public override TValue this[int index]
    {
        get
        {
            //if (InputBuffer.Array == null) throw new InvalidOperationException("No data"); // OPTIMIZE HOTSPOT - commented this
            var requestedIndex = InputBufferCursorIndex - index;
            if (requestedIndex >= 0 && requestedIndex < InputBuffer.Count)
            {
                return InputBuffer[requestedIndex];
            }
            else
            {
                int previousChunkRequestedIndex = PreviousChunk.Count + /* negative number */ requestedIndex;

                if (PreviousChunk.Array == null || previousChunkRequestedIndex > PreviousChunk.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return PreviousChunk[previousChunkRequestedIndex];
            }
        }
    }

    #region Derived

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
    //public override bool IsFull => InputBuffer.Array != null && PreviousChunk.Array != null; // UNUSED
    public override uint Size => (uint)InputBuffer.Count;

    /// <summary>
    /// Lookback guarantees a certain amount of prior items visible, but often there is more.  This property indicates how many are available.
    /// </summary>
    /// <seealso cref="UnprocessedInputCount"/>
    public int ItemsViewable => InputBufferCursorIndex + Math.Min(1, InputBuffer.Count) + PreviousChunk.Count;

    #endregion

    #endregion

    public override void MoveNext()
    {
        base.MoveNext();

        #region MEMORY OPTIMIZATION - discard unneeded array

        if (InputBufferCursorIndex >= LookbackRequired)
        {
            PreviousChunk = default;
        }

        #endregion
    }
    public override void MoveNext(int count)
    {
        base.MoveNext(count);

        #region MEMORY OPTIMIZATION - discard unneeded array

        if (InputBufferCursorIndex >= LookbackRequired)
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

