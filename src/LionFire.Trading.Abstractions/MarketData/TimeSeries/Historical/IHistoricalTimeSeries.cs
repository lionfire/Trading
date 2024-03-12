using System.Linq;

namespace LionFire.Trading.Data;

public interface IHistoricalTimeSeries { }

public interface IHistoricalTimeSeries<TValue> : IHistoricalTimeSeries
{
    ValueTask<HistoricalDataResult<TValue>> TryGetValues(DateTimeOffset start, DateTimeOffset endExclusive);


    // REVIEW OPTIMIZE - use ArraySegments instead?
    async ValueTask<HistoricalDataResult<IEnumerable<TValue>>?> TryGetValueChunks(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var values = await TryGetValues(start, endExclusive);
        if(values.IsSuccess && values.Items != null)
        {
            return new HistoricalDataResult<IEnumerable<TValue>>([values.Items]);
        }
        if (values.FailReason != null) return new(values.FailReason);
        else return HistoricalDataResult<IEnumerable<TValue>>.Fail;
    }
}

