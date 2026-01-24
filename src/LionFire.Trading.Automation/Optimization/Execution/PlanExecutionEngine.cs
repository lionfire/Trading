using System.Collections.Concurrent;
using System.Threading;
using LionFire.Trading.Optimization.Execution;
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
    private readonly JobMatrixGenerator _matrixGenerator;
    private readonly ILogger<PlanExecutionEngine> _logger;

    private readonly ConcurrentDictionary<string, ExecutionContext> _activeExecutions = new();

    public event EventHandler<PlanExecutionStateChangedEventArgs>? StateChanged;

    public PlanExecutionEngine(
        IOptimizationPlanRepository planRepository,
        IJobQueueService jobQueue,
        IJobRunner jobRunner,
        ISymbolCollectionRepository? symbolRepository,
        IPlanExecutionStateRepository? stateRepository,
        ILogger<PlanExecutionEngine> logger)
    {
        _planRepository = planRepository;
        _jobQueue = jobQueue;
        _jobRunner = jobRunner;
        _stateRepository = stateRepository;
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
            if (existingContext.State.Status == PlanExecutionStatus.Paused && options.ResumeIfPaused)
            {
                await ResumeAsync(planId, cancellationToken);
                return existingContext.State;
            }

            throw new InvalidOperationException($"Plan {planId} is already executing");
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

        // Generate job matrix
        var jobs = await _matrixGenerator.GenerateAsync(plan, cancellationToken: cancellationToken);

        _logger.LogInformation("Generated {JobCount} jobs for plan {PlanId}", jobs.Count, planId);

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
            Jobs = jobs.ToList()
        };

        var context = new ExecutionContext(state, options);
        if (!_activeExecutions.TryAdd(planId, context))
        {
            throw new InvalidOperationException($"Failed to register execution for plan {planId}");
        }

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

    public Task<PlanExecutionState?> GetStatusAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (_activeExecutions.TryGetValue(planId, out var context))
        {
            return Task.FromResult<PlanExecutionState?>(context.State);
        }
        return Task.FromResult<PlanExecutionState?>(null);
    }

    public Task<IReadOnlyList<PlanExecutionState>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var states = _activeExecutions.Values.Select(c => c.State).ToList();
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

                // Try to get next job
                var job = await _jobQueue.DequeueNextAsync(context.State.PlanId, cancellationToken);
                if (job == null)
                {
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

                // Acquire semaphore
                await semaphore.WaitAsync(cancellationToken);

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

            // Delete persisted state (execution complete, no resume needed)
            await DeleteStateAsync(context.State.PlanId);

            RaiseStateChanged(finalState, ExecutionStateChangeType.Completed, null);

            // Remove from active executions
            _activeExecutions.TryRemove(context.State.PlanId, out _);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Plan {PlanId} execution was cancelled", context.State.PlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Plan {PlanId} execution failed with error", context.State.PlanId);
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
