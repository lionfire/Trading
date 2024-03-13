using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

public interface IValuesResult<out T>
{
    IReadOnlyList<T> Values { get; }
}

public interface ITimeSeriesResult<out T> : IValuesResult<T>
{
    DateTimeOffset EndExclusive { get; init; }
    DateTimeOffset Start { get; init; }
    TimeFrame TimeFrame { get; init; }

    // ENH: IEnumerable<(DateTimeOffset, T)> ValuesWithTimestamps { get; }
}