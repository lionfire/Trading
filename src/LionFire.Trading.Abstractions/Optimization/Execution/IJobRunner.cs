using System.Threading;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Interface for executing individual optimization jobs.
/// Implementations can run locally or distributed.
/// </summary>
public interface IJobRunner
{
    /// <summary>
    /// Execute an optimization job asynchronously.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token for graceful stop.</param>
    /// <returns>The completed job with results populated.</returns>
    Task<OptimizationJob> RunAsync(
        OptimizationJob job,
        IProgress<JobProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the currently active OptimizationTask for a running job, if any.
    /// Returns null if the job is not currently running or has completed.
    /// </summary>
    /// <remarks>
    /// The returned object is untyped to avoid a dependency on LionFire.Trading.Automation
    /// from the abstractions layer. Callers should cast to OptimizationTask.
    /// </remarks>
    object? GetActiveTask(string jobId) => null;

    /// <summary>
    /// Get all currently active job IDs.
    /// </summary>
    IEnumerable<string> ActiveJobIds => Enumerable.Empty<string>();
}

/// <summary>
/// Progress information for a running job.
/// </summary>
public record JobProgress
{
    /// <summary>
    /// Job ID.
    /// </summary>
    public string JobId { get; init; } = "";

    /// <summary>
    /// Current job status.
    /// </summary>
    public JobStatus Status { get; init; }

    /// <summary>
    /// Number of backtests completed.
    /// </summary>
    public int BacktestsCompleted { get; init; }

    /// <summary>
    /// Total backtests to run.
    /// </summary>
    public int TotalBacktests { get; init; }

    /// <summary>
    /// Percentage complete (0.0-1.0).
    /// </summary>
    public double PercentComplete { get; init; }

    /// <summary>
    /// Current best AD found so far.
    /// </summary>
    public double? BestADSoFar { get; init; }

    /// <summary>
    /// Status message.
    /// </summary>
    public string? Message { get; init; }
}
