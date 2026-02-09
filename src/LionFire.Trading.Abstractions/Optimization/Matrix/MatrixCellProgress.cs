namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Tracks execution progress for a single matrix cell (symbol x timeframe).
/// Aggregates job counts from the plan execution state.
/// </summary>
public record MatrixCellProgress
{
    /// <summary>
    /// Total number of jobs for this cell.
    /// </summary>
    public int TotalJobs { get; init; }

    /// <summary>
    /// Number of jobs completed successfully.
    /// </summary>
    public int CompletedJobs { get; init; }

    /// <summary>
    /// Number of jobs currently running.
    /// </summary>
    public int RunningJobs { get; init; }

    /// <summary>
    /// Number of jobs that failed.
    /// </summary>
    public int FailedJobs { get; init; }

    /// <summary>
    /// Number of jobs still pending execution.
    /// </summary>
    public int PendingJobs { get; init; }

    /// <summary>
    /// Determines the visual state based on job status counts.
    /// Priority: Running > Failed (with no completions) > Complete > Queued > NeverRun.
    /// </summary>
    public CellVisualState VisualState => RunningJobs > 0 ? CellVisualState.Running
        : FailedJobs > 0 && CompletedJobs == 0 ? CellVisualState.Failed
        : CompletedJobs > 0 ? CellVisualState.Complete
        : PendingJobs > 0 ? CellVisualState.Queued
        : CellVisualState.NeverRun;

    /// <summary>
    /// Human-readable progress text with percentage (e.g., "3/12 (25%)").
    /// </summary>
    public string ProgressText => TotalJobs > 0 ? $"{CompletedJobs}/{TotalJobs} ({ProgressPercent:F0}%)" : "";

    /// <summary>
    /// Progress as a percentage (0-100).
    /// </summary>
    public double ProgressPercent => TotalJobs > 0 ? (double)CompletedJobs / TotalJobs * 100 : 0;

    /// <summary>
    /// Whether any jobs are actively running or pending.
    /// </summary>
    public bool IsActive => RunningJobs > 0 || PendingJobs > 0;
}
