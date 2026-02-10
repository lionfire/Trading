using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// In-memory job queue with thread-safe operations.
/// </summary>
public class JobQueueService : IJobQueueService
{
    private readonly ConcurrentDictionary<string, OptimizationJob> _jobs = new();
    private readonly object _dequeueLock = new();
    private readonly ILogger<JobQueueService> _logger;
    private readonly JobOrderingHelper _orderingHelper;

    public JobQueueService(ILogger<JobQueueService> logger, JobOrderingHelper orderingHelper)
    {
        _logger = logger;
        _orderingHelper = orderingHelper;
    }

    public event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;

    public Task EnqueueAsync(OptimizationJob job, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_jobs.TryAdd(job.Id, job))
        {
            throw new InvalidOperationException($"Job with ID '{job.Id}' already exists in queue");
        }

        RaiseJobStatusChanged(job, JobStatus.Pending, job.Status);
        return Task.CompletedTask;
    }

    public Task EnqueueBatchAsync(IEnumerable<OptimizationJob> jobs, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var job in jobs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_jobs.TryAdd(job.Id, job))
            {
                throw new InvalidOperationException($"Job with ID '{job.Id}' already exists in queue");
            }
        }

        return Task.CompletedTask;
    }

    public Task<OptimizationJob?> DequeueNextAsync(string? planId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_dequeueLock)
        {
            // Find highest-priority pending job (optionally filtered by plan)
            // Tiebreakers: coarser timeframes first, higher-volume symbols first, then alphabetical
            var pendingJob = _jobs.Values
                .Where(j => j.Status == JobStatus.Pending)
                .Where(j => planId == null || j.PlanId == planId)
                .OrderBy(j => j.Priority)
                .ThenBy(j => JobOrderingHelper.GetTimeframeSortKey(j.Timeframe))
                .ThenBy(j => _orderingHelper.GetSymbolSortKey(j.Symbol))
                .ThenBy(j => j.Symbol, StringComparer.Ordinal)
                .FirstOrDefault();

            if (pendingJob == null)
            {
                return Task.FromResult<OptimizationJob?>(null);
            }

            // Update to running
            var runningJob = pendingJob with
            {
                Status = JobStatus.Running,
                StartedAt = DateTimeOffset.UtcNow
            };

            if (_jobs.TryUpdate(pendingJob.Id, runningJob, pendingJob))
            {
                var pendingCount = _jobs.Values.Count(j => j.Status == JobStatus.Pending && (planId == null || j.PlanId == planId));
                _logger.LogInformation("Dequeued job P{Priority}: {Symbol} {Timeframe} ({PendingRemaining} pending remaining)",
                    runningJob.Priority, runningJob.Symbol, runningJob.Timeframe, pendingCount);
                RaiseJobStatusChanged(runningJob, JobStatus.Pending, JobStatus.Running);
                return Task.FromResult<OptimizationJob?>(runningJob);
            }

            // Race condition - another thread updated it first
            return Task.FromResult<OptimizationJob?>(null);
        }
    }

    public Task<OptimizationJob?> DequeueByIdAsync(string jobId, string? planId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_dequeueLock)
        {
            if (!_jobs.TryGetValue(jobId, out var pendingJob))
            {
                return Task.FromResult<OptimizationJob?>(null);
            }

            // Verify job is pending and matches plan filter
            if (pendingJob.Status != JobStatus.Pending)
            {
                return Task.FromResult<OptimizationJob?>(null);
            }

            if (planId != null && pendingJob.PlanId != planId)
            {
                return Task.FromResult<OptimizationJob?>(null);
            }

            // Update to running
            var runningJob = pendingJob with
            {
                Status = JobStatus.Running,
                StartedAt = DateTimeOffset.UtcNow
            };

            if (_jobs.TryUpdate(pendingJob.Id, runningJob, pendingJob))
            {
                RaiseJobStatusChanged(runningJob, JobStatus.Pending, JobStatus.Running);
                return Task.FromResult<OptimizationJob?>(runningJob);
            }

            // Race condition - another thread updated it first
            return Task.FromResult<OptimizationJob?>(null);
        }
    }

    public Task<OptimizationJob?> GetByIdAsync(string jobId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<OptimizationJob>> GetByStatusAsync(JobStatus status, string? planId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jobs = _jobs.Values
            .Where(j => j.Status == status)
            .Where(j => planId == null || j.PlanId == planId)
            .ToList();

        return Task.FromResult<IReadOnlyList<OptimizationJob>>(jobs);
    }

    public Task<IReadOnlyList<OptimizationJob>> GetByPlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jobs = _jobs.Values
            .Where(j => j.PlanId == planId)
            .ToList();

        return Task.FromResult<IReadOnlyList<OptimizationJob>>(jobs);
    }

    public Task UpdateAsync(OptimizationJob job, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_jobs.TryGetValue(job.Id, out var existingJob))
        {
            throw new InvalidOperationException($"Job with ID '{job.Id}' not found in queue");
        }

        var oldStatus = existingJob.Status;

        if (_jobs.TryUpdate(job.Id, job, existingJob))
        {
            if (oldStatus != job.Status)
            {
                RaiseJobStatusChanged(job, oldStatus, job.Status);
            }
        }

        return Task.CompletedTask;
    }

    public Task<JobStatusCounts> GetCountsAsync(string? planId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jobs = planId == null
            ? _jobs.Values
            : _jobs.Values.Where(j => j.PlanId == planId);

        var counts = new JobStatusCounts
        {
            Pending = jobs.Count(j => j.Status == JobStatus.Pending),
            Running = jobs.Count(j => j.Status == JobStatus.Running),
            Completed = jobs.Count(j => j.Status == JobStatus.Completed),
            Failed = jobs.Count(j => j.Status == JobStatus.Failed),
            Cancelled = jobs.Count(j => j.Status == JobStatus.Cancelled)
        };

        return Task.FromResult(counts);
    }

    public Task ClearAsync(string planId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jobsToRemove = _jobs.Values
            .Where(j => j.PlanId == planId)
            .Select(j => j.Id)
            .ToList();

        foreach (var jobId in jobsToRemove)
        {
            _jobs.TryRemove(jobId, out _);
        }

        return Task.CompletedTask;
    }

    private void RaiseJobStatusChanged(OptimizationJob job, JobStatus oldStatus, JobStatus newStatus)
    {
        JobStatusChanged?.Invoke(this, new JobStatusChangedEventArgs
        {
            Job = job,
            OldStatus = oldStatus,
            NewStatus = newStatus
        });
    }
}
