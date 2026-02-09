using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Execution;
using LionFire.Trading.Optimization.Queue;
using Microsoft.Extensions.Logging;
using Orleans;

namespace LionFire.Trading.Automation.Orleans.Optimization;

/// <summary>
/// Distributed job runner that submits optimization jobs to the Orleans
/// grain queue for execution by remote workers.
/// </summary>
public class DistributedJobRunner : IJobRunner
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<DistributedJobRunner> _logger;

    private const string CoordinatorGrainKey = "global";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    public DistributedJobRunner(
        IGrainFactory grainFactory,
        ILogger<DistributedJobRunner> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    /// <summary>
    /// Submit a job to the distributed queue and poll for completion.
    /// </summary>
    public async Task<OptimizationJob> RunAsync(
        OptimizationJob job,
        IProgress<JobProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var queueGrain = _grainFactory.GetGrain<IOptimizationQueueGrain>(CoordinatorGrainKey);

        _logger.LogInformation(
            "Submitting job {JobId} to distributed queue: {Symbol} {Timeframe} {DateRange}",
            job.Id, job.Symbol, job.Timeframe, job.DateRange.Name);

        // Convert and submit to grain queue
        var parametersJson = OptimizationJobConverter.ToParametersJson(job);
        var priority = OptimizationJobConverter.PromiseScoreToPriority(job.Score);
        var submittedBy = OptimizationJobConverter.BuildSubmittedBy(job);

        var queueItem = await queueGrain.EnqueueJobAsync(parametersJson, priority, submittedBy);

        _logger.LogDebug(
            "Job {JobId} queued as grain job {GrainJobId} with priority {Priority}",
            job.Id, queueItem.JobId, priority);

        progress?.Report(new JobProgress
        {
            JobId = job.Id,
            Status = JobStatus.Running,
            Message = $"Queued for distributed execution (queue ID: {queueItem.JobId:N})"
        });

        // Poll for completion
        var runningJob = job with
        {
            Status = JobStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(PollInterval, cancellationToken);

            OptimizationQueueItem? status;
            try
            {
                status = await queueGrain.GetJobAsync(queueItem.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error polling status for job {JobId}", job.Id);
                continue;
            }

            if (status == null)
            {
                _logger.LogWarning("Queue item {GrainJobId} not found for job {JobId}", queueItem.JobId, job.Id);
                return runningJob with
                {
                    Status = JobStatus.Failed,
                    CompletedAt = DateTimeOffset.UtcNow,
                    Error = "Queue item disappeared"
                };
            }

            switch (status.Status)
            {
                case OptimizationJobStatus.Completed:
                    _logger.LogInformation(
                        "Distributed job {JobId} completed. ResultPath={ResultPath}",
                        job.Id, status.ResultPath);

                    // The remote worker should have stored results.
                    // Try to read back OptimizationJob with results from the queue item.
                    var completedJob = TryReadCompletedJob(status) ?? OptimizationJobConverter.ApplyQueueItemCompletion(runningJob, status);

                    progress?.Report(new JobProgress
                    {
                        JobId = job.Id,
                        Status = JobStatus.Completed,
                        PercentComplete = 1.0,
                        Message = $"Completed on worker {status.AssignedSiloId}"
                    });

                    return completedJob;

                case OptimizationJobStatus.Failed:
                    _logger.LogWarning(
                        "Distributed job {JobId} failed: {Error}",
                        job.Id, status.ErrorMessage);

                    progress?.Report(new JobProgress
                    {
                        JobId = job.Id,
                        Status = JobStatus.Failed,
                        Message = $"Failed: {status.ErrorMessage}"
                    });

                    return runningJob with
                    {
                        Status = JobStatus.Failed,
                        CompletedAt = status.CompletedTime ?? DateTimeOffset.UtcNow,
                        Error = status.ErrorMessage
                    };

                case OptimizationJobStatus.Cancelled:
                    _logger.LogInformation("Distributed job {JobId} was cancelled", job.Id);

                    progress?.Report(new JobProgress
                    {
                        JobId = job.Id,
                        Status = JobStatus.Cancelled,
                        Message = "Cancelled"
                    });

                    return runningJob with
                    {
                        Status = JobStatus.Cancelled,
                        CompletedAt = status.CompletedTime ?? DateTimeOffset.UtcNow,
                        Error = "Cancelled"
                    };

                case OptimizationJobStatus.Running:
                    if (status.Progress != null)
                    {
                        progress?.Report(new JobProgress
                        {
                            JobId = job.Id,
                            Status = JobStatus.Running,
                            PercentComplete = status.Progress.Percent / 100.0,
                            BacktestsCompleted = (int)status.Progress.Completed,
                            TotalBacktests = (int)status.Progress.Queued,
                            Message = $"Running on {status.AssignedSiloId}: {status.Progress.Percent:F0}%"
                        });
                    }
                    break;

                case OptimizationJobStatus.Queued:
                    // Still waiting for a worker to pick it up
                    break;
            }
        }

        // Cancelled externally - try to cancel the queue item too
        try
        {
            await queueGrain.CancelJobAsync(queueItem.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cancel queue item {GrainJobId} for job {JobId}", queueItem.JobId, job.Id);
        }

        return runningJob with
        {
            Status = JobStatus.Cancelled,
            CompletedAt = DateTimeOffset.UtcNow,
            Error = "Cancelled"
        };
    }

    /// <summary>
    /// Try to read a fully populated OptimizationJob from the queue item's result.
    /// Workers that understand the OptimizationJob format will store the completed
    /// job back as ParametersJson with results populated.
    /// </summary>
    private OptimizationJob? TryReadCompletedJob(OptimizationQueueItem item)
    {
        if (string.IsNullOrEmpty(item.ParametersJson)) return null;

        try
        {
            var job = OptimizationJobConverter.FromParametersJson(item.ParametersJson);
            if (job?.Status == JobStatus.Completed)
            {
                return job;
            }
        }
        catch
        {
            // Not an OptimizationJob JSON - that's fine, older workers use PMultiSim format
        }

        return null;
    }
}
