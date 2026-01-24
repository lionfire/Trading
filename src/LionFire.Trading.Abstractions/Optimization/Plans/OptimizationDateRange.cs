namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Represents a date range for optimization, supporting relative and absolute dates.
/// </summary>
public record OptimizationDateRange
{
    /// <summary>
    /// Display name for the range (e.g., "1mo", "3mo", "YTD").
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Start date expression. Can be:
    /// - Relative: "-1M", "-3M", "-1Y", "-30D"
    /// - Absolute: "2025-01-01"
    /// - Special: "now"
    /// </summary>
    public string Start { get; init; } = "";

    /// <summary>
    /// End date expression. Same format as Start.
    /// </summary>
    public string End { get; init; } = "now";
}
