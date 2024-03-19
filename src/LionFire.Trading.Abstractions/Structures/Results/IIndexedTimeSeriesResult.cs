namespace LionFire.Trading;

public interface IIndexedTimeSeriesResult<out T> : ITimeSeriesResult, IValuesResult<IReadOnlyTuple<DateTimeOffset, T>>
{
}
