using System.Linq;

namespace LionFire.Trading.Data;

public interface IHistoricalDataResult
{
    Type Type { get; }
}
public interface IHistoricalDataResult<TValue> : IHistoricalDataResult
{
    // TODO: Reconcile with IValuesResult

    ArraySegment<TValue> Items { get; }
}

public readonly record struct HistoricalDataResult<TValue> : IHistoricalDataResult<TValue>
{
    public Type Type => typeof(TValue);

    public HistoricalDataResult(IEnumerable<TValue> items) : this(items.ToArray()) { }
    public HistoricalDataResult(IReadOnlyList<TValue> items) : this(items.ToArray()) { }
    public HistoricalDataResult(TValue[]? items) : this((items == null ? default : (ArraySegment<TValue>)items), items != null)
    {
    }

    public HistoricalDataResult(ArraySegment<TValue> items, bool? isSuccess = null)
    {
        Items = items;
        IsSuccess = isSuccess ?? items.Count > 0;
        FailReason = null;
    }
    public HistoricalDataResult(string failReason) : this()
    {
        Items = [];
        IsSuccess = false;
        FailReason = failReason;
    }
    public static readonly HistoricalDataResult<TValue> Fail = new HistoricalDataResult<TValue>("Unspecified failure");
    public static readonly HistoricalDataResult<TValue> NoData = new HistoricalDataResult<TValue>("No data");

    public bool IsSuccess { get; init; }
    public string? FailReason { get; init; }
    public ArraySegment<TValue> Items { get; init; }

    public void ThrowFailReason()
    {
        if (IsSuccess) { throw new InvalidOperationException(); }
        throw new Exception(FailReason ?? "Unspecified failure");
    }
}

