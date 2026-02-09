using LionFire.Trading.Optimization.Execution;

namespace LionFire.Trading.Automation.Optimization.Prioritization;

/// <summary>
/// Service that ranks pending jobs and provides next-best-job recommendations.
/// </summary>
public interface IJobPrioritizer
{
    /// <summary>
    /// Rank all pending jobs by their promise scores.
    /// </summary>
    /// <param name="state">Current execution state with all jobs.</param>
    /// <param name="config">Optional configuration. Uses defaults if null.</param>
    /// <returns>List of pending jobs sorted by promise score (highest first).</returns>
    IReadOnlyList<RankedJob> RankPendingJobs(PlanExecutionState state, PrioritizationConfig? config = null);

    /// <summary>
    /// Get the single best job to execute next.
    /// </summary>
    /// <param name="state">Current execution state with all jobs.</param>
    /// <param name="config">Optional configuration. Uses defaults if null.</param>
    /// <returns>The recommended next job, or null if no pending jobs.</returns>
    NextJobRecommendation? GetNextBestJob(PlanExecutionState state, PrioritizationConfig? config = null);

    /// <summary>
    /// Determine if a follow-up job at higher resolution should be queued.
    /// </summary>
    /// <param name="completedJob">The job that just completed.</param>
    /// <param name="config">Optional configuration. Uses defaults if null.</param>
    /// <returns>Suggestion for follow-up queuing, or null if not recommended.</returns>
    FollowUpSuggestion? ShouldQueueFollowUp(OptimizationJob completedJob, PrioritizationConfig? config = null);
}

/// <summary>
/// A pending job with its calculated promise score.
/// </summary>
public record RankedJob(
    /// <summary>The optimization job.</summary>
    OptimizationJob Job,
    /// <summary>The calculated promise score.</summary>
    PromiseScore Promise
);

/// <summary>
/// Recommendation for the next job to execute.
/// </summary>
public record NextJobRecommendation(
    /// <summary>The recommended job.</summary>
    OptimizationJob Job,
    /// <summary>The calculated promise score.</summary>
    PromiseScore Promise,
    /// <summary>Human-readable reasoning for this recommendation.</summary>
    string Reasoning
);

/// <summary>
/// Suggestion for queuing a follow-up job at higher resolution.
/// </summary>
public record FollowUpSuggestion(
    /// <summary>Whether a follow-up should be queued.</summary>
    bool ShouldQueue,
    /// <summary>Suggested maxBacktests for the follow-up job.</summary>
    int SuggestedMaxBacktests,
    /// <summary>Human-readable reasoning.</summary>
    string Reasoning
);
