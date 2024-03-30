using CircularBuffer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

public sealed class ArraySegmentsValueResult<T> : IValuesResult<T>, IArraySegmentsValuesResult<T>
{
    #region Parameter (wrapped data)

    private ReadOnlyListWrapperOfTwoArraySegments Wrapper { get; init; }

    #region Derived

    public IList<ArraySegment<T>> ArraySegments => Wrapper.ArraySegments;
    public IReadOnlyList<T> Values => Wrapper;

    #endregion

    #endregion

    public ArraySegmentsValueResult(IList<ArraySegment<T>> arraySegments)
    {
        Wrapper = new ReadOnlyListWrapperOfTwoArraySegments(arraySegments);
    }

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
}
