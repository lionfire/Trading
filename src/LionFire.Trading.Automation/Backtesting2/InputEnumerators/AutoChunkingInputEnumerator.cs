#if UNUSED

using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;

namespace LionFire.Trading.Automation;



// The backtest engine handles chunking, so the automatic part is redundant.
public sealed class AutoChunkingInputEnumerator<T> : ChunkingInputEnumerator<T>
{
    public DateChunker DateChunker { get; }
    protected DateTimeOffset NextChunkStart;

    public AutoChunkingInputEnumerator(IHistoricalTimeSeries<T> series, DateChunker dateChunker, int windowSize) : base(series, windowSize)
    {
        DateChunker = dateChunker;
    }

    public /*override*/ async ValueTask MoveNextAsync()
    {
        if (InputBuffer.Array == null || InputBufferIndex + 1 >= InputBuffer.Count)
        {
            PreviousChunk = InputBuffer;

            // Load next chunk
            var range = DateChunker.LongRangeForDate(NextChunkStart, TimeFrame);
            var result = await Series.Get(range.start, range.endExclusive);
            if (!result.IsSuccess) { result.ThrowFailReason(); }
            InputBuffer = result.Items;
            InputBufferIndex = 0;
        }
        else
        {
            InputBufferIndex++;

            #region MEMORY OPTIMIZATION - discard unneeded array

            if (InputBufferIndex >= LookbackRequired)
            {
                PreviousChunk = default;
            }

            #endregion
        }
    }
}

#endif