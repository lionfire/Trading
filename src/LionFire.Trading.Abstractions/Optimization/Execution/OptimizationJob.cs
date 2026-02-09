using LionFire.Trading.Optimization.Plans;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Represents a single optimization job within a plan execution.
/// Each job is one combination of symbol × timeframe × dateRange.
/// </summary>
public record OptimizationJob
{
    /// <summary>
    /// Unique identifier for this job.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// ID of the plan this job belongs to.
    /// </summary>
    public string PlanId { get; init; } = "";

    /// <summary>
    /// Bot type to optimize (e.g., "PAtrBot").
    /// </summary>
    public string Bot { get; init; } = "";

    /// <summary>
    /// Exchange name (e.g., "Binance").
    /// </summary>
    public string Exchange { get; init; } = "Binance";

    /// <summary>
    /// Exchange area (e.g., "spot", "futures").
    /// </summary>
    public string ExchangeArea { get; init; } = "futures";

    /// <summary>
    /// Symbol to run optimization on (e.g., "BTCUSDT").
    /// </summary>
    public string Symbol { get; init; } = "";

    /// <summary>
    /// Timeframe for the optimization (e.g., "h1", "m15").
    /// </summary>
    public string Timeframe { get; init; } = "";

    /// <summary>
    /// Date range for backtesting (stores name and resolved dates).
    /// </summary>
    public OptimizationDateRange DateRange { get; init; } = new();

    /// <summary>
    /// Resolved start date for backtesting.
    /// </summary>
    public DateTimeOffset StartDate { get; init; }

    /// <summary>
    /// Resolved end date for backtesting.
    /// </summary>
    public DateTimeOffset EndDate { get; init; }

    /// <summary>
    /// Resolution settings (maxBacktests, minParameterPriority).
    /// </summary>
    public OptimizationResolution Resolution { get; init; } = new();

    /// <summary>
    /// Execution priority from the matrix state (1 = highest, 9 = lowest).
    /// </summary>
    public int Priority { get; init; } = 5;

    /// <summary>
    /// Current status of this job.
    /// </summary>
    public JobStatus Status { get; init; } = JobStatus.Pending;

    /// <summary>
    /// When the job started executing.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    /// When the job completed (successfully or failed).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Path to the results file, if completed.
    /// </summary>
    public string? ResultPath { get; init; }

    /// <summary>
    /// Calculated score from the scoring system.
    /// </summary>
    public double? Score { get; init; }

    /// <summary>
    /// Best AD (Annualized ROI / Drawdown) achieved.
    /// </summary>
    public double? BestAD { get; init; }

    /// <summary>
    /// Number of backtests that met the scoring threshold.
    /// </summary>
    public int? GoodBacktestCount { get; init; }

    /// <summary>
    /// Total number of backtests run (non-aborted).
    /// </summary>
    public int? TotalBacktests { get; init; }

    /// <summary>
    /// Number of backtests that were aborted (e.g., due to insufficient trades or data issues).
    /// </summary>
    public int? AbortedBacktests { get; init; }

    /// <summary>
    /// Error message if the job failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Duration of job execution.
    /// </summary>
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;
}
