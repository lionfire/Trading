﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TS = System.TimeSpan;
using LionFire.Serialization;
using System.Text.Json.Serialization;


#if cAlgo
//using LFTimeFrame = LionFire.Trading.LionFireTimeFrame;
using cAlgo.API;
#endif
using System.Threading.Tasks;


namespace LionFire.Trading;

public static class TimeFrameTimeSpanX
{
    // TODO: Use the inverse of these as ApproximateTimeSpan
    public static readonly TimeSpan y1 = -TimeSpan.FromDays(365);
    public static readonly TimeSpan y2 = -TimeSpan.FromDays(365 * 2);
    public static readonly TimeSpan y3 = -TimeSpan.FromDays(365 * 3);

    public static readonly TimeSpan mn1 = -TimeSpan.FromDays(30);
    public static readonly TimeSpan mn2 = -TimeSpan.FromDays(30 * 2);

    // 1 tick = 100 nanoseconds
    //   = 0.1 microseconds
    //   = 0.0001 milliseconds
    //   = 0.0000001 seconds
    public static readonly long EstimatedTickNanoseconds = 100 * 1000; // Deliberately low so we have room for things like t1000 without bumping into s1.
    public static readonly TimeSpan t1 = -TimeSpan.FromMilliseconds(EstimatedTickNanoseconds);
    public static readonly TimeSpan t2 = -TimeSpan.FromMilliseconds(EstimatedTickNanoseconds * 2);
    public static readonly TimeSpan t3 = -TimeSpan.FromMilliseconds(EstimatedTickNanoseconds * 3);

    public static TimeFrame ToTimeFrame(this TimeSpan timeSpan)
    {
        if (timeSpan > TimeSpan.Zero)
        {
            if (timeSpan == TimeSpan.FromMinutes(1)) return TimeFrame.m1;
            if (timeSpan == TimeSpan.FromMinutes(5)) return TimeFrame.m5;
            if (timeSpan == TimeSpan.FromHours(1)) return TimeFrame.h1;
            if (timeSpan == TimeSpan.FromHours(4)) return TimeFrame.h4;
            // TODO: more
        }
        else
        {
            // Special ones: y, mn, t
        }

        throw new NotImplementedException();

    }
    public static bool IsValidTimeFrame(this TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.MaxValue) return false;
        if (timeSpan == TimeSpan.MinValue) return false;
        if (timeSpan <= TimeSpan.Zero) return false;
        if (timeSpan == default) return false;
        return true;
    }
}

#if !cAlgo

#if cAlgo
public class LionFireTimeFrame
#else
public class TimeFrame : ISerializableAsString
    , IParsableSlim<TimeFrame> // TODO - NEW
#endif
{

    #region Misc

    public override bool Equals(object o)
    {
        var other = o as TimeFrame;
        return other?.Name == Name;
    }
    public override int GetHashCode() => Name.GetHashCode();

    #endregion

    #region Static

#if cAlgo
    static LionFireTimeFrame()
#else
    static TimeFrame()
#endif
    {
        t1 = new TimeFrame("t1");
        m1 = new TimeFrame("m1");
        m2 = new TimeFrame("m2");
        m3 = new TimeFrame("m3");
        m4 = new TimeFrame("m4");
        m5 = new TimeFrame("m5");
        m6 = new TimeFrame("m6");
        m7 = new TimeFrame("m7");
        m8 = new TimeFrame("m8");
        m9 = new TimeFrame("m9");
        m10 = new TimeFrame("m10");
        m15 = new TimeFrame("m15");
        m20 = new TimeFrame("m20");
        m30 = new TimeFrame("m30");
        m45 = new TimeFrame("m45");

        h1 = new TimeFrame("h1");
        h2 = new TimeFrame("h2");
        h3 = new TimeFrame("h3");
        h4 = new TimeFrame("h4");
        h6 = new TimeFrame("h6");
        h8 = new TimeFrame("h8");
        h12 = new TimeFrame("h12");

        d1 = new TimeFrame("d1");
        d2 = new TimeFrame("d2");
        d3 = new TimeFrame("d3");

        w1 = new TimeFrame("w1");
        mn1 = new TimeFrame("mn1");
    }

    public long? GetExpectedBarCount(DateTimeOffset? start, DateTimeOffset? endExclusive)
        => (TimeSpan <= TimeSpan.Zero || !start.HasValue || !endExclusive.HasValue)
        ? null
        : (long?)((endExclusive - start) / TimeSpan);
    public long? GetExpectedBarCountForNow(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var now = DateTimeOffset.UtcNow;
        return GetExpectedBarCount(start, endExclusive > now ? GetPeriodStart(now) + TimeSpan : endExclusive);
    }
    public DateTimeOffset GetExpectedBarOpenTimeForLastClosedBar()
    {
        var now = DateTimeOffset.UtcNow;
        return AddBars(GetOpenTimeBefore(now), -1);
    }
    public DateTimeOffset GetExpectedBarOpenTimeForNow()
    {
        var now = DateTimeOffset.UtcNow;
        return GetOpenTimeBefore(now);
    }

    public static TimeFrame t1 { get; private set; }
    public static TimeFrame m1 { get; private set; }
    public static TimeFrame OneMinute => m1;

    public static TimeFrame m2 { get; private set; }
    public static TimeFrame m3 { get; private set; }
    public static TimeFrame m4 { get; private set; }
    public static TimeFrame m5 { get; private set; }
    public static TimeFrame FiveMinutes => m5;
    public static TimeFrame m6 { get; private set; }
    public static TimeFrame m7 { get; private set; }
    public static TimeFrame m8 { get; private set; }
    public static TimeFrame m9 { get; private set; }
    public static TimeFrame m10 { get; private set; }
    public static TimeFrame m15 { get; private set; }
    public static TimeFrame m20 { get; private set; }
    public static TimeFrame m30 { get; private set; }
    public static TimeFrame m45 { get; private set; }
    public static TimeFrame h1 { get; private set; }
    public static TimeFrame OneHour => h1;
    public static TimeFrame h2 { get; private set; }
    public static TimeFrame h3 { get; private set; }
    public static TimeFrame h4 { get; private set; }
    public static TimeFrame h6 { get; private set; }
    public static TimeFrame h8 { get; private set; }
    public static TimeFrame h12 { get; private set; }
    public static TimeFrame d1 { get; private set; }
    public static TimeFrame Daily => d1;
    public static TimeFrame d2 { get; private set; }
    public static TimeFrame d3 { get; private set; }
    public static TimeFrame w1 { get; private set; }
    public static TimeFrame mn1 { get; private set; }
    public static TimeFrame y1 { get; private set; }
    public static TimeFrame y2 { get; private set; }
    public static TimeFrame y3 { get; private set; }

    #endregion

    #region Construction

#if cAlgo
    public LionFireTimeFrame(string name)
#else
    public TimeFrame(string name)
#endif
    {
        this.Name = name;
        string type = name[0].ToString();

        if (name.Substring(0, 2) == "mn") type = "mn";

        switch (type)
        {
            case "t":
                TimeFrameUnit = TimeFrameUnit.Tick;
                break;
            case "s":
                TimeFrameUnit = TimeFrameUnit.Second;
                break;
            case "m":
                TimeFrameUnit = TimeFrameUnit.Minute;
                break;
            case "h":
                TimeFrameUnit = TimeFrameUnit.Hour;
                break;
            case "d":
                TimeFrameUnit = TimeFrameUnit.Day;
                break;
            case "w":
                TimeFrameUnit = TimeFrameUnit.Week;
                break;
            case "M":
            case "mn":
                TimeFrameUnit = TimeFrameUnit.Month;
                break;
            default:
                break;
        }

        TimeFrameValue = Int32.Parse(name.Substring(type.Length));

        TimeSpan = ParseTimeSpan(TimeFrameUnit, TimeFrameValue);
    }

    public static TimeFrame Parse(string timeFrameCode)
        => TryParse(timeFrameCode) ?? throw new ArgumentException($"Failed to parse TimeFrame from '{timeFrameCode}'");

    public static TimeFrame TryParse(string timeFrameCode)
    {
        if (string.IsNullOrWhiteSpace(timeFrameCode)) return null;

        var pi = typeof(TimeFrame).GetProperty(timeFrameCode, BindingFlags.Public | BindingFlags.Static);
        return pi?.GetValue(null) as TimeFrame;
    }

    public static implicit operator TimeFrame?(string? timeFrameCode)
    {
        return timeFrameCode == null ? null : TryParse(timeFrameCode);
    }

    public DateTimeOffset GetOpenTimeBefore(DateTimeOffset date)
    {
        switch (TimeFrameUnit)
        {
            case TimeFrameUnit.Tick:
                throw new NotSupportedException();
            case TimeFrameUnit.Second:
                if (60 % TimeFrameValue != 0) { throw new NotImplementedException(); }
                if (date.Millisecond != 0) { return new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, TS.Zero); }
                else
                {
                    return date - System.TimeSpan.FromSeconds(1);
                }
            case TimeFrameUnit.Minute:
                if (60 % TimeFrameValue != 0) { throw new NotImplementedException(); }
                if (date.Second != 0 || date.Millisecond != 0) { return new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, TS.Zero); }
                else
                {
                    return date - System.TimeSpan.FromMinutes(1);
                }
            case TimeFrameUnit.Hour:
                if (24 % TimeFrameValue != 0) { throw new NotImplementedException(); }
                if (date.Minute != 0 || date.Second != 0 || date.Millisecond != 0) { return new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, 0, 0, TS.Zero); }
                else
                {
                    return date - System.TimeSpan.FromHours(1);
                }

            //case TimeFrameUnit.Day:
            //    break;
            //case TimeFrameUnit.Week:
            //    break;
            //case TimeFrameUnit.Month:
            //    break;
            //case TimeFrameUnit.Year:
            //    break;
            case TimeFrameUnit.Unspecified:
            default:
                throw new NotImplementedException();
        }
    }

    #endregion



    #region Operators

    public static implicit operator string?(TimeFrame? tf) => tf?.Name;

    #endregion

    #region Properties

    /// <summary>
    /// Format: unit + value, e.g. m1, h1, d1, mn1.  This is reverse of normal display order to avoid the problem with variable names cannot start with a number.
    /// </summary>
    public string Name { get; set; }

    public TimeFrameUnit TimeFrameUnit { get; set; }

    public int TimeFrameValue { get; set; }

    #endregion

    #region Derived

    /// <summary>
    /// If irregular, returns MaxValue
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TimeSpan ParseTimeSpan(TimeFrameUnit unit, int value)
    {
        TimeSpan timeSpan;
        switch (unit)
        {
            case TimeFrameUnit.Tick:
                timeSpan = TimeSpan.MaxValue;
                break;
            case TimeFrameUnit.Minute:
                timeSpan = System.TimeSpan.FromMinutes(value);
                break;
            case TimeFrameUnit.Hour:
                timeSpan = System.TimeSpan.FromHours(value);
                break;
            case TimeFrameUnit.Day:
                timeSpan = System.TimeSpan.FromDays(value);
                break;
            case TimeFrameUnit.Week:
                timeSpan = System.TimeSpan.FromDays(value * 7);
                break;
            case TimeFrameUnit.Month:
                timeSpan = TimeSpan.MaxValue; // Irregular due to different month lengths
                break;
            case TimeFrameUnit.Year:
                timeSpan = TimeSpan.MaxValue; // Irregular due to leap days
                break;
            default:
                throw new ArgumentNullException(nameof(unit));
        }
        return timeSpan;
    }

    [JsonIgnore]
    public TimeSpan ApproximateTimeSpan => TimeSpan > TimeSpan.Zero ? TimeSpan : -TimeSpan;

    /// <summary>
    /// RENAME: Period
    /// </summary>
    public TimeSpan TimeSpan { get; init; }
    //{ // TODO - Init this readonly during construction
    //    get
    //    {
    //        if (timeSpan.HasValue)
    //        {
    //            switch (TimeFrameUnit)
    //            {
    //                case TimeFrameUnit.Tick:
    //                    timeSpan = null;
    //                    break;
    //                case TimeFrameUnit.Minute:
    //                    timeSpan = System.TimeSpan.FromMinutes(TimeFrameValue);
    //                    break;
    //                case TimeFrameUnit.Hour:
    //                    timeSpan = System.TimeSpan.FromHours(TimeFrameValue);
    //                    break;
    //                case TimeFrameUnit.Day:
    //                    timeSpan = System.TimeSpan.FromDays(TimeFrameValue);
    //                    break;
    //                case TimeFrameUnit.Week:
    //                    timeSpan = System.TimeSpan.FromDays(TimeFrameValue * 7);
    //                    break;
    //                case TimeFrameUnit.Month:
    //                    timeSpan = null;
    //                    break;
    //                default:
    //                    throw new ArgumentNullException("TimeFrameUnit");
    //            }
    //        }
    //        return timeSpan;
    //    }
    //}
    //private TimeSpan? timeSpan;

    public bool HasFixedTimeSpan
    {
        get
        {
            switch (TimeFrameUnit)
            {
                case TimeFrameUnit.Tick:
                    return false;
                case TimeFrameUnit.Minute:
                case TimeFrameUnit.Hour:
                case TimeFrameUnit.Day:
                case TimeFrameUnit.Week:
                case TimeFrameUnit.Month:
                    return true;
                default:
                    throw new ArgumentNullException("TimeFrameUnit");
            }
        }
    }

    public static TimeSpan TickApproximationTime = System.TimeSpan.FromMilliseconds(200);
    public TimeSpan TimeSpanApproximation
    {
        get
        {
            TimeSpan result;
            switch (TimeFrameUnit)
            {
                case TimeFrameUnit.Tick:
                    result = TickApproximationTime;
                    break;
                case TimeFrameUnit.Minute:
                    result = System.TimeSpan.FromMinutes(TimeFrameValue);
                    break;
                case TimeFrameUnit.Hour:
                    result = System.TimeSpan.FromHours(TimeFrameValue);
                    break;
                case TimeFrameUnit.Day:
                    result = System.TimeSpan.FromDays(TimeFrameValue);
                    break;
                case TimeFrameUnit.Week:
                    result = System.TimeSpan.FromDays(TimeFrameValue * 7);
                    break;
                case TimeFrameUnit.Month:
                    result = System.TimeSpan.FromDays(30.436875);
                    break;
                default:
                    throw new ArgumentNullException("TimeFrameUnit");
            }
            return result;
        }
    }

    #endregion

    #region Utilities

    public DateTimeOffset GetPeriodStart(DateTimeOffset time)
    {
        switch (TimeFrameUnit)
        {
            //case TimeFrameUnit.Unspecified:
            //    break;
            //case TimeFrameUnit.Tick:
            //    break;
            case TimeFrameUnit.Second:
                if ((60 / this.TimeFrameValue) % 1.0 == 0.0)
                {
                    var second = (time.Minute / this.TimeFrameValue) * TimeFrameValue;
                    return new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, second, TS.Zero);
                }
                else
                {
                    throw new NotImplementedException("GetPeriodStart for " + Name);
                }
            case TimeFrameUnit.Minute:
                if ((60 / this.TimeFrameValue) % 1.0 == 0.0)
                {
                    var minute = (time.Minute / this.TimeFrameValue) * TimeFrameValue;
                    return new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, minute, 0, TS.Zero);
                }
                else
                {
                    throw new NotImplementedException("GetPeriodStart for " + Name);
                }
            case TimeFrameUnit.Hour:
                if ((24 / this.TimeFrameValue) % 1.0 == 0.0)
                {
                    var hour = (time.Hour / this.TimeFrameValue) * TimeFrameValue;
                    return new DateTimeOffset(time.Year, time.Month, time.Day, hour, 0, 0, TS.Zero);
                }
                else
                {
                    throw new NotImplementedException("GetPeriodStart for " + Name);
                }
            //case TimeFrameUnit.Day:

            //    break;
            //case TimeFrameUnit.Week:
            //    break;
            //case TimeFrameUnit.Month:
            //    break;
            //case TimeFrameUnit.Year:
            //    break;
            default:
                throw new NotImplementedException("GetPeriodStart for " + Name);
        }

    }

    #endregion

    #region Misc

    public override string ToString() => Name;


    public DateTime Round(DateTime value)
    {
        if (this.TimeFrameValue != 1 && TimeFrameUnit != TimeFrameUnit.Tick)
        {
            Console.WriteLine($"WARN: Round not fully implemented for {this}");
        }

        switch (this.TimeFrameUnit)
        {
            case TimeFrameUnit.Tick:
                return value;
            case TimeFrameUnit.Second:
                return value.RoundMillisecondsToZero();
            case TimeFrameUnit.Minute:
                return value.RoundSecondsToZero();
            case TimeFrameUnit.Hour:
                return value.RoundMinutesToZero();
            case TimeFrameUnit.Day:
                return value.RoundHoursToZero();
            case TimeFrameUnit.Week:
                throw new NotImplementedException();
            case TimeFrameUnit.Month:
                return value.RoundDaysToOne();
            case TimeFrameUnit.Year:
                return value.RoundMonthsToOne();
            default:
                throw new NotImplementedException();
        }
    }

    // FUTURE: Get factors of a timeframe
    public TimeFrame MoreGranular(bool factorOfTwo = true)
    {
        var val = TimeFrameValue - 1;

        for (TimeFrameUnit unit = TimeFrameUnit; unit != TimeFrameUnit.Unspecified; unit = unit.MoreGranular())
        {
            for (; val >= 1;)
            {
                var tf = TimeFrame.TryGet(unit, val);
                if (tf != null) return tf;
                if (factorOfTwo)
                {
                    val /= 2;
                }
                else
                {
                    val--;
                }
            }
            val = Math.Max(1, val);
        }
        return null;
    }

    public DateTime GetInclusiveCloseTimeForOpen(DateTime openTime, TimeSpan? epsilon = null)
    {
        epsilon ??= System.TimeSpan.FromMilliseconds(1); // REVIEW: should this be 0, and exclusive?
        return GetNextOpenTimeForOpen(openTime) - epsilon.Value;
    }

    public DateTime GetNextOpenTimeForOpen(DateTime openTime)
    {
        //if (TimeFrameUnit == TimeFrameUnit.Month)
        //{
        //    throw new NotImplementedException("month");
        //}
        if (TimeSpan <= TimeSpan.Zero) throw new NotImplementedException();
        return openTime + TimeSpan;
    }

    public static TimeFrame TryGet(TimeFrameUnit unit, int val)
    {
        return TryParse(unit.ToLetterCode() + val.ToString());
    }

    public DateTimeOffset NextBarOpen(DateTimeOffset time)
    {
        switch (this.ToShortString())
        {

            case "h1":
                return new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, 0, 0, time.Offset) + System.TimeSpan.FromHours(1);
            case "m1":
                return new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Offset) + System.TimeSpan.FromMinutes(1);
            default:
                throw new NotImplementedException();
        }
    }
    public TimeSpan TimeUntilBarClose(DateTimeOffset time) => NextBarOpen(time) - time;

    public static TimeFrame FromTimeSpan(TimeSpan timeSpan)
    {
        timeSpan += new TimeSpan(0, 0, 0, 0, 1);

        if (timeSpan - TS.FromMinutes(1) < TS.FromSeconds(1)) return TimeFrame.m1;
        if (timeSpan - TS.FromHours(1) < TS.FromSeconds(1)) return TimeFrame.h1;

        throw new NotImplementedException();
    }

    public long ToExactBarCount(TS timeSpan)
    {
        throw new NotImplementedException();
    }

    public DateTimeOffset AddBar(DateTimeOffset barTime)
    {
        if (TimeSpan < TimeSpan.Zero) throw new NotImplementedException("Irregular TimeFrames");
        return barTime + TimeSpan;
    }
    public DateTimeOffset AddBars(DateTimeOffset barTime, long count)
    {
        if (TimeSpan < TimeSpan.Zero) throw new NotImplementedException("Irregular TimeFrames");
        return barTime + (TimeSpan * count);
    }


    public string? Serialize() => this.ToString();
    public static object? Deserialize(string? serializedString) => serializedString == null ? null : new TimeFrame(serializedString);
    static TimeFrame IParsableSlim<TimeFrame>.Parse(string s) => (TimeFrame)TimeFrame.Deserialize(s)!;

    ///// <summary>
    ///// </summary>
    ///// <param name="openTime">Assumed to be a valid Open Time for this TimeFrame (otherwise behavior is undefined)</param>
    ///// <returns></returns>
    ///// <exception cref="NotImplementedException"></exception>
    //public DateTimeOffset GetNextOpenTime(DateTimeOffset openTime)
    //{
    //    if (TimeSpan < TimeSpan.Zero) throw new NotImplementedException("Irregular TimeFrames");
    //    return openTime + TimeSpan;
    //}

    #endregion
}
#endif

#if FUTURE // ?
//public static class TimeFrameBarX
//{
//// Add/subtract
//}

//public readonly struct TimeFrameBar
//{
//    public TimeFrameBar(TimeFrame timeFrame, DateTimeOffset openTime)
//    {
//        TimeFrame = timeFrame;
//        OpenTime = openTime;
//    }

//    public TimeFrame TimeFrame { get; }
//    public DateTimeOffset OpenTime { get; }
//}
#endif

public static class TimeFrameExtensions
{
    public static int GetBarsFromMinutes(this TimeFrame timeFrame, double minutes)
    {
        return (int)timeFrame._GetBarsFromMinutes(minutes);
    }
    private static double _GetBarsFromMinutes(this TimeFrame timeFrame, double minutes)
    {
#if !cAlgo
#else
#endif
        switch (timeFrame.ToShortString())
        {
            case "m":
            case "m1":
                return minutes;
            case "m2":
                return minutes / 2.0;
            case "m3":
                return minutes / 3.0;
            case "m4":
                return minutes / 4.0;
            case "m5":
                return minutes / 5.0;
            case "m6":
                return minutes / 6.0;
            case "m7":
                return minutes / 7.0;
            case "m8":
                return minutes / 8.0;
            case "m9":
                return minutes / 9.0;
            case "m10":
                return minutes / 10.0;
            case "m15":
                return minutes / 15.0;
            case "m20":
                return minutes / 20.0;
            case "m30":
                return minutes / 30.0;
            case "m45":
                return minutes / 45.0;
            case "h1":
            case "h":
                return minutes / 60.0;
            case "h2":
                return minutes / (60.0 * 2);
            case "h4":
                return minutes / (60.0 * 4);
            case "h6":
                return minutes / (60.0 * 6);
            case "h8":
                return minutes / (60.0 * 8);
            case "h10":
                return minutes / (60.0 * 10);
            case "h12":
                return minutes / (60.0 * 12);
            case "h16":
                return minutes / (60.0 * 16);
            case "h18":
                return minutes / (60.0 * 18);
            case "d":
            case "d1":
                return minutes / (60.0 * 24);
            case "d2":
                return minutes / (60.0 * 24 * 2);
            case "d3":
                return minutes / (60.0 * 24 * 3);
            case "w":
            case "w1":
                return minutes / (60.0 * 24 * 7);
        }
        throw new NotImplementedException(timeFrame.ToString());
    }



    public static string ToShortString(this TimeFrame tf)
    {
        var str = tf.ToString();
#if cAlgo
        str = str.Replace("Day", "d");
        str = str.Replace("Hour", "h");
        str = str.Replace("Minute", "m");
        str = str.Replace("Tick", "t");
        if (str.Length == 1) { str += "1"; }
#endif
        return str;

    }


}
