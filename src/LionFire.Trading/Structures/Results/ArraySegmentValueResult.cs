using System.Linq;

namespace LionFire.Trading;

public class ArraySegmentValueResult<T> : IValuesResult<T>, IArraySegmentsValuesResult<T>
{
    #region Parameter (wrapped data)

    public ArraySegment<T> ArraySegment { get; }

    #region Derived

    public IReadOnlyList<T> Values => ArraySegment;
    public IList<ArraySegment<T>> ArraySegments => [ArraySegment];

    #endregion

    #endregion

    public ArraySegmentValueResult(ArraySegment<T> arraySegment)
    {
        ArraySegment = arraySegment;
    }

    //readonly struct ReadOnlyListWrapperOfArraySegment : IReadOnlyList<T>
    //{
    //    public ArraySegment<T> ArraySegment { get; }

    //    public ReadOnlyListWrapperOfArraySegment(ArraySegment<T> arraySegment)
    //    {
    //        ArraySegment = arraySegment;
    //    }

    //    public T this[int index] => ArraySegment[index];

    //    public int Count => ArraySegment.Count;

    //    public IEnumerator<T> GetEnumerator() => ArraySegment.GetEnumerator();

    //    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    //}

}
