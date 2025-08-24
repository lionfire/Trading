using LionFire.Trading.Optimization;
using Orleans;

namespace LionFire.Trading.Optimization.Queue;

/// <summary>
/// Represents an optimization job in the queue
/// </summary>
[GenerateSerializer]
public sealed class OptimizationQueueItem
{
    /// <summary>
    /// Unique identifier for this job
    /// </summary>
    [Id(0)]
    public Guid JobId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Priority of the job (lower numbers = higher priority)
    /// </summary>
    [Id(1)]
    public int Priority { get; set; } = 5; // Default normal priority
    
    /// <summary>
    /// Current status of the job
    /// </summary>
    [Id(2)]
    public OptimizationJobStatus Status { get; set; } = OptimizationJobStatus.Queued;
    
    /// <summary>
    /// Serialized optimization parameters
    /// </summary>
    [Id(3)]
    public string ParametersJson { get; set; } = string.Empty;
    
    /// <summary>
    /// When the job was created
    /// </summary>
    [Id(4)]
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// When the job started executing (null if not started)
    /// </summary>
    [Id(5)]
    public DateTimeOffset? StartedTime { get; set; }
    
    /// <summary>
    /// When the job completed (null if not completed)
    /// </summary>
    [Id(6)]
    public DateTimeOffset? CompletedTime { get; set; }
    
    /// <summary>
    /// ID of the silo assigned to execute this job
    /// </summary>
    [Id(7)]
    public string? AssignedSiloId { get; set; }
    
    /// <summary>
    /// Current progress of the optimization (if running)
    /// </summary>
    [Id(8)]
    public OptimizationProgress? Progress { get; set; }
    
    /// <summary>
    /// Path to optimization results (if completed)
    /// </summary>
    [Id(9)]
    public string? ResultPath { get; set; }
    
    /// <summary>
    /// Error message if job failed
    /// </summary>
    [Id(10)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// User or system that submitted the job
    /// </summary>
    [Id(11)]
    public string? SubmittedBy { get; set; }
    
    /// <summary>
    /// Last time this job was updated (heartbeat from executing silo)
    /// </summary>
    [Id(12)]
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Number of retry attempts for this job
    /// </summary>
    [Id(13)]
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Maximum number of retries allowed
    /// </summary>
    [Id(14)]
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Get estimated completion time based on current progress
    /// </summary>
    public DateTimeOffset? EstimatedCompletionTime
    {
        get
        {
            if (Status != OptimizationJobStatus.Running || Progress == null || StartedTime == null)
                return null;
                
            if (Progress.Percent <= 0)
                return null;
                
            var elapsed = DateTimeOffset.UtcNow - StartedTime.Value;
            var estimatedTotal = TimeSpan.FromTicks((long)(elapsed.Ticks / (Progress.Percent / 100.0)));
            return StartedTime.Value + estimatedTotal;
        }
    }
    
    /// <summary>
    /// Get duration if job is completed
    /// </summary>
    public TimeSpan? Duration
    {
        get
        {
            if (StartedTime == null) return null;
            var endTime = CompletedTime ?? DateTimeOffset.UtcNow;
            return endTime - StartedTime.Value;
        }
    }
}