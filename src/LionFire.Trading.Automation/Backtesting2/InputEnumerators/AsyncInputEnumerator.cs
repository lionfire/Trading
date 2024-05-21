#if FUTURE // maybe, for generating expensive indicator output on demand instead of in chunks, in case backtests are frequently aborted
namespace LionFire.Trading.Automation;

public abstract class AsyncInputEnumerator : InputEnumeratorBase
{
    public override void MoveNext() => MoveNextAsync().GetAwaiter().GetResult();
}

// FUTURE: Implement concrete classes for this if needed
public abstract class AsyncInputEnumerator<T> : AsyncInputEnumerator
{
    public AsyncInputEnumerator()
    {
    }
}


#if OLD // Pipelining for expensitve Indicators?
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

#endif