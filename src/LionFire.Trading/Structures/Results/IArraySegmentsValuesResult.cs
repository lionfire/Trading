namespace LionFire.Trading;

public interface IArraySegmentsValuesResult<T>
{
    IList<ArraySegment<T>> ArraySegments { get; }
}
