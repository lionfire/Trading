using LionFire.Trading.Optimization.Execution;
using LionFire.Trading.Optimization.Matrix;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Provides aggregated optimization results for matrix cells by reading completed jobs
/// from the plan execution state repository and grouping by symbol and timeframe.
/// </summary>
public class MatrixResultsProvider : IMatrixResultsProvider
{
    private readonly IPlanExecutionStateRepository _executionStateRepository;
    private readonly IPlanExecutionService _executionService;
    private readonly DiskBacktestResultsScanner? _diskScanner;
    private readonly ILogger<MatrixResultsProvider> _logger;

    /// <summary>
    /// Default AD threshold for determining "passing" backtests.
    /// </summary>
    private const double DefaultPassingThreshold = 1.0;

    public MatrixResultsProvider(
        IPlanExecutionStateRepository executionStateRepository,
        IPlanExecutionService executionService,
        ILogger<MatrixResultsProvider> logger,
        DiskBacktestResultsScanner? diskScanner = null)
    {
        _executionStateRepository = executionStateRepository;
        _executionService = executionService;
        _logger = logger;
        _diskScanner = diskScanner;
    }

    /// <inheritdoc />
    public async Task<MatrixCellResult?> GetCellResultAsync(string planId, string symbol, string timeframe)
    {
        var allResults = await GetAllResultsAsync(planId);
        var key = PlanMatrixState.CellKey(symbol, timeframe);
        return allResults.GetValueOrDefault(key);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, MatrixCellResult>> GetAllResultsAsync(string planId)
    {
        var results = new Dictionary<string, MatrixCellResult>();

        // Prefer in-memory execution state (more up-to-date than file)
        PlanExecutionState? executionState = null;
        try
        {
            executionState = await _executionService.GetStatusAsync(planId);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get in-memory execution state for plan '{PlanId}', falling back to file", planId);
        }

        // Fall back to persisted file state
        if (executionState is null)
        {
            try
            {
                executionState = await _executionStateRepository.LoadAsync(planId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load execution state for plan '{PlanId}'", planId);
                return results;
            }
        }

        if (executionState is not null && executionState.Jobs.Count > 0)
        {
            // Group completed jobs by symbol|timeframe
            var completedJobs = executionState.Jobs
                .Where(j => j.Status == JobStatus.Completed)
                .ToList();

            var grouped = completedJobs
                .GroupBy(j => PlanMatrixState.CellKey(j.Symbol, j.Timeframe));

            foreach (var group in grouped)
            {
                var jobs = group.ToList();
                var result = AggregateJobs(jobs);
                if (result is not null)
                {
                    results[group.Key] = result;
                }
            }
        }

        // Fall back to disk scan when no execution state results exist
        if (results.Count == 0 && _diskScanner != null)
        {
            try
            {
                var diskResults = await _diskScanner.ScanForPlanAsync(planId);
                foreach (var kvp in diskResults)
                {
                    results.TryAdd(kvp.Key, kvp.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan disk for results for plan '{PlanId}'", planId);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, MatrixCellProgress>> GetProgressAsync(string planId)
    {
        var progress = new Dictionary<string, MatrixCellProgress>();

        // Prefer in-memory execution state (more up-to-date than file)
        PlanExecutionState? executionState = null;
        try
        {
            executionState = await _executionService.GetStatusAsync(planId);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get in-memory execution state for plan '{PlanId}', falling back to file", planId);
        }

        // Fall back to persisted file state
        if (executionState is null)
        {
            try
            {
                executionState = await _executionStateRepository.LoadAsync(planId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load execution state for progress of plan '{PlanId}'", planId);
                return progress;
            }
        }

        if (executionState is null || executionState.Jobs.Count == 0)
            return progress;

        var grouped = executionState.Jobs
            .GroupBy(j => PlanMatrixState.CellKey(j.Symbol, j.Timeframe));

        foreach (var group in grouped)
        {
            var jobs = group.ToList();
            progress[group.Key] = new MatrixCellProgress
            {
                TotalJobs = jobs.Count,
                CompletedJobs = jobs.Count(j => j.Status == JobStatus.Completed),
                RunningJobs = jobs.Count(j => j.Status == JobStatus.Running),
                FailedJobs = jobs.Count(j => j.Status == JobStatus.Failed),
                PendingJobs = jobs.Count(j => j.Status == JobStatus.Pending),
            };
        }

        return progress;
    }

    /// <summary>
    /// Aggregates multiple optimization jobs for a single cell into a single result.
    /// </summary>
    private static MatrixCellResult? AggregateJobs(List<OptimizationJob> jobs)
    {
        if (jobs.Count == 0) return null;

        var errorJobs = jobs.Where(j => !string.IsNullOrEmpty(j.Error) || (j.TotalBacktests ?? 0) == 0).ToList();
        var firstError = jobs.FirstOrDefault(j => !string.IsNullOrEmpty(j.Error))?.Error;

        var jobsWithAd = jobs.Where(j => j.BestAD.HasValue).ToList();
        if (jobsWithAd.Count == 0)
        {
            // Jobs exist but none have AD scores — check if this is an error condition
            var totalBacktests = jobs.Sum(j => j.TotalBacktests ?? 0);
            var grade = totalBacktests == 0
                ? OptimizationGrade.Error
                : OptimizationGrade.F;
            return new MatrixCellResult
            {
                BestAd = 0,
                AverageAd = 0,
                TotalBacktests = totalBacktests,
                PassingCount = 0,
                Grade = grade,
                ErrorJobCount = errorJobs.Count,
                ErrorMessage = firstError,
                LastRunAt = jobs.Max(j => j.CompletedAt)
            };
        }

        var bestAd = jobsWithAd.Max(j => j.BestAD!.Value);
        var averageAd = jobsWithAd.Average(j => j.BestAD!.Value);
        var totalBacktestsAll = jobs.Sum(j => j.TotalBacktests ?? 0);
        var abortedBacktestsAll = jobs.Sum(j => j.AbortedBacktests ?? 0);
        var passingCount = jobs.Sum(j => j.GoodBacktestCount ?? 0);
        var bestScore = jobsWithAd.Where(j => j.Score.HasValue).Select(j => j.Score!.Value).DefaultIfEmpty(0).Max();
        var lastRunAt = jobs.Max(j => j.CompletedAt);
        var grade2 = OptimizationGradeComputer.ComputeGrade(bestAd);

        return new MatrixCellResult
        {
            BestAd = bestAd,
            AverageAd = averageAd,
            TotalBacktests = totalBacktestsAll,
            AbortedBacktests = abortedBacktestsAll,
            PassingCount = passingCount,
            Score = bestScore > 0 ? bestScore : passingCount,
            Grade = grade2,
            ErrorJobCount = errorJobs.Count,
            ErrorMessage = firstError,
            LastRunAt = lastRunAt
        };
    }
}
