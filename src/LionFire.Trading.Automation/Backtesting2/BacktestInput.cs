using CircularBuffer;
using LionFire.Execution;
using LionFire.Threading;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.ValueWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static LionFire.Trading.Automation.BacktestTask2;

namespace LionFire.Trading.Automation;

/// <summary>
/// An enumerator of a series of input OutputBuffer, typically historical data for a Symbol, or Indicator output.
/// 
/// Features:
/// - chunked loading
/// - OutputBuffer buffer
/// </summary>
public abstract class InputEnumeratorBase
{
    #region State

    #region Input

    public abstract int UnprocessedInputCount { get; }

    #endregion

    #endregion

    #region Methods

    #region Input

    /// <summary>
    /// Buffers must be empty
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="start"></param>
    /// <param name="endExclusive"></param>
    /// <returns></returns>
    public Task PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive, uint count)
    {
        if (UnprocessedInputCount > 0) throw new InvalidOperationException("Buffer must be empty before PreloadRange");
        // OPTIMIZE maybe: Allow _PreloadRange before current chunk is done.  Either have dual buffers, or a CircularBuffer of buffers.
        return _PreloadRange(start, endExclusive, count);
    }
    protected virtual Task _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive, uint count) => Task.CompletedTask;

    #endregion

    #region Output

    public abstract void MoveNext();

    #endregion

    #endregion

}

public abstract class AsyncInputEnumerator : InputEnumeratorBase
{
    public abstract Task MoveNextAsync();
    public override void MoveNext() => MoveNextAsync().GetAwaiter().GetResult();
}

// FUTURE: Implement concrete classes for this if needed
public abstract class AsyncInputEnumerator<T> : AsyncInputEnumerator
{
    public AsyncInputEnumerator()
    {
    }
}

public abstract class InputEnumerator<T> : InputEnumeratorBase
{
    #region Dependencies

    public IHistoricalTimeSeries<T> Series { get; }

    #endregion

    #region Characteristics

    //public override bool IsAsync => false;

    #endregion

    #region Lifecycle

    public InputEnumerator(IHistoricalTimeSeries<T> series)
    {
        Series = series;
    }

    #endregion

    #region State

    #region Input

    protected ArraySegment<T> InputBuffer = ArraySegment<T>.Empty;
    protected int InputBufferIndex = 0;

    #region Derived

    public override int UnprocessedInputCount => InputBuffer.Length - InputBufferIndex;

    #endregion

    #endregion

    #endregion

    public T CurrentValue => InputBuffer[InputBufferIndex - 1];

    #region Methods

    #region Input

    protected override async Task _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive, uint _)
    {
        var result = await Series.Get(start, endExclusive);
        if (!result.IsSuccess) { throw new Exception("Failed to get historical data"); }
        InputBuffer = result.Items;
        InputBufferIndex = 0;
    }

    #endregion

    #region Output

    //[MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public override void MoveNext() => InputBufferIndex++;

    //public virtual ValueTask<T> MoveNextAsync() => throw new InvalidOperationException();
    //public void ThrowMissingData() => throw new InvalidOperationException("Unexpected: no more data");

    #endregion

    #endregion

}
public sealed class SingleValueInputEnumerator<T> : InputEnumerator<T>
{
    #region Lifecycle

    public SingleValueInputEnumerator(IHistoricalTimeSeries<T> series) : base(series)
    {
    }

    #endregion


}

public sealed class WindowedInputEnumerator<T> : InputEnumerator<T>
{
    #region Parameters

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

    public override void MoveNext() => OutputBuffer.PushFront(InputBuffer[InputBufferIndex++]);

    #endregion

    #endregion

}

#if FUTURE // Pipelining for expensitve Indicators?
public class IndicatorInputProcessor<T> : InputEnumerator<T>
{
    // OPTIMIZE: HistoricalIndicatorHarness.GetInputData: async loading of input data

    #region Lifecycle

    public IndicatorInputProcessor(IIndicatorHarness<T> harness, bool async = false, uint chunkSize = 0)
    {
        this.Harness = harness;
        IsAsync = async;
        ChunkSize = chunkSize;
    }

    #endregion

    #region State

    TimeFrameValuesWindowWithGaps<T>? values;
    private IIndicatorHarness<T> Harness;
    uint ChunkSize;
    DateTimeOffset NextBar;
    DateTimeOffset EndExclusive;

    #endregion

    protected override Task _PrepareInputChunk(DateTimeOffset start, DateTimeOffset endExclusive, uint count)
    {
        NextBar = start;
        EndExclusive = endExclusive;

        if (values == null || values.ValueCount == 0)
        {
            values = new TimeFrameValuesWindowWithGaps<T>(count, Harness.TimeFrame, start);
        }
        return LoadMore();
    }

    private async Task LoadMore()
    {
        if (values == null) throw new InvalidOperationException();

        var nextEnd = ChunkSize == 0 ? EndExclusive : NextBar + ChunkSize * Harness.TimeFrame.TimeSpan;

        var result = await Harness.GetReverseOutput(NextBar, nextEnd, values).ConfigureAwait(false);
        if (!result.IsSuccess) throw new Exception("Failed to get indicator output: " + result);
        NextBar = nextEnd;
    }

    public override T MoveNext()
    {
        if (values == null || values.ValueCount == 0)
        {
            LoadMore().Wait();
        }
        return values!.PopBack();
    }

    public override async ValueTask<T> MoveNextAsync()
    {
        if (values == null || values.ValueCount == 0)
        {
            await LoadMore().ConfigureAwait(false);
        }
        return values!.PopBack();
    }

    public override bool IsAsync { get; }
}

#endif