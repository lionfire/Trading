using MorseCode.ITask;
using System.Linq;

namespace LionFire.Trading.Data;

public interface IHistoricalTimeSeries
{
    Type ValueType { get; }
    TimeFrame TimeFrame { get; }
}

public interface IHistoricalTimeSeries<TValue> : IHistoricalTimeSeries
{
    // Future Optimizing: if there are chunks, put those chunks in HistoricalDataResult
    ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive);
}

