using System.Linq;

namespace LionFire.Trading.Data;

public interface IHistoricalDataResult
{
    Type Type { get; }
}

// REVIEW - This is slightly more specialized and designed for performance than IValuesResult, which is designed for general use
public interface IHistoricalDataResult<TValue> 
    : IHistoricalDataResult
    , IValuesResult<TValue>
{
    new ArraySegment<TValue> Values { get; }
}

public readonly record struct HistoricalDataResult<TValue> 
    : IHistoricalDataResult<TValue>
{
    #region (static)

    public static readonly HistoricalDataResult<TValue> Fail = new HistoricalDataResult<TValue>("Unspecified failure");
    public static readonly HistoricalDataResult<TValue> NoData = new HistoricalDataResult<TValue>("No data");

    #endregion


    public Type Type => typeof(TValue);

    #region Lifecycle

    public HistoricalDataResult(IEnumerable<TValue> items) : this(items.ToArray()) { }
    public HistoricalDataResult(IReadOnlyList<TValue> items) : this(items.ToArray()) { }
    public HistoricalDataResult(TValue[]? items) : this((items == null ? default : (ArraySegment<TValue>)items), items != null)
    {
    }
    public HistoricalDataResult(ArraySegment<TValue> items, bool? isSuccess = null)
    {
        Values = items;
        IsSuccess = isSuccess ?? items.Count > 0;
        FailReason = null;
    }
    public HistoricalDataResult(string failReason) : this()
    {
        Values = [];
        IsSuccess = false;
        FailReason = failReason;
    }
    #endregion

    #region Properties

    public bool IsSuccess { get; init; }
    public string? FailReason { get; init; }

    #endregion

    #region Collection

    public readonly ArraySegment<TValue> Values { get; init; }
    IReadOnlyList<TValue>? IValuesResult<TValue>.Values => Values;

    #endregion

    #region Methods

    public readonly void ThrowFailReason()
    {
        if (IsSuccess) { throw new InvalidOperationException(); }
        throw new Exception(FailReason ?? "Unspecified failure");
    }
    
    #endregion
}

