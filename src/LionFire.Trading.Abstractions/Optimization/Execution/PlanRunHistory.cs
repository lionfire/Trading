namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Represents a historical execution run of an optimization plan.
/// </summary>
public record PlanRunHistory
{
    /// <summary>
    /// Unique identifier for this run.
    /// </summary>
    public string RunId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// ID of the plan this run belongs to.
    /// </summary>
    public string PlanId { get; init; } = "";

    /// <summary>
    /// When the execution started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// When the execution completed (or null if still running/cancelled).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Total duration of the execution.
    /// </summary>
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt - StartedAt : null;

    /// <summary>
    /// Total number of jobs in this execution.
    /// </summary>
    public int TotalJobs { get; init; }

    /// <summary>
    /// Number of jobs that completed successfully.
    /// </summary>
    public int CompletedJobs { get; init; }

    /// <summary>
    /// Number of jobs that failed.
    /// </summary>
    public int FailedJobs { get; init; }

    /// <summary>
    /// Best AD (Annualized ROI / Drawdown) achieved in this run.
    /// </summary>
    public double? BestAD { get; init; }

    /// <summary>
    /// Average AD across all completed jobs.
    /// </summary>
    public double? AverageAD { get; init; }

    /// <summary>
    /// Number of jobs with AD >= 1.0.
    /// </summary>
    public int GoodJobCount { get; init; }

    /// <summary>
    /// Final status of the execution.
    /// </summary>
    public PlanExecutionStatus FinalStatus { get; init; }

    /// <summary>
    /// Optional notes about this run.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Path to the stored results, if persisted.
    /// </summary>
    public string? ResultsPath { get; init; }
}
