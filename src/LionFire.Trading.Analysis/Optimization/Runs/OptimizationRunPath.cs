using LionFire.Structures;

namespace LionFire.Trading.Automation.Optimization;

public static class OptimizationRunPath
{
    /// <summary>
    /// Date format used in folder names (uses DateTimeFormatting.DateFormat)
    /// </summary>
    public static string DateFormat => DateTimeFormatting.DateFormat;

    public static string GetRelativePath(OptimizationRunId id)
    {
        return Path.Combine(id.Bot, id.Symbol, id.TimeFrame, $"{id.Start}-{id.End}");
    }
}
