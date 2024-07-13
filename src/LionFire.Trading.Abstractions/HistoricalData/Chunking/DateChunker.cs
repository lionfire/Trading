
using LionFire.Base;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LionFire.Trading.HistoricalData;

public class DateChunker
{
    public IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive), bool isLong)> GetBarChunks(IRangeWithTimeFrame range)
        => GetBarChunks(range.Start, range.EndExclusive, range.TimeFrame);
    public IEnumerable<((DateTimeOffset start, DateTimeOffset endExclusive), bool isLong)> GetBarChunks(DateTimeOffset start, DateTimeOffset endExclusive, TimeFrame timeFrame, bool shortOnly = false)
    {
        DateTimeOffset cursor = start;

        do
        {
            var range = RangeForDate(cursor, timeFrame);
            if (shortOnly && range.isLong)
            {
                foreach (var shortened in ShortRangesForLongRange(cursor, timeFrame))
                {
                    yield return ((shortened.start, shortened.endExclusive), false);
                }
            }
            yield return range;
            cursor = range.Item1.endExclusive;
        } while (cursor < endExclusive);
    }

    public bool IsLongRangeForDate(DateTimeOffset date, TimeFrame timeFrame)
    {
        var now = DateTimeOffset.UtcNow;

        if (timeFrame.TimeFrameUnit == TimeFrameUnit.Minute)
        {
            return date < new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        }
        if (timeFrame.TimeFrameUnit == TimeFrameUnit.Hour)
        {
            return date < new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }

        switch (timeFrame.Name)
        {
            default:
                throw new NotImplementedException();
        }
    }

    public ((DateTimeOffset start, DateTimeOffset endExclusive), bool isLong) RangeForDate(DateTimeOffset date, TimeFrame timeFrame)
        => IsLongRangeForDate(date, timeFrame) ? (LongRangeForDate(date, timeFrame), true) : (ShortRangeForDate(date, timeFrame), false);

    public IEnumerable<(DateTimeOffset start, DateTimeOffset endExclusive)> ShortRangesForLongRange(DateTimeOffset date, TimeFrame timeFrame)
    {
        var longRange = LongRangeForDate(date, timeFrame);
        var shortStart = longRange.start;
        while (shortStart < longRange.endExclusive)
        {
            var shortRange = ShortRangeForDate(shortStart, timeFrame);
            yield return shortRange;
            shortStart = shortRange.endExclusive;
        }
    }

    public (DateTimeOffset start, DateTimeOffset endExclusive) ShortRangeForDate(DateTimeOffset date, TimeFrame timeFrame)
    {
        switch (timeFrame.Name)
        {
            case "m1":
                return DateRangeUtils.GetDays(date, 1);
            case "h1":
                return DateRangeUtils.GetMonths(date);
            //return (new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTimeOffset(date.Year, date.Month, DateTimeOffset.DaysInMonth(date.Year, date.Month), 23, 0, 0, DateTimeKind.Utc));
            default:
                throw new NotImplementedException();
        }
    }

    public (DateTimeOffset start, DateTimeOffset endExclusive) MaxCountForLongRange(TimeFrame timeFrame)
    {
        // Choose a leap year
        // Choose longest month
        // OPTIMIZE: This could be replaced with constants
        return LongRangeForDate(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), timeFrame);
    }

    public (DateTimeOffset start, DateTimeOffset endExclusive) LongRangeForDate(DateTimeOffset date, TimeFrame timeFrame)
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

    public void ValidateIsChunkBoundary(DateTimeOffset start, DateTimeOffset endExclusive, TimeFrame timeFrame)
    {
        var chunks = GetBarChunks(start, endExclusive, timeFrame);
        if (chunks.Count() != 1 || chunks.First().Item1.start != start || chunks.First().Item1.endExclusive != endExclusive)
        {
            throw new ArgumentException($"{nameof(start)} and {nameof(endExclusive)} must be on a chunk boundary for {timeFrame.Name}");
        }
    }
    public bool IsValidShortRange(TimeFrame timeFrame, DateTimeOffset from, DateTimeOffset to)
    {
        var r = ShortRangeForDate(from, timeFrame);
        return r.start == from && r.endExclusive == to;
    }
    public bool IsValidLongRange(TimeFrame timeFrame, DateTimeOffset from, DateTimeOffset to)
    {
        var r = LongRangeForDate(from, timeFrame);
        return r.start == from && r.endExclusive == to;
    }
}
