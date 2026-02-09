namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Tracks the state of a plan execution, including progress and all jobs.
/// </summary>
public record PlanExecutionState
{
    /// <summary>
    /// ID of the plan being executed.
    /// </summary>
    public string PlanId { get; init; } = "";

    /// <summary>
    /// Unique ID for this execution run (allows multiple runs of same plan).
    /// </summary>
    public string ExecutionId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Current execution status.
    /// </summary>
    public PlanExecutionStatus Status { get; init; } = PlanExecutionStatus.NotStarted;

    /// <summary>
    /// Total number of jobs in this execution.
    /// </summary>
    public int TotalJobs { get; init; }

    /// <summary>
    /// Number of jobs completed successfully.
    /// </summary>
    public int CompletedJobs { get; init; }

    /// <summary>
    /// Number of jobs that failed.
    /// </summary>
    public int FailedJobs { get; init; }

    /// <summary>
    /// Number of jobs currently running.
    /// </summary>
    public int RunningJobs { get; init; }

    /// <summary>
    /// When execution started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    /// When execution was last paused.
    /// </summary>
    public DateTimeOffset? PausedAt { get; init; }

    /// <summary>
    /// When execution completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// All jobs in this execution.
    /// </summary>
    public IReadOnlyList<OptimizationJob> Jobs { get; init; } = [];

    /// <summary>
    /// Number of parallel workers configured.
    /// </summary>
    public int ParallelWorkers { get; init; } = 1;

    /// <summary>
    /// Best AD score across all completed jobs.
    /// </summary>
    public double? BestAD { get; init; }

    /// <summary>
    /// Average AD score across all completed jobs.
    /// </summary>
    public double? AverageAD { get; init; }

    /// <summary>
    /// Total jobs with AD >= 1.0.
    /// </summary>
    public int GoodJobCount { get; init; }

    /// <summary>
    /// Execution log entries.
    /// </summary>
    public IReadOnlyList<string> Log { get; init; } = [];

    /// <summary>
    /// History of past executions (most recent first, limited to 20).
    /// </summary>
    public IReadOnlyList<PlanRunHistory> RunHistory { get; init; } = [];

    /// <summary>
    /// Completion percentage (0-100).
    /// </summary>
    public double ProgressPercent => TotalJobs > 0
        ? Math.Round((CompletedJobs + FailedJobs) * 100.0 / TotalJobs, 1)
        : 0;

    /// <summary>
    /// Number of pending jobs.
    /// </summary>
    public int PendingJobs => TotalJobs - CompletedJobs - FailedJobs - RunningJobs;

    /// <summary>
    /// Estimated time to completion based on average job duration.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (CompletedJobs == 0 || !StartedAt.HasValue) return null;
            var elapsed = DateTimeOffset.UtcNow - StartedAt.Value;
            var avgTimePerJob = elapsed / CompletedJobs;
            return avgTimePerJob * PendingJobs;
        }
    }

    /// <summary>
    /// Whether job prioritization is enabled for this execution.
    /// </summary>
    public bool UsePrioritization { get; init; } = true;

    /// <summary>
    /// Number of jobs that were selected using prioritization.
    /// </summary>
    public int PrioritizedJobsExecuted { get; init; }

    /// <summary>
    /// Number of follow-up jobs auto-queued based on promising results.
    /// </summary>
    public int FollowUpJobsQueued { get; init; }
}
