namespace LionFire.Trading.Automation.Optimization;

public static class OptimizationRunPath
{
    //public static string GetRelativePath_Old(OptimizationRunId id)
    //{
    //    return Path.Combine(id.Symbol, id.Bot, id.TimeFrame);
    //}
    public static string DateFormat = "yyyy.MM.dd";
    public static string GetRelativePath(OptimizationRunId id)
    {
        return Path.Combine(id.Bot, id.Symbol, id.TimeFrame, $"{id.Start}-{id.End}");
    }
}
