namespace LionFire.Trading;

public class DateAutoOptimizationOptions
{
    /// <summary>
    /// If the interval between the previous DateOptimization and UtcNow is greater than this amount, run a new DateOptimization with the new range
    /// </summary>
    public TimeSpan? RerunWhenOutOfDateBy { get; set; }

    public DateTimeOffset? FixedStartTime { get; set; }

    public TimeSpan? Duration { get; set; }

}
