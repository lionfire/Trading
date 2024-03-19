namespace LionFire.Trading;

public interface ITimeSeriesResult
{
    DateTimeOffset EndExclusive { get; init; }
    DateTimeOffset Start { get; init; }
    TimeFrame TimeFrame { get; init; }
}

public interface ITimeSeriesResult<out T> : ITimeSeriesResult, IValuesResult<T>
{
    // ENH: IEnumerable<(DateTimeOffset, T)> ValuesWithTimestamps { get; }
}
