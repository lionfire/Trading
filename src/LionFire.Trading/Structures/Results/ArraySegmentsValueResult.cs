using System.Collections;

namespace LionFire.Trading;

public readonly struct ArraySegmentsValueResult<T>(IList<ArraySegment<T>> arraySegments)
: IValuesResult<T>
, IArraySegmentsValuesResult<T>
{
    #region Parameter (wrapped data)

    private ReadOnlyListWrapperOfTwoArraySegments Wrapper { get; init; } = new ReadOnlyListWrapperOfTwoArraySegments(arraySegments);

    #region Derived

    public readonly IList<ArraySegment<T>> ArraySegments => Wrapper.ArraySegments;
    public readonly IReadOnlyList<T> Values => Wrapper;

    #endregion

    #endregion

    #region (inner) Wrapper Type

    readonly struct ReadOnlyListWrapperOfTwoArraySegments : IReadOnlyList<T>
    {
        public ReadOnlyListWrapperOfTwoArraySegments(IList<ArraySegment<T>> arraySegments)
        {
            ArraySegments = arraySegments;
            if (ArraySegments.Count != 2) throw new ArgumentException("Only 2 segments supported");
        }

        public T this[int index]
        {
            get
            {
                if (index < ArraySegments[0].Count)
                {
                    return ArraySegments[0].Array![ArraySegments[0].Offset + index];
                }
                else
                {
                    return ArraySegments[1].Array![ArraySegments[1].Offset + index - ArraySegments[0].Count];
                }
            }
        }

        public int Count => ArraySegments[0].Count + ArraySegments[1].Count;

        public IList<ArraySegment<T>> ArraySegments { get; }

        public IEnumerator<T> GetEnumerator() => ArraySegments[0].Concat(ArraySegments[1]).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    #endregion
}
