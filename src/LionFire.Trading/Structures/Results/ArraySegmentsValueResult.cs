using CircularBuffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

public interface IArraySegmentsValuesResult<T>
{
    IList<ArraySegment<T>> ArraySegments { get; }
}
public class ArraySegmentsValueResult<T> : IValuesResult<T>, IArraySegmentsValuesResult<T>
{
    public ArraySegmentsValueResult(IList<ArraySegment<T>> arraySegments)
    {
        ArraySegments = arraySegments;
        if(arraySegments.Count != 2) throw new ArgumentException("Only 2 segments supported");
    }

    public IList<ArraySegment<T>> ArraySegments { get; }


    public IEnumerable<T> Values => ArraySegments[0].Concat(ArraySegments[1]);
    //public IEnumerable<T> Values => ArraySegments.SelectMany(x => x);
}

public class ArraySegmentValueResult<T> : IValuesResult<T>, IArraySegmentsValuesResult<T>
{
    public ArraySegmentsValueResult(ArraySegment<T> arraySegment)
    {
        ArraySegments = arraySegments;
        if (arraySegments.Count != 2) throw new ArgumentException("Only 2 segments supported");
    }

    public ArraySegment<T> ArraySegment { get; }


    public IEnumerable<T> Values => ArraySegments[0].Concat(ArraySegments[1]);
    //public IEnumerable<T> Values => ArraySegments.SelectMany(x => x);
}
