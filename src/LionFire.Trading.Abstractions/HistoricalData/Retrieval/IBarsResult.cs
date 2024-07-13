namespace LionFire.Trading.HistoricalData.Retrieval;

// TODO CLEANUP and REVIEW
public interface IBarsResult<TValue> : ITimeSeriesResult<TValue>
{
    //    string Name { get; }
    //    HistoricalDataSourceKind2 SourceType { get; }

    IReadOnlyList<TValue>? Bars => Values;
    Type NativeType { get; }

    bool IsUpToDate { get; }

    IBarsResult<TValue> Trim(DateTimeOffset start, DateTimeOffset endExclusive);
}

public interface IBarsResult : IBarsResult<IKline> { }

