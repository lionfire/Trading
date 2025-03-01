using LionFire.Persistence.Handles;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizationRunId : IEquatable<OptimizationRunId>
{
    public string Bot { get; set; }

    public string Symbol { get; set; }

    public string TimeFrame { get; set; }

    public string Start { get; set; }

    public string End { get; set; }


    #region Derived

    [JsonIgnore]
    public string StartAndEnd => $"{Start}-{End}";

    [JsonIgnore]
    public DateOnly? StartDate => DateOnly.TryParseExact(Start, "yyyy.MM.dd", out var r) ? r : null;

    [JsonIgnore]
    public DateOnly? EndDate => DateOnly.TryParseExact(End, "yyyy.MM.dd", out var r) ? r : null;

    [JsonIgnore]
    public double Days
    {
        get
        {
            if (StartDate == null || EndDate == null)
            {
                return -1;
            }
            return (EndDate.Value.ToDateTime(TimeOnly.MinValue) - StartDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays;
        }
    }

    #endregion

    public bool Equals(OptimizationRunId other)
    {
        if (other == null) { return false; }
        if (other.Bot != Bot) { return false; }
        if (other.Symbol != Symbol) { return false; }
        if (other.TimeFrame != TimeFrame) { return false; }
        if (other.Start != Start) { return false; }
        if (other.End != End) { return false; }
        return true;
    }

    public override bool Equals(object obj) => Equals(obj as OptimizationRunId);

    public override int GetHashCode() =>
        Bot.GetHashCode()
        ^ Symbol.GetHashCode()
        ^ TimeFrame.GetHashCode()
        ^ Start.GetHashCode()
        ^ End.GetHashCode();

}

