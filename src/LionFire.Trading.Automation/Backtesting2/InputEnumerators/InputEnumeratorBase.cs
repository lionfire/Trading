using LionFire.Collections.Concurrent;
using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;
using System.Numerics;
using System.Reactive.Disposables;

namespace LionFire.Trading.Automation;

public interface IInputEnumerator
{
    int LookbackRequired { get; }
}

/// <summary>
/// An enumerator of a series of input OutputBuffer, typically historical data for a DefaultSymbol, or Indicator output.
/// 
/// Features:
/// - chunked loading
/// - OutputBuffer buffer
/// </summary>
public abstract class InputEnumeratorBase : IInputEnumerator
{
    #region Identity

    //public abstract Type ValueType { get; }

    #endregion

    #region Parameters

    public int LookbackRequired { get; protected set; }

    #endregion

    #region State

    #region Input

    public abstract int UnprocessedInputCount { get; }

    #endregion

    #endregion

    public abstract IReadOnlyValuesWindow Values { get; }

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
    public ValueTask PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        if (HasPreviousChunk && UnprocessedInputCount > 0) throw new InvalidOperationException("Buffer must be empty before PreloadRange");
        //if (!HasPreviousChunk && LookbackRequired > 0 && UnprocessedInputCount > LookbackRequired) throw new InvalidOperationException("Buffer must be smaller than LookbackRequired on 2nd chunk load, before PreloadRange");

        // OPTIMIZE maybe: Allow _PreloadRange before current chunk is done.  Either have dual buffers, or a CircularBuffer of buffers.
        return _PreloadRange(start, endExclusive);
    }
    protected virtual ValueTask _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive) => ValueTask.CompletedTask;

    protected virtual bool HasPreviousChunk => false;

    #endregion

    #region Output

    public abstract void MoveNext();
    public abstract void MoveNext(int count);
    //public abstract ValueTask MoveNextAsync();

    #endregion

    #endregion

}

public abstract class InputEnumeratorBase<TValue, TPrecision> : InputEnumeratorBase, IReadOnlyValuesWindow<TValue, TPrecision>
    where TValue : notnull
    where TPrecision : struct, INumber<TPrecision>
{
    #region Dependencies

    public IHistoricalTimeSeries<TValue> Series { get; }

    #endregion

    public override IReadOnlyValuesWindow Values => this;

    #region Characteristics

    //public override bool IsAsync => false;

    #endregion

    #region Lifecycle

    public InputEnumeratorBase(IHistoricalTimeSeries<TValue> series, int lookback)
    {
        Series = series;
        LookbackRequired = lookback;
    }

    #endregion

    #region State

    #region Input

    protected ArraySegment<TValue> InputBuffer = ArraySegment<TValue>.Empty;
    protected int InputBufferCursorIndex = -1;

    #region Derived

    public override int UnprocessedInputCount => Math.Max(0, InputBuffer.Count - InputBufferCursorIndex - 1);

    #endregion

    #endregion

    #endregion

    public TValue CurrentValue => InputBuffer[InputBufferCursorIndex];
    public bool HasCurrentValue => InputBufferCursorIndex >= 0 && InputBuffer.Count > 0;
    public TPrecision? CurrentPrice => HasCurrentValue ? GetLastPrice(CurrentValue) : default;
    public TPrecision GetLastPrice(TValue value)
    {
        if (typeof(TValue) == typeof(TPrecision)) return (TPrecision)(object)value!;

        if (value is IClosePrice<TPrecision> closePrice) return closePrice.Close;

        throw new NotSupportedException();
    }

    #region IReadOnlyValuesWindow<T>

    public abstract uint Capacity { get; }

    //public abstract bool IsFull { get; } // UNUSED, not sure how it would be useful or how exactly it works
    public abstract uint Size { get; }

    public abstract TValue this[int index] { get; }

    #endregion

    #region Methods

    #region Input

    protected override async ValueTask _PreloadRange(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var result = await Series.Get(start, endExclusive);
        if (!result.IsSuccess) { throw new Exception($"Failed to get historical data from {start} to {endExclusive} for {Series}"); }
        InputBuffer = result.Values;
        InputBufferCursorIndex = -1; // MoveNext will bump it to 0
    }

    #endregion

    #region Output

    //[MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public override void MoveNext()
    {
        bool checkPriceTriggers = (up != null || down != null);//&& InputBufferIndex >= 0;

        var hasCurrentValue = HasCurrentValue;

        if (checkPriceTriggers)
        {
            PreviousValue = hasCurrentValue ? CurrentValue : default;
        }
        InputBufferCursorIndex++;

        if (checkPriceTriggers)
        {
            var newHighLow = CurrentValue as IHighLowPrice<TPrecision>;
            var newOpen = CurrentValue as IOpenPrice<TPrecision>;
            var newClose = CurrentValue as IClosePrice<TPrecision>;

            TPrecision high;
            // REFACTOR OPTIMIZE
            {
                if (newHighLow != null) high = newHighLow.High;
                else if (newClose != null)
                {
                    if (newOpen != null) high = newOpen.Open > newClose.Close ? newOpen.Open : newClose.Close;
                    else high = newClose.Close;
                }
                else if (CurrentValue is TPrecision number)
                {
                    high = number;
                }
                else { throw new NotSupportedException(); }
            }

            TPrecision low;
            // REFACTOR OPTIMIZE
            {
                if (newHighLow != null) low = newHighLow.Low;
                else if (newClose != null)
                {
                    if (newOpen != null) low = newOpen.Open < newClose.Close ? newOpen.Open : newClose.Close;
                    else low = newClose.Close;
                }
                else if (CurrentValue is TPrecision number)
                {
                    low = number;
                }
                else { throw new NotSupportedException(); }
            }

            if (up != null)
            {
                while (true)
                {
                    var first = up.First();
                    if (first.Key > high) break;
                    first.Value(CurrentValue);
                    up.RemoveAt(0);
                    if (up.Count == 0)
                    {
                        up = null;
                        break;
                    }
                }
            }

            if (down != null) // DUPLICATE of up logic
            {
                while (true)
                {
                    var first = down.First();
                    if (first.Key < low) break;
                    first.Value(CurrentValue);
                    down.RemoveAt(0);
                    if (down.Count == 0)
                    {
                        down = null;
                        break;
                    }
                }
            }

            //var intersection = Intersector<TValue>.Get<TValue>(PreviousValue, CurrentValue);
            //if(intersection.HasValue)

        }
    }
    public override void MoveNext(int count) => InputBufferCursorIndex += count;

    //public override ValueTask MoveNextAsync() { InputBufferIndex++; return ValueTask.CompletedTask; }
    //public void ThrowMissingData() => throw new InvalidOperationException("Unexpected: no more data");

    #endregion

    #endregion

    #region Events: Price Triggers

#if OLD // Disposable subscriptions

    //private class ValueTriggerSubscription : IDisposable
    //{
    //    public void Dispose()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    //ConcurrentHashSet<ValueTriggerSubscription> subscriptions = new();
#endif

    #region State

    SortedList<TPrecision, Action<TValue>>? up;
    SortedList<TPrecision, Action<TValue>>? down;
    TValue? PreviousValue;

    /// <summary>
    /// Hackish way to alllow for duplicate keys in a SortedList which doesn't normally allow duplicates.
    /// Warning: this can be dangerous in certain cases (i.e. infinite loops).
    /// Assumptions:
    /// - keys are never hull
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    private class PriceTriggersDuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        internal static readonly PriceTriggersDuplicateKeyComparer<TKey> Default = new();
        public int Compare(TKey? x, TKey? y)
        {
            int result = x!.CompareTo(y);
            return result == 0
                ? 1 // Equal? Pretend it's greater.  Breaks Remove(key) and IndexOfKey(key)
                : result;
        }
    }

    #endregion

    public void SubscribeToPrice(TPrecision triggerValue, Action<TValue> onReached, PriceSubscriptionDirection direction = PriceSubscriptionDirection.UpOrDown)
    {

        SortedList<TPrecision, Action<TValue>> getSubscriptionList(bool isUp)
        {
            if (isUp) return up ??= new SortedList<TPrecision, Action<TValue>>(PriceTriggersDuplicateKeyComparer<TPrecision>.Default);
            else return down ??= new SortedList<TPrecision, Action<TValue>>(PriceTriggersDuplicateKeyComparer<TPrecision>.Default);
        }

        var hasCurrentValue = HasCurrentValue;
        var currentPrice = CurrentPrice;

        switch (direction)
        {
            case PriceSubscriptionDirection.Up:
                if (hasCurrentValue && triggerValue <= currentPrice!)
                {
                    onReached(CurrentValue);
                    return;
                }
                getSubscriptionList(isUp: true).Add(triggerValue, onReached);
                return;
            case PriceSubscriptionDirection.Down:
                if (hasCurrentValue && triggerValue >= currentPrice!)
                //if (currentPrice is not null && triggerValue <= currentPrice)
                {
                    onReached(CurrentValue);
                    return;
                }
                getSubscriptionList(false).Add(triggerValue, onReached);
                return;
            case PriceSubscriptionDirection.UpOrDown:
                if (hasCurrentValue)
                {
                    if (triggerValue == currentPrice)
                    {
                        onReached(CurrentValue);
                    }
                    else
                    {
                        getSubscriptionList(triggerValue > currentPrice!).Add(triggerValue, onReached);
                    }
                    return;
                }
                else // currentPrice is not available
                {
                    // Add to both lists, so at least one will be triggered when the first value is available
                    getSubscriptionList(true).Add(triggerValue, onReached);
                    getSubscriptionList(false).Add(triggerValue, onReached);
                    return;
                }
            //case PriceSubscriptionDirection.Unspecified:
            default:
                throw new ArgumentException();
        }
    }
    #endregion
}

//public static class Intersector<T>
//{
//    public static (TPrecision oldValue, TPrecision newValue, T value)? Get<T>(T previous, T current)
//    {
//        throw new NotImplementedException();
//    }
//}

