namespace LionFire.Trading.Automation.Optimization;

public static class OptimizationRunPath
{
    //public static string GetRelativePath_Old(OptimizationRunId id)
    //{
    //    return Path.Combine(id.DefaultSymbol, id.Bot, id.DefaultTimeFrame);
    //}
    public static string DateFormat = "yyyy.MM.dd";
    public static string GetRelativePath(OptimizationRunId id)
    {
        return Path.Combine(id.Bot, id.Symbol, id.TimeFrame, $"{id.Start}-{id.End}");
    }
}
