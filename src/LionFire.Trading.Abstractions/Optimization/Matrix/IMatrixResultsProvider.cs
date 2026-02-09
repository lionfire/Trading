namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Provides aggregated optimization results for matrix cells, derived from completed plan execution jobs.
/// </summary>
public interface IMatrixResultsProvider
{
    /// <summary>
    /// Gets the aggregated result for a specific symbol/timeframe cell within a plan.
    /// </summary>
    /// <param name="planId">The plan identifier.</param>
    /// <param name="symbol">The trading symbol (e.g., "BTCUSDT").</param>
    /// <param name="timeframe">The timeframe (e.g., "h1").</param>
    /// <returns>The aggregated cell result, or null if no results exist.</returns>
    Task<MatrixCellResult?> GetCellResultAsync(string planId, string symbol, string timeframe);

    /// <summary>
    /// Gets all aggregated results for a plan, keyed by "symbol|timeframe".
    /// </summary>
    /// <param name="planId">The plan identifier.</param>
    /// <returns>Dictionary of cell results keyed by "symbol|timeframe".</returns>
    Task<Dictionary<string, MatrixCellResult>> GetAllResultsAsync(string planId);

    /// <summary>
    /// Gets execution progress for all cells in a plan, keyed by "symbol|timeframe".
    /// Groups jobs by symbol and timeframe, aggregating their status counts.
    /// </summary>
    /// <param name="planId">The plan identifier.</param>
    /// <returns>Dictionary of cell progress keyed by "symbol|timeframe".</returns>
    Task<Dictionary<string, MatrixCellProgress>> GetProgressAsync(string planId);
}

/// <summary>
/// Aggregated optimization result for a single matrix cell (symbol x timeframe).
/// </summary>
public record MatrixCellResult
{
    /// <summary>
    /// Best AD (Annualized ROI / Drawdown) value across all completed jobs for this cell.
    /// </summary>
    public double BestAd { get; init; }

    /// <summary>
    /// Average AD value across all completed jobs for this cell.
    /// </summary>
    public double AverageAd { get; init; }

    /// <summary>
    /// Total number of backtests run across all jobs for this cell.
    /// </summary>
    public int TotalBacktests { get; init; }

    /// <summary>
    /// Number of backtests with AD >= threshold (typically 1.0).
    /// </summary>
    public int PassingCount { get; init; }

    /// <summary>
    /// Letter grade computed from the best AD score.
    /// </summary>
    public OptimizationGrade Grade { get; init; }

    /// <summary>
    /// Best score value across all completed jobs for this cell.
    /// </summary>
    public double? Score { get; init; }

    /// <summary>
    /// Total number of aborted backtests across all jobs for this cell.
    /// </summary>
    public int AbortedBacktests { get; init; }

    /// <summary>
    /// Number of jobs that completed with errors (job ran but produced no backtests or threw an exception).
    /// </summary>
    public int ErrorJobCount { get; init; }

    /// <summary>
    /// First error message from failed/errored jobs, if any.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// When the most recent job for this cell completed.
    /// </summary>
    public DateTimeOffset? LastRunAt { get; init; }
}
