
using System.Linq;

namespace LionFire.Trading.HistoricalData.Serialization;

public class HistoricalDataChunkRangeProvider
{
    public IEnumerable<(DateTime, DateTime)> GetBarChunks(DateTime start, DateTime endExclusive, TimeFrame timeFrame)
    {
        DateTime cursor = start;

        do
        {
            var range = RangeForDate(cursor, timeFrame);
            yield return range;
            cursor = range.endExclusive;
        } while (cursor < endExclusive);
    }

    public bool IsLongRangeForDate(DateTime date, TimeFrame timeFrame)
    {
        var now = DateTime.UtcNow;
        switch (timeFrame.Name)
        {
            case "m1":
                return date < new DateTime(now.Year, now.Month, 1);
            case "h1":
                return date < new DateTime(now.Year, 1, 1);
            default:
                throw new NotImplementedException();
        }
    }

    public (DateTime start, DateTime endExclusive) RangeForDate(DateTime date, TimeFrame timeFrame) 
        => IsLongRangeForDate(date, timeFrame) ? LongRangeForDate(date, timeFrame) : ShortRangeForDate(date, timeFrame);

    public (DateTime start, DateTime endExclusive) ShortRangeForDate(DateTime date, TimeFrame timeFrame)
    {
        switch (timeFrame.Name)
        {
            case "m1":
                return DateRangeUtils.GetDays(date, 1);
            case "h1":
                return DateRangeUtils.GetMonths(date);
                //return (new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 0, 0, DateTimeKind.Utc));
            default:
                throw new NotImplementedException();
        }
    }

    public (DateTime start, DateTime endExclusive) LongRangeForDate(DateTime date, TimeFrame timeFrame)
    {
        switch (timeFrame.Name)
        {
            case "m1":
                return DateRangeUtils.GetMonths(date);
            case "h1":
                return DateRangeUtils.GetYear(date);
            default:
                throw new NotImplementedException();
        }
    }

    public void ValidateIsChunkBoundary(DateTime start, DateTime endExclusive, TimeFrame timeFrame)
    {
        var chunks = GetBarChunks(start, endExclusive, timeFrame);
        if(chunks.Count() != 1 || chunks.First().Item1 != start || chunks.First().Item2 != endExclusive)
        {
            throw new ArgumentException($"{nameof(start)} and {nameof(endExclusive)} must be on a chunk boundary for {timeFrame.Name}");
        }
    }
}
