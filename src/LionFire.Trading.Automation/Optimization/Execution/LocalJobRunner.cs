using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using DynamicData;
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

    /// <summary>
    /// Currently active OptimizationTasks keyed by job ID.
    /// </summary>
    private readonly ConcurrentDictionary<string, OptimizationTask> _activeTasks = new();

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
    /// Get the currently active OptimizationTask for a running job.
    /// </summary>
    public object? GetActiveTask(string jobId)
        => _activeTasks.TryGetValue(jobId, out var task) ? task : null;

    /// <summary>
    /// Get all currently active job IDs.
    /// </summary>
    public IEnumerable<string> ActiveJobIds => _activeTasks.Keys;

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

            _logger.LogInformation(
                "Job {JobId} PMultiSim: Bot={BotType}, Symbol={Symbol}, TF={Timeframe}, Start={Start}, End={End}, MaxBacktests={MaxBt}",
                job.Id, pBotType.Name, job.Symbol, job.Timeframe, snappedStart, snappedEnd, job.Resolution.MaxBacktests);

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

            // Track active task for live UI access
            _activeTasks[job.Id] = optimizationTask;

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

            // Remove from active tasks on completion
            _activeTasks.TryRemove(job.Id, out _);

            // Calculate score - read from in-memory journal cache (CSV may not be flushed yet)
            var outputDir = optimizationTask.OptimizationDirectory;
            OptimizationScore? score = null;
            double? bestAD = null;
            int goodBacktestCount = 0;
            int totalBacktests = 0;
            int abortedBacktests = 0;

            var taskProgress = optimizationTask.Progress;
            _logger.LogInformation(
                "Job {JobId} optimization finished: OutputDir={OutputDir}, Progress.Completed={Completed}, Progress.Queued={Queued}, Progress.Skipped={Skipped}",
                job.Id,
                outputDir ?? "(null)",
                taskProgress?.Completed ?? -1,
                taskProgress?.Queued ?? -1,
                taskProgress?.Skipped ?? -1);

            try
            {
                // Prefer in-memory journal cache (always up-to-date, unlike CSV which may not be flushed)
                var journal = optimizationTask.Journal;
                List<BacktestBatchJournalEntry>? backtestResults = null;

                if (journal?.ObservableCache is { Count: > 0 } cache)
                {
                    backtestResults = cache.Items.ToList();
                    _logger.LogInformation(
                        "Job {JobId} read {Count} backtest results from in-memory journal",
                        job.Id, backtestResults.Count);
                }
                else if (!string.IsNullOrEmpty(outputDir))
                {
                    // Flush journal to ensure CSV is written before reading
                    if (journal != null)
                    {
                        await journal.DisposeAsync();
                    }
                    backtestResults = BacktestResultsReader.ReadFromDirectory(outputDir);
                    _logger.LogInformation(
                        "Job {JobId} read {Count} backtest results from CSV at {Dir}",
                        job.Id, backtestResults.Count, outputDir);
                }
                else
                {
                    _logger.LogWarning("Job {JobId}: No journal and no output directory - no results to read", job.Id);
                }

                if (backtestResults != null)
                {
                    abortedBacktests = backtestResults.Count(r => r.IsAborted);
                    totalBacktests = backtestResults.Count - abortedBacktests;

                    if (backtestResults.Count > 0)
                    {
                        var scorer = new OptimizationScorer(backtestResults);
                        score = scorer.Calculate();

                        bestAD = score.Summary?.MaxAd;
                        goodBacktestCount = score.Summary?.PassingCount ?? 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate score for job {JobId}", job.Id);
            }

            // Ensure journal is flushed to disk for future reference
            try
            {
                if (optimizationTask.Journal != null)
                {
                    await optimizationTask.Journal.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error disposing journal for job {JobId} (may already be disposed)", job.Id);
            }

            var completedJob = runningJob with
            {
                Status = JobStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow,
                ResultPath = outputDir,
                Score = score?.Value,
                BestAD = bestAD,
                GoodBacktestCount = goodBacktestCount,
                TotalBacktests = totalBacktests,
                AbortedBacktests = abortedBacktests
            };

            _logger.LogInformation(
                "Completed job {JobId}: Score={Score}, BestAD={BestAD}, Passing={PassingCount}/{Total}, Aborted={Aborted}, Duration={Duration}ms",
                job.Id, score?.Value, bestAD, goodBacktestCount, totalBacktests, abortedBacktests, stopwatch.ElapsedMilliseconds);

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
            _activeTasks.TryRemove(job.Id, out _);

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
            _activeTasks.TryRemove(job.Id, out _);

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
