using System.Text.Json;
using LionFire.Trading.Optimization.Execution;
using LionFire.Trading.Optimization.Queue;

namespace LionFire.Trading.Automation.Orleans.Optimization;

/// <summary>
/// Converts between OptimizationJob (plan execution system) and
/// OptimizationQueueItem (Orleans grain queue system).
/// </summary>
public static class OptimizationJobConverter
{
    private const string JobIdKey = "optimizationJobId";
    private const string PlanIdKey = "planId";

    /// <summary>
    /// Convert an OptimizationJob to parameters JSON for the queue item.
    /// Stores the full OptimizationJob as JSON so workers can reconstruct it.
    /// </summary>
    public static string ToParametersJson(OptimizationJob job)
        => JsonSerializer.Serialize(job, JsonOptions);

    /// <summary>
    /// Reconstruct an OptimizationJob from a queue item's parameters JSON.
    /// </summary>
    public static OptimizationJob? FromParametersJson(string parametersJson)
        => JsonSerializer.Deserialize<OptimizationJob>(parametersJson, JsonOptions);

    /// <summary>
    /// Map OptimizationJob promise score (0.0-1.0) to queue priority (1-9).
    /// Lower priority number = higher priority.
    /// </summary>
    public static int PromiseScoreToPriority(double? promiseScore)
    {
        if (!promiseScore.HasValue) return 5; // default normal priority

        // Map 0.0-1.0 score to 1-9 priority (inverted: high score = low priority number)
        var clamped = Math.Clamp(promiseScore.Value, 0.0, 1.0);
        return Math.Max(1, (int)(9.0 - clamped * 8.0));
    }

    /// <summary>
    /// Build a submittedBy string from the job context.
    /// </summary>
    public static string BuildSubmittedBy(OptimizationJob job)
        => $"plan:{job.PlanId}";

    /// <summary>
    /// Apply completion data from a queue item back onto an OptimizationJob.
    /// </summary>
    public static OptimizationJob ApplyQueueItemCompletion(OptimizationJob originalJob, OptimizationQueueItem queueItem)
    {
        return queueItem.Status switch
        {
            OptimizationJobStatus.Completed => originalJob with
            {
                Status = JobStatus.Completed,
                StartedAt = queueItem.StartedTime,
                CompletedAt = queueItem.CompletedTime,
                ResultPath = queueItem.ResultPath,
            },
            OptimizationJobStatus.Failed => originalJob with
            {
                Status = JobStatus.Failed,
                StartedAt = queueItem.StartedTime,
                CompletedAt = queueItem.CompletedTime,
                Error = queueItem.ErrorMessage,
            },
            OptimizationJobStatus.Cancelled => originalJob with
            {
                Status = JobStatus.Cancelled,
                StartedAt = queueItem.StartedTime,
                CompletedAt = queueItem.CompletedTime,
                Error = "Cancelled",
            },
            _ => originalJob,
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };
}
