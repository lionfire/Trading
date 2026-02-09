using System.Threading;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Service for executing optimization plans.
/// </summary>
public interface IPlanExecutionService
{
    /// <summary>
    /// Start executing a plan.
    /// </summary>
    /// <param name="planId">ID of the plan to execute.</param>
    /// <param name="options">Execution options (parallelism, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The initial execution state.</returns>
    Task<PlanExecutionState> StartAsync(
        string planId,
        PlanExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a running execution (cannot be resumed).
    /// </summary>
    /// <param name="planId">ID of the plan to stop.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StopAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause a running execution (can be resumed later).
    /// </summary>
    /// <param name="planId">ID of the plan to pause.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PauseAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume a paused execution.
    /// </summary>
    /// <param name="planId">ID of the plan to resume.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResumeAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the current execution state for a plan.
    /// </summary>
    /// <param name="planId">ID of the plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current execution state, or null if not executing.</returns>
    Task<PlanExecutionState?> GetStatusAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active executions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all active execution states.</returns>
    Task<IReadOnlyList<PlanExecutionState>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry failed jobs for a plan.
    /// </summary>
    /// <param name="planId">ID of the plan.</param>
    /// <param name="jobIds">Optional specific job IDs to retry (all failed if null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RetryFailedAsync(string planId, IEnumerable<string>? jobIds = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Run optimization for a specific cell (symbol/timeframe combination) within a plan.
    /// Creates and executes jobs for just this cell, respecting plan settings.
    /// </summary>
    /// <param name="planId">ID of the plan.</param>
    /// <param name="symbol">The symbol to optimize.</param>
    /// <param name="timeframe">The timeframe to optimize.</param>
    /// <param name="options">Execution options (parallelism, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution state for this cell's jobs.</returns>
    Task<PlanExecutionState> RunCellAsync(
        string planId,
        string symbol,
        string timeframe,
        PlanExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Event fired when execution state changes.
    /// </summary>
    event EventHandler<PlanExecutionStateChangedEventArgs>? StateChanged;
}

/// <summary>
/// Options for plan execution.
/// </summary>
public record PlanExecutionOptions
{
    /// <summary>
    /// Number of parallel workers (jobs running concurrently).
    /// </summary>
    public int ParallelWorkers { get; init; } = 1;

    /// <summary>
    /// How often to auto-save state (in jobs completed).
    /// </summary>
    public int AutoSaveInterval { get; init; } = 10;

    /// <summary>
    /// Whether to resume from a previous paused execution.
    /// </summary>
    public bool ResumeIfPaused { get; init; } = true;

    /// <summary>
    /// Whether to use job prioritization based on promise scores.
    /// When enabled, jobs are executed in order of their promise score rather than sequentially.
    /// </summary>
    public bool UsePrioritization { get; init; } = true;

    /// <summary>
    /// Whether to auto-queue follow-up jobs at higher resolution for promising results.
    /// </summary>
    public bool AutoQueueFollowUps { get; init; } = true;

    /// <summary>
    /// Optional filter to only run jobs for a specific date range (by name).
    /// When null, all date ranges are included.
    /// </summary>
    public string? DateRangeFilter { get; init; }
}

/// <summary>
/// Event args for plan execution state changes.
/// </summary>
public class PlanExecutionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// The updated execution state.
    /// </summary>
    public required PlanExecutionState State { get; init; }

    /// <summary>
    /// Type of change that occurred.
    /// </summary>
    public required ExecutionStateChangeType ChangeType { get; init; }

    /// <summary>
    /// Job that was affected, if applicable.
    /// </summary>
    public OptimizationJob? AffectedJob { get; init; }
}

/// <summary>
/// Type of execution state change.
/// </summary>
public enum ExecutionStateChangeType
{
    /// <summary>Execution started.</summary>
    Started,

    /// <summary>A job started running.</summary>
    JobStarted,

    /// <summary>A job completed.</summary>
    JobCompleted,

    /// <summary>A job failed.</summary>
    JobFailed,

    /// <summary>Execution was paused.</summary>
    Paused,

    /// <summary>Execution was resumed.</summary>
    Resumed,

    /// <summary>Execution completed.</summary>
    Completed,

    /// <summary>Execution was stopped.</summary>
    Stopped
}
