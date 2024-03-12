namespace LionFire.Trading.Data;

public readonly record struct HistoricalDataResult<TValue>
{
    public HistoricalDataResult(IEnumerable<TValue> items) : this()
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
    public IEnumerable<TValue>? Items { get; init; }
}

