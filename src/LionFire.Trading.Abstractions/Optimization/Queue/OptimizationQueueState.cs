using Orleans;

namespace LionFire.Trading.Optimization.Queue;

/// <summary>
/// State for the OptimizationQueueGrain - persisted to storage
/// </summary>
[GenerateSerializer]
public sealed class OptimizationQueueState
{
    /// <summary>
    /// All jobs in the queue (completed jobs are kept for history)
    /// </summary>
    [Id(0)]
    public Dictionary<Guid, OptimizationQueueItem> Jobs { get; set; } = new();
    
    /// <summary>
    /// Queue of job IDs ordered by priority and creation time
    /// </summary>
    [Id(1)]
    public List<Guid> QueueOrder { get; set; } = new();
    
    /// <summary>
    /// Jobs currently running on each silo
    /// </summary>
    [Id(2)]
    public Dictionary<string, Guid> RunningSiloJobs { get; set; } = new();
    
    /// <summary>
    /// Next job ID counter for unique job numbering
    /// </summary>
    [Id(3)]
    public long NextJobNumber { get; set; } = 1;
    
    /// <summary>
    /// Last time the queue was cleaned up (removing old completed jobs)
    /// </summary>
    [Id(4)]
    public DateTimeOffset LastCleanup { get; set; } = DateTimeOffset.UtcNow;
}