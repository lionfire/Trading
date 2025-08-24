namespace LionFire.Trading.Optimization.Queue;

/// <summary>
/// Status of an optimization job in the queue
/// </summary>
public enum OptimizationJobStatus
{
    /// <summary>
    /// Job is waiting in queue to be executed
    /// </summary>
    Queued = 0,
    
    /// <summary>
    /// Job is currently being executed
    /// </summary>
    Running = 1,
    
    /// <summary>
    /// Job completed successfully
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Job failed during execution
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Job was cancelled by user
    /// </summary>
    Cancelled = 4
}