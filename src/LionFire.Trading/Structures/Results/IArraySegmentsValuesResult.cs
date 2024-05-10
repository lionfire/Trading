namespace LionFire.Trading;

public interface IArraySegmentsValuesResult<T> : IValuesResult<T>
{
    IList<ArraySegment<T>> ArraySegments { get; }
}
