using System.Diagnostics;
using System.Threading;
using LionFire.Trading.Automation.Optimization.Scoring;
using LionFire.Trading.Journal;
using LionFire.Trading.Optimization;
using LionFire.Trading.Optimization.Execution;
using LionFire.Trading.Optimization.Plans;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Execution;

/// <summary>
/// Local job runner that executes optimization jobs in-process using OptimizationTask.
/// </summary>
public class LocalJobRunner : IJobRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BotTypeRegistry _botTypeRegistry;
    private readonly ILogger<LocalJobRunner> _logger;

    public LocalJobRunner(
        IServiceProvider serviceProvider,
        BotTypeRegistry botTypeRegistry,
        ILogger<LocalJobRunner> logger)
    {
        _serviceProvider = serviceProvider;
        _botTypeRegistry = botTypeRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Run an optimization job locally.
    /// </summary>
    public async Task<OptimizationJob> RunAsync(
        OptimizationJob job,
        IProgress<JobProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Starting job {JobId}: {Symbol} {Timeframe} {DateRange}",
                job.Id, job.Symbol, job.Timeframe, job.DateRange.Name);

            // Update job to running state
            var runningJob = job with
            {
                Status = JobStatus.Running,
                StartedAt = DateTimeOffset.UtcNow
            };

            progress?.Report(new JobProgress
            {
                JobId = job.Id,
                Status = JobStatus.Running,
                Message = "Starting optimization"
            });

            // Resolve bot type
            var pBotType = ResolveBotType(job.Bot);

            // Create PMultiSim parameters
            var timeFrame = TimeFrame.Parse(job.Timeframe);
            var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(
                job.Exchange,
                job.ExchangeArea,
                job.Symbol,
                timeFrame);

            // Snap dates to bar boundaries
            var snappedStart = timeFrame.GetPeriodStart(job.StartDate.DateTime);
            if (snappedStart < job.StartDate.DateTime)
            {
                snappedStart = timeFrame.AddBars(snappedStart, 1);
            }
            var snappedEnd = timeFrame.GetPeriodStart(job.EndDate.DateTime);

            var pMultiSim = new PMultiSim
            {
                PBotType = pBotType,
                ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
                Start = snappedStart,
                EndExclusive = snappedEnd,
            };

            // Configure optimization parameters
            pMultiSim.POptimization = new POptimization(pMultiSim)
            {
                MaxBacktests = job.Resolution.MaxBacktests,
                MaxBatchSize = 1024,
                TradeJournalOptions = new TradeJournalOptions
                {
                    Enabled = false, // Disable journals for batch execution
                    KeepTradeJournalsForTopNResults = 0,
                },
            };

            // Create and run optimization
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var optimizationTask = new OptimizationTask(_serviceProvider, pMultiSim);
            optimizationTask.AddCancellationToken(cts.Token);

            await optimizationTask.StartAsync(cts.Token);

            // Monitor progress
            while (optimizationTask.RunTask != null && !optimizationTask.RunTask.IsCompleted)
            {
                await Task.WhenAny(
                    optimizationTask.RunTask,
                    Task.Delay(TimeSpan.FromSeconds(5), cts.Token));

                if (optimizationTask.Progress != null)
                {
                    var p = optimizationTask.Progress;
                    var total = p.Queued > 0 ? p.Queued : p.Completed;
                    var percent = total > 0 ? (double)p.Completed / total : 0;

                    progress?.Report(new JobProgress
                    {
                        JobId = job.Id,
                        Status = JobStatus.Running,
                        PercentComplete = percent,
                        BacktestsCompleted = (int)p.Completed,
                        TotalBacktests = (int)total,
                        Message = $"Running: {p.Completed}/{total} backtests"
                    });
                }
            }

            // Wait for final completion
            if (optimizationTask.RunTask != null)
            {
                await optimizationTask.RunTask;
            }

            stopwatch.Stop();

            // Calculate score
            var outputDir = optimizationTask.OptimizationDirectory;
            OptimizationScore? score = null;
            double? bestAD = null;
            int goodBacktestCount = 0;
            int totalBacktests = 0;

            if (!string.IsNullOrEmpty(outputDir))
            {
                try
                {
                    var backtestResults = BacktestResultsReader.ReadFromDirectory(outputDir);
                    totalBacktests = backtestResults.Count;

                    if (backtestResults.Count > 0)
                    {
                        var scorer = new OptimizationScorer(backtestResults);
                        score = scorer.Calculate();

                        bestAD = score.Summary?.MaxAd;
                        goodBacktestCount = score.Summary?.GoodCount ?? 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to calculate score for job {JobId}", job.Id);
                }
            }

            var completedJob = runningJob with
            {
                Status = JobStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow,
                ResultPath = outputDir,
                Score = score?.Value,
                BestAD = bestAD,
                GoodBacktestCount = goodBacktestCount,
                TotalBacktests = totalBacktests
            };

            _logger.LogInformation(
                "Completed job {JobId}: Score={Score}, BestAD={BestAD}, Good={GoodCount}/{Total}, Duration={Duration}ms",
                job.Id, score?.Value, bestAD, goodBacktestCount, totalBacktests, stopwatch.ElapsedMilliseconds);

            progress?.Report(new JobProgress
            {
                JobId = job.Id,
                Status = JobStatus.Completed,
                PercentComplete = 1.0,
                BacktestsCompleted = totalBacktests,
                TotalBacktests = totalBacktests,
                Message = $"Completed: Score={score?.Value:F0}, BestAD={bestAD:F2}"
            });

            return completedJob;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            var cancelledJob = job with
            {
                Status = JobStatus.Cancelled,
                CompletedAt = DateTimeOffset.UtcNow,
                Error = "Job was cancelled"
            };

            _logger.LogInformation("Job {JobId} was cancelled after {Duration}ms", job.Id, stopwatch.ElapsedMilliseconds);

            progress?.Report(new JobProgress
            {
                JobId = job.Id,
                Status = JobStatus.Cancelled,
                Message = "Job cancelled"
            });

            return cancelledJob;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var failedJob = job with
            {
                Status = JobStatus.Failed,
                CompletedAt = DateTimeOffset.UtcNow,
                Error = ex.Message
            };

            _logger.LogError(ex, "Job {JobId} failed after {Duration}ms: {Error}", job.Id, stopwatch.ElapsedMilliseconds, ex.Message);

            progress?.Report(new JobProgress
            {
                JobId = job.Id,
                Status = JobStatus.Failed,
                Message = $"Failed: {ex.Message}"
            });

            return failedJob;
        }
    }

    /// <summary>
    /// Resolve the PBot type from the bot name.
    /// </summary>
    private Type ResolveBotType(string botName)
    {
        var pBotType = _botTypeRegistry.GetPBotType(botName);

        if (pBotType == null)
        {
            // Try with 'P' prefix
            pBotType = _botTypeRegistry.GetPBotType("P" + botName);
        }

        if (pBotType == null)
        {
            throw new InvalidOperationException($"Bot type not found: {botName}");
        }

        // Close generic type if needed
        if (pBotType.IsGenericTypeDefinition)
        {
            var typeArgs = pBotType.GetGenericArguments();
            var closedArgs = typeArgs.Select(_ => typeof(double)).ToArray();
            pBotType = pBotType.MakeGenericType(closedArgs);
        }

        return pBotType;
    }
}
