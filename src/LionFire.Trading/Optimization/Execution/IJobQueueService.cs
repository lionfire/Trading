using System.Threading;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Service for managing a queue of optimization jobs.
/// </summary>
public interface IJobQueueService
{
    /// <summary>
    /// Enqueue a single job.
    /// </summary>
    Task EnqueueAsync(OptimizationJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueue multiple jobs efficiently.
    /// </summary>
    Task EnqueueBatchAsync(IEnumerable<OptimizationJob> jobs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeue the next pending job and mark it as running.
    /// </summary>
    /// <param name="planId">Optional filter by plan ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next job, or null if no pending jobs.</returns>
    Task<OptimizationJob?> DequeueNextAsync(string? planId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeue a specific job by its ID and mark it as running.
    /// </summary>
    /// <param name="jobId">The ID of the job to dequeue.</param>
    /// <param name="planId">Optional filter by plan ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job, or null if not found or not pending.</returns>
    Task<OptimizationJob?> DequeueByIdAsync(string jobId, string? planId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a job by its ID.
    /// </summary>
    Task<OptimizationJob?> GetByIdAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all jobs with a specific status.
    /// </summary>
    Task<IReadOnlyList<OptimizationJob>> GetByStatusAsync(JobStatus status, string? planId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all jobs for a plan.
    /// </summary>
    Task<IReadOnlyList<OptimizationJob>> GetByPlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a job's status and optionally its result.
    /// </summary>
    Task UpdateAsync(OptimizationJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get counts of jobs by status.
    /// </summary>
    Task<JobStatusCounts> GetCountsAsync(string? planId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all jobs for a plan.
    /// </summary>
    Task ClearAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event fired when a job's status changes.
    /// </summary>
    event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
}

/// <summary>
/// Counts of jobs by status.
/// </summary>
public record JobStatusCounts
{
    public int Pending { get; init; }
    public int Running { get; init; }
    public int Completed { get; init; }
    public int Failed { get; init; }
    public int Cancelled { get; init; }
    public int Total => Pending + Running + Completed + Failed + Cancelled;
}

/// <summary>
/// Event args for job status changes.
/// </summary>
public class JobStatusChangedEventArgs : EventArgs
{
    public required OptimizationJob Job { get; init; }
    public required JobStatus OldStatus { get; init; }
    public required JobStatus NewStatus { get; init; }
}
