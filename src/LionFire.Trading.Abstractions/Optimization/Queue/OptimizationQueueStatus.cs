using Orleans;

namespace LionFire.Trading.Optimization.Queue;

/// <summary>
/// Status and statistics for the optimization queue
/// </summary>
[GenerateSerializer]
public sealed class OptimizationQueueStatus
{
    /// <summary>
    /// Number of jobs waiting in queue
    /// </summary>
    [Id(0)]
    public int QueuedCount { get; set; }
    
    /// <summary>
    /// Number of jobs currently running
    /// </summary>
    [Id(1)]
    public int RunningCount { get; set; }
    
    /// <summary>
    /// Number of completed jobs
    /// </summary>
    [Id(2)]
    public int CompletedCount { get; set; }
    
    /// <summary>
    /// Number of failed jobs
    /// </summary>
    [Id(3)]
    public int FailedCount { get; set; }
    
    /// <summary>
    /// Number of cancelled jobs
    /// </summary>
    [Id(4)]
    public int CancelledCount { get; set; }
    
    /// <summary>
    /// Total number of jobs ever submitted
    /// </summary>
    [Id(5)]
    public int TotalJobs { get; set; }
    
    /// <summary>
    /// Number of active silos processing jobs
    /// </summary>
    [Id(6)]
    public int ActiveSilos { get; set; }
    
    /// <summary>
    /// Estimated time until next job starts (based on current progress)
    /// </summary>
    [Id(7)]
    public TimeSpan? EstimatedNextJobDelay { get; set; }
    
    /// <summary>
    /// Average job completion time (for completed jobs)
    /// </summary>
    [Id(8)]
    public TimeSpan? AverageJobDuration { get; set; }
    
    /// <summary>
    /// When this status was generated
    /// </summary>
    [Id(9)]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}