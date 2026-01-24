namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Status of an individual optimization job.
/// </summary>
public enum JobStatus
{
    /// <summary>Job is queued and waiting to be executed.</summary>
    Pending = 0,

    /// <summary>Job is currently being executed.</summary>
    Running = 1,

    /// <summary>Job completed successfully.</summary>
    Completed = 2,

    /// <summary>Job failed with an error.</summary>
    Failed = 3,

    /// <summary>Job was cancelled before completion.</summary>
    Cancelled = 4
}
