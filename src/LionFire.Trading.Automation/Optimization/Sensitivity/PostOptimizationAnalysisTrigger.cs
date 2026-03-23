using LionFire.Trading.Automation.Optimization.Execution;
using LionFire.Trading.Optimization.Execution;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Listens for optimization completion events and triggers sensitivity analysis automatically.
/// Skips analysis when results are insufficient (too few cells or too few trades).
/// </summary>
public class PostOptimizationAnalysisTrigger : IHostedService
{
    private readonly PlanExecutionEngine _executionEngine;
    private readonly SensitivityAnalyzer _analyzer;
    private readonly StepRecommendationPersistence _persistence;
    private readonly IOptionsMonitor<SensitivityAnalysisOptions> _options;
    private readonly IOptionsMonitor<BacktestOptions> _backtestOptions;
    private readonly ILogger<PostOptimizationAnalysisTrigger> _logger;

    /// <summary>
    /// Minimum number of backtest entries needed to run analysis.
    /// </summary>
    private const int MinEntriesForAnalysis = 10;

    public PostOptimizationAnalysisTrigger(
        PlanExecutionEngine executionEngine,
        SensitivityAnalyzer analyzer,
        StepRecommendationPersistence persistence,
        IOptionsMonitor<SensitivityAnalysisOptions> options,
        IOptionsMonitor<BacktestOptions> backtestOptions,
        ILogger<PostOptimizationAnalysisTrigger> logger)
    {
        _executionEngine = executionEngine;
        _analyzer = analyzer;
        _persistence = persistence;
        _options = options;
        _backtestOptions = backtestOptions;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executionEngine.StateChanged += OnStateChanged;
        _logger.LogInformation("Post-optimization sensitivity analysis trigger registered");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _executionEngine.StateChanged -= OnStateChanged;
        return Task.CompletedTask;
    }

    private void OnStateChanged(object? sender, PlanExecutionStateChangedEventArgs e)
    {
        if (e.ChangeType != ExecutionStateChangeType.Completed) return;

        // Fire-and-forget — don't block the execution engine
        _ = Task.Run(async () =>
        {
            try
            {
                await AnalyzeCompletedPlanAsync(e.State);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run post-optimization sensitivity analysis for plan {PlanId}", e.State.PlanId);
            }
        });
    }

    private async Task AnalyzeCompletedPlanAsync(PlanExecutionState state)
    {
        if (state.TotalJobs == 0)
        {
            _logger.LogDebug("Skipping sensitivity analysis for plan {PlanId}: no jobs", state.PlanId);
            return;
        }

        // Find result directories for completed jobs
        var completedJobs = state.Jobs.Where(j => j.Status == JobStatus.Completed && j.ResultPath != null).ToList();
        if (completedJobs.Count == 0)
        {
            _logger.LogDebug("Skipping sensitivity analysis for plan {PlanId}: no completed jobs with results", state.PlanId);
            return;
        }

        var baseDir = _backtestOptions.CurrentValue.Dir;
        if (string.IsNullOrEmpty(baseDir)) return;

        int analyzed = 0;
        foreach (var job in completedJobs)
        {
            try
            {
                var resultsPath = Path.Combine(baseDir, job.ResultPath!);
                if (!Directory.Exists(resultsPath) && !File.Exists(resultsPath)) continue;

                var recommendations = _analyzer.Analyze(
                    resultsPath,
                    planId: state.PlanId,
                    botType: job.Bot,
                    symbol: job.Symbol,
                    timeFrame: job.Timeframe);

                if (recommendations.Recommendations.Count == 0)
                {
                    _logger.LogDebug("No sensitivity recommendations for {Symbol}/{Timeframe} in plan {PlanId} (insufficient data)",
                        job.Symbol, job.Timeframe, state.PlanId);
                    continue;
                }

                var saveDir = Directory.Exists(resultsPath)
                    ? resultsPath
                    : Path.GetDirectoryName(resultsPath)!;

                await _persistence.SaveAsync(saveDir, recommendations);
                analyzed++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze job {Symbol}/{Timeframe} in plan {PlanId}",
                    job.Symbol, job.Timeframe, state.PlanId);
            }
        }

        if (analyzed > 0)
        {
            _logger.LogInformation("Post-optimization sensitivity analysis for plan {PlanId}: analyzed {Count}/{Total} completed jobs",
                state.PlanId, analyzed, completedJobs.Count);
        }
    }
}
