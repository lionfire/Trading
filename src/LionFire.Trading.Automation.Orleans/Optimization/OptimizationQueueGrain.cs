using System.Text.Json;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using LionFire.Trading.Optimization;
using LionFire.Trading.Optimization.Queue;
using LionFire.Trading.Grains.Optimization;

namespace LionFire.Trading.Automation.Orleans.Optimization;

/// <summary>
/// Orleans grain implementation for managing the global optimization job queue
/// </summary>
public class OptimizationQueueGrain : Grain, IOptimizationQueueGrain
{
    private const string QueueStateKey = "queue-state";
    private const int DefaultCleanupIntervalHours = 24;
    private const int DefaultJobTimeoutMinutes = 30;
    private const int DefaultCompletedJobRetentionDays = 7;

    private readonly IPersistentState<OptimizationQueueState> _state;
    private readonly ILogger<OptimizationQueueGrain> _logger;

    public OptimizationQueueGrain(
        [PersistentState(QueueStateKey)] IPersistentState<OptimizationQueueState> state,
        ILogger<OptimizationQueueGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OptimizationQueueGrain activated");
        
        // Clean up stale jobs on activation
        RegisterTimer(_ => CleanupAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(DefaultCleanupIntervalHours));
        
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<OptimizationQueueItem> EnqueueJobAsync(string parametersJson, int priority = 5, string? submittedBy = null)
    {
        var job = new OptimizationQueueItem
        {
            JobId = Guid.NewGuid(),
            Priority = priority,
            ParametersJson = parametersJson,
            SubmittedBy = submittedBy,
            CreatedTime = DateTimeOffset.UtcNow,
            LastUpdated = DateTimeOffset.UtcNow
        };

        _state.State.Jobs[job.JobId] = job;
        
        // Insert job into queue in priority order (lower priority number = higher priority)
        var insertIndex = _state.State.QueueOrder.Count;
        for (int i = 0; i < _state.State.QueueOrder.Count; i++)
        {
            var existingJobId = _state.State.QueueOrder[i];
            if (_state.State.Jobs.TryGetValue(existingJobId, out var existingJob))
            {
                if (existingJob.Priority > priority || 
                    (existingJob.Priority == priority && existingJob.CreatedTime > job.CreatedTime))
                {
                    insertIndex = i;
                    break;
                }
            }
        }
        
        _state.State.QueueOrder.Insert(insertIndex, job.JobId);
        
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Job {JobId} enqueued with priority {Priority} at position {Position}", 
            job.JobId, priority, insertIndex + 1);
            
        return job;
    }

    public async Task<OptimizationQueueItem?> DequeueJobAsync(string siloId, int maxConcurrentJobs = 1)
    {
        // Check if this silo already has max concurrent jobs
        var currentSiloJobs = _state.State.RunningSiloJobs.Values.Count(jobId => 
            _state.State.Jobs.TryGetValue(jobId, out var job) && job.AssignedSiloId == siloId);
            
        if (currentSiloJobs >= maxConcurrentJobs)
        {
            return null;
        }

        // Find next available job in queue
        OptimizationQueueItem? nextJob = null;
        for (int i = 0; i < _state.State.QueueOrder.Count; i++)
        {
            var jobId = _state.State.QueueOrder[i];
            if (_state.State.Jobs.TryGetValue(jobId, out var job) && job.Status == OptimizationJobStatus.Queued)
            {
                nextJob = job;
                _state.State.QueueOrder.RemoveAt(i);
                break;
            }
        }

        if (nextJob == null)
        {
            return null;
        }

        // Assign job to silo
        nextJob.Status = OptimizationJobStatus.Running;
        nextJob.AssignedSiloId = siloId;
        nextJob.StartedTime = DateTimeOffset.UtcNow;
        nextJob.LastUpdated = DateTimeOffset.UtcNow;
        
        _state.State.RunningSiloJobs[siloId] = nextJob.JobId;
        
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Job {JobId} dequeued and assigned to silo {SiloId}", 
            nextJob.JobId, siloId);
            
        return nextJob;
    }

    public async Task<bool> UpdateJobProgressAsync(Guid jobId, OptimizationProgress progress)
    {
        if (!_state.State.Jobs.TryGetValue(jobId, out var job))
        {
            return false;
        }

        if (job.Status != OptimizationJobStatus.Running)
        {
            return false;
        }

        job.Progress = progress;
        job.LastUpdated = DateTimeOffset.UtcNow;
        
        await _state.WriteStateAsync();
        
        return true;
    }

    public async Task<bool> CompleteJobAsync(Guid jobId, string? resultPath = null)
    {
        if (!_state.State.Jobs.TryGetValue(jobId, out var job))
        {
            return false;
        }

        if (job.Status != OptimizationJobStatus.Running)
        {
            return false;
        }

        job.Status = OptimizationJobStatus.Completed;
        job.CompletedTime = DateTimeOffset.UtcNow;
        job.LastUpdated = DateTimeOffset.UtcNow;
        job.ResultPath = resultPath;
        
        // Remove from running silo jobs
        if (job.AssignedSiloId != null)
        {
            _state.State.RunningSiloJobs.Remove(job.AssignedSiloId);
        }
        
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Job {JobId} completed successfully", jobId);
        
        return true;
    }

    public async Task<bool> FailJobAsync(Guid jobId, string errorMessage)
    {
        if (!_state.State.Jobs.TryGetValue(jobId, out var job))
        {
            return false;
        }

        job.RetryCount++;
        job.ErrorMessage = errorMessage;
        job.LastUpdated = DateTimeOffset.UtcNow;

        if (job.RetryCount >= job.MaxRetries)
        {
            job.Status = OptimizationJobStatus.Failed;
            job.CompletedTime = DateTimeOffset.UtcNow;
            
            // Remove from running silo jobs
            if (job.AssignedSiloId != null)
            {
                _state.State.RunningSiloJobs.Remove(job.AssignedSiloId);
            }
            
            _logger.LogWarning("Job {JobId} failed permanently after {RetryCount} retries: {ErrorMessage}", 
                jobId, job.RetryCount, errorMessage);
        }
        else
        {
            // Reset for retry
            job.Status = OptimizationJobStatus.Queued;
            job.AssignedSiloId = null;
            job.StartedTime = null;
            
            // Re-add to queue with lower priority (higher number)
            job.Priority = Math.Min(job.Priority + 1, 10);
            _state.State.QueueOrder.Add(jobId);
            
            // Remove from running silo jobs
            if (job.AssignedSiloId != null)
            {
                _state.State.RunningSiloJobs.Remove(job.AssignedSiloId);
            }
            
            _logger.LogWarning("Job {JobId} failed, retry {RetryCount}/{MaxRetries}: {ErrorMessage}", 
                jobId, job.RetryCount, job.MaxRetries, errorMessage);
        }
        
        await _state.WriteStateAsync();
        
        return true;
    }

    public async Task<bool> CancelJobAsync(Guid jobId)
    {
        if (!_state.State.Jobs.TryGetValue(jobId, out var job))
        {
            return false;
        }

        if (job.Status == OptimizationJobStatus.Completed || 
            job.Status == OptimizationJobStatus.Failed || 
            job.Status == OptimizationJobStatus.Cancelled)
        {
            return false;
        }

        job.Status = OptimizationJobStatus.Cancelled;
        job.CompletedTime = DateTimeOffset.UtcNow;
        job.LastUpdated = DateTimeOffset.UtcNow;
        
        // Remove from queue if queued
        _state.State.QueueOrder.Remove(jobId);
        
        // Remove from running silo jobs if running
        if (job.AssignedSiloId != null)
        {
            _state.State.RunningSiloJobs.Remove(job.AssignedSiloId);
        }
        
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Job {JobId} cancelled", jobId);
        
        return true;
    }

    public Task<OptimizationQueueItem?> GetJobAsync(Guid jobId)
    {
        _state.State.Jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<OptimizationQueueItem>> GetJobsAsync(OptimizationJobStatus? status = null, int limit = 100)
    {
        var jobs = _state.State.Jobs.Values.AsEnumerable();
        
        if (status.HasValue)
        {
            jobs = jobs.Where(j => j.Status == status.Value);
        }
        
        var result = jobs
            .OrderByDescending(j => j.CreatedTime)
            .Take(limit)
            .ToList();
            
        return Task.FromResult<IReadOnlyList<OptimizationQueueItem>>(result);
    }

    public Task<OptimizationQueueStatus> GetQueueStatusAsync()
    {
        var jobs = _state.State.Jobs.Values;
        var completedJobs = jobs.Where(j => j.Status == OptimizationJobStatus.Completed && j.Duration.HasValue);
        
        var status = new OptimizationQueueStatus
        {
            QueuedCount = jobs.Count(j => j.Status == OptimizationJobStatus.Queued),
            RunningCount = jobs.Count(j => j.Status == OptimizationJobStatus.Running),
            CompletedCount = jobs.Count(j => j.Status == OptimizationJobStatus.Completed),
            FailedCount = jobs.Count(j => j.Status == OptimizationJobStatus.Failed),
            CancelledCount = jobs.Count(j => j.Status == OptimizationJobStatus.Cancelled),
            TotalJobs = jobs.Count(),
            ActiveSilos = _state.State.RunningSiloJobs.Keys.Distinct().Count(),
            AverageJobDuration = completedJobs.Any() ? 
                TimeSpan.FromTicks((long)completedJobs.Average(j => j.Duration!.Value.Ticks)) : 
                null
        };
        
        // Estimate next job delay based on current running jobs
        var runningJobs = jobs.Where(j => j.Status == OptimizationJobStatus.Running).ToList();
        if (runningJobs.Any() && status.QueuedCount > 0)
        {
            var earliestCompletion = runningJobs
                .Select(j => j.EstimatedCompletionTime)
                .Where(t => t.HasValue)
                .OrderBy(t => t!.Value)
                .FirstOrDefault();
                
            if (earliestCompletion.HasValue)
            {
                status.EstimatedNextJobDelay = earliestCompletion.Value - DateTimeOffset.UtcNow;
                if (status.EstimatedNextJobDelay < TimeSpan.Zero)
                {
                    status.EstimatedNextJobDelay = TimeSpan.Zero;
                }
            }
        }
        
        return Task.FromResult(status);
    }

    public async Task<bool> HeartbeatAsync(Guid jobId, string siloId)
    {
        if (!_state.State.Jobs.TryGetValue(jobId, out var job))
        {
            return false;
        }

        if (job.Status != OptimizationJobStatus.Running || job.AssignedSiloId != siloId)
        {
            return false;
        }

        job.LastUpdated = DateTimeOffset.UtcNow;
        await _state.WriteStateAsync();
        
        return true;
    }

    public async Task<int> CleanupAsync(int completedJobRetentionDays = DefaultCompletedJobRetentionDays, int jobTimeoutMinutes = DefaultJobTimeoutMinutes)
    {
        var cleanupCount = 0;
        var cutoffTime = DateTimeOffset.UtcNow.AddDays(-completedJobRetentionDays);
        var timeoutCutoff = DateTimeOffset.UtcNow.AddMinutes(-jobTimeoutMinutes);
        
        // Clean up old completed/failed/cancelled jobs
        var jobsToRemove = _state.State.Jobs.Values
            .Where(j => (j.Status == OptimizationJobStatus.Completed || 
                        j.Status == OptimizationJobStatus.Failed || 
                        j.Status == OptimizationJobStatus.Cancelled) &&
                       j.CompletedTime < cutoffTime)
            .Select(j => j.JobId)
            .ToList();
            
        foreach (var jobId in jobsToRemove)
        {
            _state.State.Jobs.Remove(jobId);
            cleanupCount++;
        }
        
        // Handle stale running jobs (no heartbeat for timeout period)
        var staleJobs = _state.State.Jobs.Values
            .Where(j => j.Status == OptimizationJobStatus.Running && 
                       j.LastUpdated < timeoutCutoff)
            .ToList();
            
        foreach (var staleJob in staleJobs)
        {
            _logger.LogWarning("Job {JobId} appears stale (no heartbeat since {LastUpdated}), resetting to queued", 
                staleJob.JobId, staleJob.LastUpdated);
                
            staleJob.Status = OptimizationJobStatus.Queued;
            staleJob.AssignedSiloId = null;
            staleJob.StartedTime = null;
            staleJob.Priority = Math.Min(staleJob.Priority + 2, 10); // Lower priority for retry
            
            // Re-add to queue
            _state.State.QueueOrder.Add(staleJob.JobId);
            
            // Remove from running silo jobs
            if (staleJob.AssignedSiloId != null)
            {
                _state.State.RunningSiloJobs.Remove(staleJob.AssignedSiloId);
            }
            
            cleanupCount++;
        }
        
        if (cleanupCount > 0)
        {
            _state.State.LastCleanup = DateTimeOffset.UtcNow;
            await _state.WriteStateAsync();
            
            _logger.LogInformation("Cleanup completed: removed {CleanupCount} jobs", cleanupCount);
        }
        
        return cleanupCount;
    }
}