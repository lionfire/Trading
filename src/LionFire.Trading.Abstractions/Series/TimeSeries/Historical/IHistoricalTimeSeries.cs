using MorseCode.ITask;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Data;

public interface IHistoricalTimeSeries
{
    Type ValueType { get; }
    Type PrecisionType => ValueType.IsPrimitive
        ? ValueType 
        : (ValueType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPrecision<>))
                .FirstOrDefault() ?? throw new ArgumentException($"{nameof(ValueType)} '{ValueType.FullName}' must implement {typeof(IPrecision<>).Name}<TValue>")).GetGenericArguments()[0];

    TimeFrame TimeFrame { get; }
}

public interface IHistoricalTimeSeries<TValue> : IHistoricalTimeSeries
{
    // Future Optimizing: if there are chunks, put those chunks in HistoricalDataResult
    ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive);
}

