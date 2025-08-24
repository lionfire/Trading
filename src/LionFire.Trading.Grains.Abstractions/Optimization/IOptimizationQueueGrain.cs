using LionFire.Trading.Optimization;
using LionFire.Trading.Optimization.Queue;

namespace LionFire.Trading.Grains.Optimization;

/// <summary>
/// Orleans grain interface for managing the global optimization job queue
/// </summary>
public interface IOptimizationQueueGrain : IGrainWithStringKey
{
    /// <summary>
    /// Add a new optimization job to the queue
    /// </summary>
    /// <param name="parametersJson">Serialized PMultiSim parameters</param>
    /// <param name="priority">Job priority (lower = higher priority)</param>
    /// <param name="submittedBy">User or system submitting the job</param>
    /// <returns>The created queue item</returns>
    Task<OptimizationQueueItem> EnqueueJobAsync(string parametersJson, int priority = 5, string? submittedBy = null);
    
    /// <summary>
    /// Get the next available job for a silo to execute
    /// </summary>
    /// <param name="siloId">ID of the requesting silo</param>
    /// <param name="maxConcurrentJobs">Maximum jobs this silo can handle concurrently</param>
    /// <returns>Next job to execute, or null if none available</returns>
    Task<OptimizationQueueItem?> DequeueJobAsync(string siloId, int maxConcurrentJobs = 1);
    
    /// <summary>
    /// Update the progress of a running job
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <param name="progress">Current optimization progress</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateJobProgressAsync(Guid jobId, OptimizationProgress progress);
    
    /// <summary>
    /// Mark a job as completed
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <param name="resultPath">Path to the optimization results</param>
    /// <returns>True if update was successful</returns>
    Task<bool> CompleteJobAsync(Guid jobId, string? resultPath = null);
    
    /// <summary>
    /// Mark a job as failed
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <returns>True if update was successful</returns>
    Task<bool> FailJobAsync(Guid jobId, string errorMessage);
    
    /// <summary>
    /// Cancel a job (can be queued or running)
    /// </summary>
    /// <param name="jobId">ID of the job to cancel</param>
    /// <returns>True if job was cancelled</returns>
    Task<bool> CancelJobAsync(Guid jobId);
    
    /// <summary>
    /// Get details of a specific job
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <returns>Job details or null if not found</returns>
    Task<OptimizationQueueItem?> GetJobAsync(Guid jobId);
    
    /// <summary>
    /// Get all jobs with optional filtering
    /// </summary>
    /// <param name="status">Filter by status (null for all)</param>
    /// <param name="limit">Maximum number of jobs to return</param>
    /// <returns>List of jobs matching criteria</returns>
    Task<IReadOnlyList<OptimizationQueueItem>> GetJobsAsync(OptimizationJobStatus? status = null, int limit = 100);
    
    /// <summary>
    /// Get queue statistics and summary
    /// </summary>
    /// <returns>Queue status information</returns>
    Task<OptimizationQueueStatus> GetQueueStatusAsync();
    
    /// <summary>
    /// Report heartbeat from a silo executing a job
    /// </summary>
    /// <param name="jobId">ID of the job being executed</param>
    /// <param name="siloId">ID of the executing silo</param>
    /// <returns>True if heartbeat was recorded</returns>
    Task<bool> HeartbeatAsync(Guid jobId, string siloId);
    
    /// <summary>
    /// Clean up old completed jobs and handle stale running jobs
    /// </summary>
    /// <param name="completedJobRetentionDays">Days to keep completed jobs</param>
    /// <param name="jobTimeoutMinutes">Minutes before considering a running job stale</param>
    /// <returns>Number of jobs cleaned up</returns>
    Task<int> CleanupAsync(int completedJobRetentionDays = 7, int jobTimeoutMinutes = 30);
}