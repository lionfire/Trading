using LionFire.Trading.Data;

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
    public ValueTask PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive )
    {
        if (UnprocessedInputCount > 0) throw new InvalidOperationException("Buffer must be empty before PreloadRange");
        // OPTIMIZE maybe: Allow _PreloadRange before current chunk is done.  Either have dual buffers, or a CircularBuffer of buffers.
        return _PreloadRange(start, endExclusive);
    }
    protected virtual ValueTask _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive) => ValueTask.CompletedTask;

    #endregion

    #region Output

    public abstract void MoveNext();
    public abstract void MoveNext(int count);
    //public abstract ValueTask MoveNextAsync();

    #endregion

    #endregion

}



public abstract class InputEnumeratorBase<T> : InputEnumeratorBase
{
    #region Dependencies

    public IHistoricalTimeSeries<T> Series { get; }

    #endregion

    #region Characteristics

    //public override bool IsAsync => false;

    #endregion

    #region Lifecycle

    public InputEnumeratorBase(IHistoricalTimeSeries<T> series)
    {
        Series = series;
    }

    #endregion

    #region State

    #region Input

    protected ArraySegment<T> InputBuffer = ArraySegment<T>.Empty;
    protected int InputBufferIndex = 0;

    #region Derived

    public override int UnprocessedInputCount => InputBuffer.Count - InputBufferIndex;

    #endregion

    #endregion

    #endregion

    public T CurrentValue => InputBuffer[InputBufferIndex - 1];

    #region Methods

    #region Input

    protected override async ValueTask _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive)
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
    public override void MoveNext(int count) => InputBufferIndex += count;

    //public override ValueTask MoveNextAsync() { InputBufferIndex++; return ValueTask.CompletedTask; }
    //public void ThrowMissingData() => throw new InvalidOperationException("Unexpected: no more data");

    #endregion

    #endregion

}

