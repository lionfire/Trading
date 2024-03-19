using MorseCode.ITask;
using System.Linq;

namespace LionFire.Trading.Data;

public interface IHistoricalTimeSeries
{
    //ITask<IHistoricalDataResult<IEnumerable<object>>> TryGetValueChunks(DateTimeOffset start, DateTimeOffset endExclusive);
}

public interface IHistoricalTimeSeries<TValue> : IHistoricalTimeSeries
{
    ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive);


    // OLD - if there are chunks, put those chunks in HistoricalDataResult
    // REVIEW OPTIMIZE - use ArraySegments instead?
    //async new ValueTask<HistoricalDataResult<IEnumerable<TValue>>?> TryGetValueChunks(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var values = await TryGetValues(start, endExclusive);
    //    if (values.IsSuccess && values.Items != null)
    //    {
    //        return new HistoricalDataResult<IEnumerable<TValue>>([values.Items]);
    //    }
    //    if (values.FailReason != null) return new(values.FailReason);
    //    else return HistoricalDataResult<IEnumerable<TValue>>.Fail;
    //}
}

