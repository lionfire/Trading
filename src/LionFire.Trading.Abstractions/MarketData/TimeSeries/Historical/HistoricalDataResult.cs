namespace LionFire.Trading.Data;

public interface IHistoricalDataResult
{
    Type Type { get; }
}
public interface IHistoricalDataResult<out TValue> : IHistoricalDataResult
{
    IReadOnlyList<TValue>? Items { get; }
}

public readonly record struct HistoricalDataResult<TValue> : IHistoricalDataResult<TValue>
{
    public Type Type => typeof(TValue);

    public HistoricalDataResult(IReadOnlyList<TValue> items) : this()
    {
        Items = items;
        IsSuccess = items != null;
        FailReason = null;
    }
    public HistoricalDataResult(string failReason) : this()
    {
        Items = null;
        IsSuccess = false;
        FailReason = failReason;
    }
    public static readonly HistoricalDataResult<TValue> Fail = new HistoricalDataResult<TValue>("Unspecified failure");
    public static readonly HistoricalDataResult<TValue> NoData = new HistoricalDataResult<TValue>("No data");

    public bool IsSuccess { get; init; }
    public string? FailReason { get; init; }
    public IReadOnlyList<TValue>? Items { get; init; }
}

