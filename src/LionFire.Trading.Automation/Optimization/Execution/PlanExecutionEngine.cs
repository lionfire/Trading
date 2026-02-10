using System.Collections.Concurrent;
using System.Threading;
using LionFire.Trading.Automation.Optimization.Prioritization;
using LionFire.Trading.Optimization.Execution;
using LionFire.Trading.Optimization.Matrix;
using LionFire.Trading.Optimization.Plans;
using LionFire.Trading.Symbols;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Execution;

/// <summary>
/// Orchestrates the execution of optimization plans with parallel worker support.
/// </summary>
public class PlanExecutionEngine : IPlanExecutionService
{
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IJobQueueService _jobQueue;
    private readonly IJobRunner _jobRunner;
    private readonly IPlanExecutionStateRepository? _stateRepository;
    private readonly IJobPrioritizer? _prioritizer;
    private readonly IPlanMatrixService? _matrixService;
    private readonly JobMatrixGenerator _matrixGenerator;
    private readonly ILogger<PlanExecutionEngine> _logger;

    private readonly ConcurrentDictionary<string, ExecutionContext> _activeExecutions = new();
    private readonly ConcurrentDictionary<string, PlanExecutionState> _completedExecutions = new();

    public event EventHandler<PlanExecutionStateChangedEventArgs>? StateChanged;

    public PlanExecutionEngine(
        IOptimizationPlanRepository planRepository,
        IJobQueueService jobQueue,
        IJobRunner jobRunner,
        ISymbolCollectionRepository? symbolRepository,
        IPlanExecutionStateRepository? stateRepository,
        IJobPrioritizer? prioritizer,
        IPlanMatrixService? matrixService,
        ILogger<PlanExecutionEngine> logger)
    {
        _planRepository = planRepository;
        _jobQueue = jobQueue;
        _jobRunner = jobRunner;
        _stateRepository = stateRepository;
        _prioritizer = prioritizer;
        _matrixService = matrixService;
        _matrixGenerator = new JobMatrixGenerator(symbolRepository);
        _logger = logger;
    }

    public async Task<PlanExecutionState> StartAsync(
        string planId,
        PlanExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new PlanExecutionOptions();

        // Check for existing in-memory execution
        if (_activeExecutions.TryGetValue(planId, out var existingContext))
        {
            // Check if the worker task is stale (completed/faulted)
            if (existingContext.WorkerTask is { IsCompleted: true })
            {
                _logger.LogWarning(
                    "Stale execution context found for plan {PlanId} (worker task status: {Status}). Cleaning up.",
                    planId, existingContext.WorkerTask.Status);
                _activeExecutions.TryRemove(planId, out _);
                await _jobQueue.ClearAsync(planId, cancellationToken);
                // Fall through to create a fresh execution
            }
            else if (existingContext.State.Status == PlanExecutionStatus.Paused && options.ResumeIfPaused)
            {
                await ResumeAsync(planId, cancellationToken);
                return existingContext.State;
            }
            else
            {
                throw new InvalidOperationException($"Plan {planId} is already executing");
            }
        }

        // Check for persisted state (resume from disk)
        if (options.ResumeIfPaused && _stateRepository != null)
        {
            var persistedState = await _stateRepository.LoadAsync(planId, cancellationToken);
            if (persistedState != null && persistedState.Status == PlanExecutionStatus.Paused)
            {
                _logger.LogInformation("Resuming execution of plan {PlanId} from persisted state", planId);
                return await ResumeFromPersistedStateAsync(persistedState, options, cancellationToken);
            }
        }

        // Load the plan
        var plan = await _planRepository.GetAsync(planId, cancellationToken);
        if (plan == null)
        {
            throw new InvalidOperationException($"Plan {planId} not found");
        }

        _logger.LogInformation("Starting execution of plan {PlanId}: {PlanName}", planId, plan.Name);

        // Load matrix state for priority assignment
        PlanMatrixState? matrixState = null;
        if (_matrixService != null)
        {
            try
            {
                matrixState = await _matrixService.GetStateAsync(planId);
                _logger.LogInformation("Loaded matrix state for plan {PlanId}: {CellCount} cell states, {RowCount} row states, {ColCount} column states",
                    planId, matrixState.CellStates.Count, matrixState.RowStates.Count, matrixState.ColumnStates.Count);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not load matrix state for plan {PlanId}, using default priorities", planId); }
        }
        else
        {
            _logger.LogWarning("No IPlanMatrixService available — all jobs will use default priority 5");
        }

        // Generate job matrix
        var jobs = await _matrixGenerator.GenerateAsync(plan, matrixState: matrixState, cancellationToken: cancellationToken);

        var priorityGroups = jobs.GroupBy(j => j.Priority).OrderBy(g => g.Key);
        _logger.LogInformation("Generated {JobCount} jobs for plan {PlanId}. Priority distribution: {Priorities}",
            jobs.Count, planId,
            string.Join(", ", priorityGroups.Select(g => $"P{g.Key}={g.Count()}")));

        // Log first few jobs to verify order
        foreach (var job in jobs.OrderBy(j => j.Priority).ThenBy(j => j.Symbol).ThenBy(j => j.Timeframe).Take(5))
        {
            _logger.LogInformation("  Job P{Priority}: {Symbol} {Timeframe}", job.Priority, job.Symbol, job.Timeframe);
        }

        // Create execution context
        var executionId = Guid.NewGuid().ToString("N");
        var state = new PlanExecutionState
        {
            PlanId = planId,
            ExecutionId = executionId,
            Status = PlanExecutionStatus.Running,
            TotalJobs = jobs.Count,
            StartedAt = DateTimeOffset.UtcNow,
            ParallelWorkers = options.ParallelWorkers,
            Jobs = jobs.ToList(),
            UsePrioritization = options.UsePrioritization && _prioritizer != null
        };

        var context = new ExecutionContext(state, options);
        if (!_activeExecutions.TryAdd(planId, context))
        {
            throw new InvalidOperationException($"Failed to register execution for plan {planId}");
        }

        // Clear previous completed state and stale queue jobs
        _completedExecutions.TryRemove(planId, out _);
        await _jobQueue.ClearAsync(planId, cancellationToken);

        // Enqueue all jobs
        await _jobQueue.EnqueueBatchAsync(jobs, cancellationToken);

        // Save initial state
        await SaveStateAsync(state);

        // Fire started event
        RaiseStateChanged(state, ExecutionStateChangeType.Started, null);

        // Start worker tasks
        context.WorkerTask = Task.Run(() => RunWorkersAsync(context, cancellationToken), cancellationToken);

        return state;
    }

    private async Task<PlanExecutionState> ResumeFromPersistedStateAsync(
        PlanExecutionState persistedState,
        PlanExecutionOptions options,
        CancellationToken cancellationToken)
    {
        // Re-enqueue pending jobs
        var pendingJobs = persistedState.Jobs
            .Where(j => j.Status == JobStatus.Pending)
            .ToList();

        if (pendingJobs.Count > 0)
        {
            await _jobQueue.EnqueueBatchAsync(pendingJobs, cancellationToken);
        }

        // Update state to running
        var runningState = persistedState with
        {
            Status = PlanExecutionStatus.Running,
            ParallelWorkers = options.ParallelWorkers
        };

        var context = new ExecutionContext(runningState, options);
        if (!_activeExecutions.TryAdd(runningState.PlanId, context))
        {
            throw new InvalidOperationException($"Failed to register execution for plan {runningState.PlanId}");
        }

        // Fire resumed event
        RaiseStateChanged(runningState, ExecutionStateChangeType.Resumed, null);

        // Start worker tasks
        context.WorkerTask = Task.Run(() => RunWorkersAsync(context, cancellationToken), cancellationToken);

        return runningState;
    }

    public async Task StopAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (!_activeExecutions.TryGetValue(planId, out var context))
        {
            throw new InvalidOperationException($"No active execution for plan {planId}");
        }

        _logger.LogInformation("Stopping execution of plan {PlanId}", planId);

        // Cancel workers
        await context.CancellationTokenSource.CancelAsync();

        // Wait for workers to finish
        if (context.WorkerTask != null)
        {
            try
            {
                await context.WorkerTask.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Worker task did not complete within timeout for plan {PlanId}", planId);
            }
        }

        // Update state
        var updatedState = context.State with
        {
            Status = PlanExecutionStatus.Failed,
            CompletedAt = DateTimeOffset.UtcNow
        };
        context.State = updatedState;

        // Clear job queue
        await _jobQueue.ClearAsync(planId, cancellationToken);

        // Remove from active
        _activeExecutions.TryRemove(planId, out _);

        RaiseStateChanged(updatedState, ExecutionStateChangeType.Stopped, null);
    }

    public async Task PauseAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (!_activeExecutions.TryGetValue(planId, out var context))
        {
            throw new InvalidOperationException($"No active execution for plan {planId}");
        }

        if (context.State.Status != PlanExecutionStatus.Running)
        {
            throw new InvalidOperationException($"Plan {planId} is not running");
        }

        _logger.LogInformation("Pausing execution of plan {PlanId}", planId);

        // Signal workers to pause
        context.IsPaused = true;

        // Update state
        var updatedState = context.State with
        {
            Status = PlanExecutionStatus.Paused,
            PausedAt = DateTimeOffset.UtcNow
        };
        context.State = updatedState;

        // Save state for resumption
        await SaveStateAsync(updatedState);

        RaiseStateChanged(updatedState, ExecutionStateChangeType.Paused, null);
    }

    public async Task ResumeAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (!_activeExecutions.TryGetValue(planId, out var context))
        {
            throw new InvalidOperationException($"No active execution for plan {planId}");
        }

        if (context.State.Status != PlanExecutionStatus.Paused)
        {
            throw new InvalidOperationException($"Plan {planId} is not paused");
        }

        _logger.LogInformation("Resuming execution of plan {PlanId}", planId);

        // Resume workers
        context.IsPaused = false;
        context.PauseEvent.Set();

        // Update state
        var updatedState = context.State with
        {
            Status = PlanExecutionStatus.Running
        };
        context.State = updatedState;

        RaiseStateChanged(updatedState, ExecutionStateChangeType.Resumed, null);

        await Task.CompletedTask;
    }

    public async Task<PlanExecutionState?> GetStatusAsync(string planId, CancellationToken cancellationToken = default)
    {
        // Check active (running) executions first
        if (_activeExecutions.TryGetValue(planId, out var context))
        {
            return context.State;
        }

        // Check in-memory completed executions
        if (_completedExecutions.TryGetValue(planId, out var completedState))
        {
            return completedState;
        }

        // Fall back to persisted state (completed/failed executions from previous Silo sessions)
        if (_stateRepository != null)
        {
            return await _stateRepository.LoadAsync(planId, cancellationToken);
        }

        return null;
    }

    public Task<IReadOnlyList<PlanExecutionState>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var states = _activeExecutions.Values.Select(c => c.State)
            .Concat(_completedExecutions.Values)
            .GroupBy(s => s.PlanId)
            .Select(g => g.First()) // active takes precedence (listed first)
            .ToList();
        return Task.FromResult<IReadOnlyList<PlanExecutionState>>(states);
    }

    public async Task RetryFailedAsync(string planId, IEnumerable<string>? jobIds = null, CancellationToken cancellationToken = default)
    {
        if (!_activeExecutions.TryGetValue(planId, out var context))
        {
            throw new InvalidOperationException($"No active execution for plan {planId}");
        }

        // Get failed jobs
        var failedJobs = await _jobQueue.GetByStatusAsync(JobStatus.Failed, planId, cancellationToken);

        if (jobIds != null)
        {
            var jobIdSet = jobIds.ToHashSet();
            failedJobs = failedJobs.Where(j => jobIdSet.Contains(j.Id)).ToList();
        }

        _logger.LogInformation("Retrying {Count} failed jobs for plan {PlanId}", failedJobs.Count, planId);

        // Reset jobs to pending
        foreach (var job in failedJobs)
        {
            var resetJob = job with
            {
                Status = JobStatus.Pending,
                Error = null,
                StartedAt = null,
                CompletedAt = null
            };
            await _jobQueue.UpdateAsync(resetJob, cancellationToken);
        }

        // Update state
        var state = context.State;
        var updatedState = state with
        {
            FailedJobs = state.FailedJobs - failedJobs.Count
        };
        context.State = updatedState;
    }

    public async Task<PlanExecutionState> RunCellAsync(
        string planId,
        string symbol,
        string timeframe,
        PlanExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new PlanExecutionOptions();

        // Load the plan
        var plan = await _planRepository.GetAsync(planId, cancellationToken);
        if (plan == null)
        {
            throw new InvalidOperationException($"Plan {planId} not found");
        }

        _logger.LogInformation(
            "Running cell {Symbol}/{Timeframe} for plan {PlanId}: {PlanName}",
            symbol, timeframe, planId, plan.Name);

        // Load matrix state for priority assignment
        PlanMatrixState? matrixState = null;
        if (_matrixService != null)
        {
            try { matrixState = await _matrixService.GetStateAsync(planId); }
            catch (Exception ex) { _logger.LogDebug(ex, "Could not load matrix state for cell run {PlanId}, using default priorities", planId); }
        }

        // Generate jobs for just this cell (filtered by date range if specified)
        var jobs = await _matrixGenerator.GenerateAsync(
            plan,
            symbolFilter: [symbol],
            timeframeFilter: [timeframe],
            dateRangeFilter: options?.DateRangeFilter,
            matrixState: matrixState,
            cancellationToken: cancellationToken);

        if (jobs.Count == 0)
        {
            throw new InvalidOperationException($"No jobs generated for {symbol}/{timeframe} in plan {planId}");
        }

        _logger.LogInformation("Generated {JobCount} jobs for cell {Symbol}/{Timeframe}", jobs.Count, symbol, timeframe);

        // Check if there's an active execution context for this plan
        if (_activeExecutions.TryGetValue(planId, out var existingContext))
        {
            // Check if the worker task is still alive
            if (existingContext.WorkerTask is { IsCompleted: true })
            {
                _logger.LogWarning(
                    "Stale execution context found for plan {PlanId} (worker task completed with status {Status}). Cleaning up and starting fresh.",
                    planId, existingContext.WorkerTask.Status);
                _activeExecutions.TryRemove(planId, out _);
                await _jobQueue.ClearAsync(planId, cancellationToken);
                // Fall through to create a new execution context below
            }
            else
            {
                // Add jobs to existing execution
                await _jobQueue.EnqueueBatchAsync(jobs, cancellationToken);

                // Update the state with new jobs
                lock (existingContext.StateLock)
                {
                    var updatedJobs = existingContext.State.Jobs.Concat(jobs).ToList();
                    existingContext.State = existingContext.State with
                    {
                        TotalJobs = existingContext.State.TotalJobs + jobs.Count,
                        Jobs = updatedJobs
                    };
                }

                _logger.LogInformation(
                    "Added {JobCount} jobs to existing execution for plan {PlanId}",
                    jobs.Count, planId);

                return existingContext.State;
            }
        }

        // Collect previously completed jobs to preserve across runs
        var previouslyCompletedJobs = new List<OptimizationJob>();
        if (_completedExecutions.TryGetValue(planId, out var prevCompleted))
        {
            previouslyCompletedJobs.AddRange(
                prevCompleted.Jobs.Where(j => j.Status == JobStatus.Completed));
        }
        else if (_stateRepository != null)
        {
            try
            {
                var prevState = await _stateRepository.LoadAsync(planId, cancellationToken);
                if (prevState?.Jobs != null)
                {
                    previouslyCompletedJobs.AddRange(
                        prevState.Jobs.Where(j => j.Status == JobStatus.Completed));
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not load previous state for plan {PlanId} to merge results", planId);
            }
        }

        if (previouslyCompletedJobs.Count > 0)
        {
            _logger.LogInformation(
                "Merging {PrevCount} previously completed jobs with {NewCount} new jobs for plan {PlanId}",
                previouslyCompletedJobs.Count, jobs.Count, planId);
        }

        // Create a new execution context for this cell, including previously completed jobs
        var allJobs = previouslyCompletedJobs.Concat(jobs).ToList();
        var prevCompletedCount = previouslyCompletedJobs.Count;
        var executionId = Guid.NewGuid().ToString("N");
        var state = new PlanExecutionState
        {
            PlanId = planId,
            ExecutionId = executionId,
            Status = PlanExecutionStatus.Running,
            TotalJobs = allJobs.Count,
            CompletedJobs = prevCompletedCount,
            StartedAt = DateTimeOffset.UtcNow,
            ParallelWorkers = options.ParallelWorkers,
            Jobs = allJobs,
            UsePrioritization = options.UsePrioritization && _prioritizer != null
        };

        var context = new ExecutionContext(state, options);
        if (!_activeExecutions.TryAdd(planId, context))
        {
            // Another execution started in between - add to it instead
            if (_activeExecutions.TryGetValue(planId, out var concurrentContext))
            {
                await _jobQueue.EnqueueBatchAsync(jobs, cancellationToken);
                lock (concurrentContext.StateLock)
                {
                    var updatedJobs = concurrentContext.State.Jobs.Concat(jobs).ToList();
                    concurrentContext.State = concurrentContext.State with
                    {
                        TotalJobs = concurrentContext.State.TotalJobs + jobs.Count,
                        Jobs = updatedJobs
                    };
                }
                return concurrentContext.State;
            }
            throw new InvalidOperationException($"Failed to register execution for plan {planId}");
        }

        // Clear previous completed state and stale queue jobs
        _completedExecutions.TryRemove(planId, out _);
        await _jobQueue.ClearAsync(planId, cancellationToken);

        // Enqueue jobs
        await _jobQueue.EnqueueBatchAsync(jobs, cancellationToken);

        // Save initial state so progress is visible immediately
        await SaveStateAsync(state);

        // Fire started event
        RaiseStateChanged(state, ExecutionStateChangeType.Started, null);

        // Start worker tasks
        context.WorkerTask = Task.Run(() => RunWorkersAsync(context, cancellationToken), cancellationToken);

        return state;
    }

    private async Task RunWorkersAsync(ExecutionContext context, CancellationToken externalCancellation)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            externalCancellation, context.CancellationTokenSource.Token);
        var cancellationToken = linkedCts.Token;

        var semaphore = new SemaphoreSlim(context.Options.ParallelWorkers);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Check for pause
                while (context.IsPaused && !cancellationToken.IsCancellationRequested)
                {
                    context.PauseEvent.Wait(cancellationToken);
                }

                // Acquire semaphore BEFORE dequeuing to avoid pre-fetching the next job
                await semaphore.WaitAsync(cancellationToken);

                // Try to get next job - use prioritizer if enabled
                var job = await GetNextJobAsync(context, cancellationToken);
                if (job == null)
                {
                    semaphore.Release();

                    // Check if all jobs are done
                    var counts = await _jobQueue.GetCountsAsync(context.State.PlanId, cancellationToken);
                    if (counts.Running == 0 && counts.Pending == 0)
                    {
                        // All done
                        break;
                    }

                    // Wait a bit and try again
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                // Fire job started event
                var state = context.State;
                var startedState = state with
                {
                    RunningJobs = state.RunningJobs + 1
                };
                context.State = startedState;
                RaiseStateChanged(startedState, ExecutionStateChangeType.JobStarted, job);

                // Run job in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var completedJob = await _jobRunner.RunAsync(job, null, cancellationToken);
                        await _jobQueue.UpdateAsync(completedJob, cancellationToken);

                        // Check for follow-up job
                        await CheckAndQueueFollowUpAsync(context, completedJob, cancellationToken);

                        // Update state
                        UpdateStateAfterJobCompletion(context, completedJob);
                    }
                    catch (OperationCanceledException)
                    {
                        // Job was cancelled, update to cancelled status
                        var cancelledJob = job with
                        {
                            Status = JobStatus.Cancelled,
                            CompletedAt = DateTimeOffset.UtcNow
                        };
                        await _jobQueue.UpdateAsync(cancelledJob, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Job {JobId} failed with exception", job.Id);
                        var failedJob = job with
                        {
                            Status = JobStatus.Failed,
                            Error = ex.Message,
                            CompletedAt = DateTimeOffset.UtcNow
                        };
                        try
                        {
                            await _jobQueue.UpdateAsync(failedJob, CancellationToken.None);
                            UpdateStateAfterJobCompletion(context, failedJob);
                        }
                        catch
                        {
                            // Ignore update failures
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);
            }

            // Wait for all running jobs to complete
            while (semaphore.CurrentCount < context.Options.ParallelWorkers)
            {
                await Task.Delay(100, CancellationToken.None);
            }

            // Mark execution as complete
            var finalState = context.State with
            {
                Status = PlanExecutionStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow
            };
            context.State = finalState;

            _logger.LogInformation(
                "Plan {PlanId} execution completed: {Completed} completed, {Failed} failed",
                context.State.PlanId, finalState.CompletedJobs, finalState.FailedJobs);

            // Save final completed state so it can be viewed after execution
            await SaveStateAsync(finalState);

            // Keep completed state in memory for immediate UI access
            _completedExecutions[context.State.PlanId] = finalState;

            RaiseStateChanged(finalState, ExecutionStateChangeType.Completed, null);

            // Remove from active executions
            _activeExecutions.TryRemove(context.State.PlanId, out _);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Plan {PlanId} execution was cancelled", context.State.PlanId);
            _activeExecutions.TryRemove(context.State.PlanId, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Plan {PlanId} execution failed with error", context.State.PlanId);
            _activeExecutions.TryRemove(context.State.PlanId, out _);
        }
    }

    private void UpdateStateAfterJobCompletion(ExecutionContext context, OptimizationJob job)
    {
        PlanExecutionState updatedState;
        bool shouldAutoSave;

        lock (context.StateLock)
        {
            var state = context.State;

            var isSuccess = job.Status == JobStatus.Completed;
            var newCompletedJobs = isSuccess ? state.CompletedJobs + 1 : state.CompletedJobs;
            var newFailedJobs = job.Status == JobStatus.Failed ? state.FailedJobs + 1 : state.FailedJobs;
            var newRunningJobs = Math.Max(0, state.RunningJobs - 1);

            // Update AD stats
            double? newBestAD = state.BestAD;
            double? newAverageAD = state.AverageAD;
            int newGoodJobCount = state.GoodJobCount;

            if (isSuccess && job.BestAD.HasValue)
            {
                if (!newBestAD.HasValue || job.BestAD.Value > newBestAD.Value)
                {
                    newBestAD = job.BestAD.Value;
                }

                // Recalculate average (incremental)
                if (newAverageAD.HasValue)
                {
                    newAverageAD = (newAverageAD.Value * state.CompletedJobs + job.BestAD.Value) / newCompletedJobs;
                }
                else
                {
                    newAverageAD = job.BestAD.Value;
                }

                if (job.BestAD.Value >= 1.0)
                {
                    newGoodJobCount++;
                }
            }

            // Update jobs list
            var updatedJobs = state.Jobs
                .Select(j => j.Id == job.Id ? job : j)
                .ToList();

            updatedState = state with
            {
                CompletedJobs = newCompletedJobs,
                FailedJobs = newFailedJobs,
                RunningJobs = newRunningJobs,
                BestAD = newBestAD,
                AverageAD = newAverageAD,
                GoodJobCount = newGoodJobCount,
                Jobs = updatedJobs
            };
            context.State = updatedState;

            // Track completed jobs for auto-save
            context.JobsCompletedSinceLastSave++;
            shouldAutoSave = context.JobsCompletedSinceLastSave >= context.Options.AutoSaveInterval;
            if (shouldAutoSave)
            {
                context.JobsCompletedSinceLastSave = 0;
            }

            var changeType = isSuccess
                ? ExecutionStateChangeType.JobCompleted
                : ExecutionStateChangeType.JobFailed;

            RaiseStateChanged(updatedState, changeType, job);
        }

        // Auto-save outside lock
        if (shouldAutoSave)
        {
            _ = SaveStateAsync(updatedState);
        }
    }

    private void RaiseStateChanged(PlanExecutionState state, ExecutionStateChangeType changeType, OptimizationJob? job)
    {
        StateChanged?.Invoke(this, new PlanExecutionStateChangedEventArgs
        {
            State = state,
            ChangeType = changeType,
            AffectedJob = job
        });
    }

    private async Task SaveStateAsync(PlanExecutionState state)
    {
        if (_stateRepository == null) return;

        try
        {
            await _stateRepository.SaveAsync(state);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save execution state for plan {PlanId}", state.PlanId);
        }
    }

    private async Task DeleteStateAsync(string planId)
    {
        if (_stateRepository == null) return;

        try
        {
            await _stateRepository.DeleteAsync(planId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete execution state for plan {PlanId}", planId);
        }
    }

    private async Task<OptimizationJob?> GetNextJobAsync(ExecutionContext context, CancellationToken cancellationToken)
    {
        // Use prioritizer if enabled
        if (context.State.UsePrioritization && _prioritizer != null)
        {
            var recommendation = _prioritizer.GetNextBestJob(context.State);
            if (recommendation != null)
            {
                // Remove from queue (mark as running)
                var job = await _jobQueue.DequeueByIdAsync(recommendation.Job.Id, context.State.PlanId, cancellationToken);
                if (job != null)
                {
                    _logger.LogInformation(
                        "Prioritized job {Symbol}/{Timeframe} with promise {Score:P0}: {Reasoning}",
                        job.Symbol,
                        job.Timeframe,
                        recommendation.Promise.Score,
                        recommendation.Reasoning);

                    // Track prioritization usage
                    lock (context.StateLock)
                    {
                        context.State = context.State with
                        {
                            PrioritizedJobsExecuted = context.State.PrioritizedJobsExecuted + 1
                        };
                    }

                    return job;
                }
            }
        }

        // Fall back to sequential dequeue
        return await _jobQueue.DequeueNextAsync(context.State.PlanId, cancellationToken);
    }

    private async Task CheckAndQueueFollowUpAsync(ExecutionContext context, OptimizationJob completedJob, CancellationToken cancellationToken)
    {
        if (!context.Options.AutoQueueFollowUps || _prioritizer == null)
        {
            return;
        }

        var suggestion = _prioritizer.ShouldQueueFollowUp(completedJob);
        if (suggestion == null || !suggestion.ShouldQueue)
        {
            return;
        }

        // Create follow-up job with higher resolution
        var followUpJob = completedJob with
        {
            Id = Guid.NewGuid().ToString("N"),
            Status = JobStatus.Pending,
            Resolution = completedJob.Resolution with
            {
                MaxBacktests = suggestion.SuggestedMaxBacktests
            },
            BestAD = null,
            Score = null,
            StartedAt = null,
            CompletedAt = null,
            ResultPath = null
        };

        _logger.LogInformation(
            "Queueing follow-up job for {Symbol}/{Timeframe}: {Reasoning}",
            followUpJob.Symbol,
            followUpJob.Timeframe,
            suggestion.Reasoning);

        // Enqueue the follow-up job
        await _jobQueue.EnqueueAsync(followUpJob, cancellationToken);

        // Update state
        lock (context.StateLock)
        {
            var updatedJobs = context.State.Jobs.Append(followUpJob).ToList();
            context.State = context.State with
            {
                TotalJobs = context.State.TotalJobs + 1,
                FollowUpJobsQueued = context.State.FollowUpJobsQueued + 1,
                Jobs = updatedJobs
            };
        }
    }

    private class ExecutionContext
    {
        public PlanExecutionState State { get; set; }
        public PlanExecutionOptions Options { get; }
        public CancellationTokenSource CancellationTokenSource { get; } = new();
        public Task? WorkerTask { get; set; }
        public bool IsPaused { get; set; }
        public ManualResetEventSlim PauseEvent { get; } = new(true);
        public object StateLock { get; } = new();
        public int JobsCompletedSinceLastSave { get; set; }

        public ExecutionContext(PlanExecutionState state, PlanExecutionOptions options)
        {
            State = state;
            Options = options;
        }
    }
}
