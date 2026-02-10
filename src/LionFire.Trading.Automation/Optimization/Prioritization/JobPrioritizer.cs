using LionFire.Trading.Optimization.Execution;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Prioritization;

/// <summary>
/// Ranks pending jobs and provides next-best-job recommendations.
/// </summary>
public class JobPrioritizer : IJobPrioritizer
{
    private readonly PromiseScoreCalculator _calculator;
    private readonly ILogger<JobPrioritizer>? _logger;

    public JobPrioritizer(PromiseScoreCalculator calculator, ILogger<JobPrioritizer>? logger = null)
    {
        _calculator = calculator;
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<RankedJob> RankPendingJobs(PlanExecutionState state, PrioritizationConfig? config = null)
    {
        config ??= new PrioritizationConfig();

        var pendingJobs = state.Jobs.Where(j => j.Status == JobStatus.Pending).ToList();
        if (pendingJobs.Count == 0)
        {
            return [];
        }

        var completedJobs = state.Jobs.Where(j => j.Status == JobStatus.Completed).ToList();

        var rankedJobs = pendingJobs
            .Select(job =>
            {
                var promise = _calculator.CalculatePromise(job, completedJobs, config.Weights);
                return new RankedJob(job, promise);
            })
            .OrderBy(r => r.Job.Priority) // Respect cell priority first (1=highest)
            .ThenByDescending(r => r.Promise.Score) // Within same priority, use promise score
            .ThenByDescending(r => r.Promise.Confidence)
            .ToList();

        _logger?.LogDebug(
            "Ranked {Count} pending jobs. Top: {Symbol}/{Timeframe} with score {Score:P0}",
            rankedJobs.Count,
            rankedJobs.FirstOrDefault()?.Job.Symbol ?? "none",
            rankedJobs.FirstOrDefault()?.Job.Timeframe ?? "none",
            rankedJobs.FirstOrDefault()?.Promise.Score ?? 0);

        return rankedJobs;
    }

    /// <inheritdoc />
    public NextJobRecommendation? GetNextBestJob(PlanExecutionState state, PrioritizationConfig? config = null)
    {
        var rankedJobs = RankPendingJobs(state, config);
        if (rankedJobs.Count == 0)
        {
            _logger?.LogDebug("No pending jobs available");
            return null;
        }

        var best = rankedJobs[0];

        // Build enhanced reasoning
        var reasoning = BuildRecommendationReasoning(best, rankedJobs.Count);

        _logger?.LogInformation(
            "Recommending {Symbol}/{Timeframe} with promise score {Score:P0} (confidence: {Confidence:P0}): {Reasoning}",
            best.Job.Symbol,
            best.Job.Timeframe,
            best.Promise.Score,
            best.Promise.Confidence,
            reasoning);

        return new NextJobRecommendation(best.Job, best.Promise, reasoning);
    }

    /// <inheritdoc />
    public FollowUpSuggestion? ShouldQueueFollowUp(OptimizationJob completedJob, PrioritizationConfig? config = null)
    {
        config ??= new PrioritizationConfig();
        var followUpConfig = config.FollowUp;

        // Check if job has a valid AD score
        if (!completedJob.BestAD.HasValue)
        {
            return new FollowUpSuggestion(false, 0, "No AD score available");
        }

        var ad = completedJob.BestAD.Value;
        var currentMaxBacktests = completedJob.Resolution.MaxBacktests;

        // Determine current tier
        if (currentMaxBacktests <= followUpConfig.CoarseMaxBacktests)
        {
            // Coarse tier - check if should promote to medium
            if (ad >= followUpConfig.CoarseToMediumAdThreshold)
            {
                var reasoning = $"Coarse run ({currentMaxBacktests} backtests) achieved AD {ad:F2} >= {followUpConfig.CoarseToMediumAdThreshold} threshold";
                _logger?.LogInformation(
                    "Suggesting follow-up for {Symbol}/{Timeframe}: {Reasoning}",
                    completedJob.Symbol, completedJob.Timeframe, reasoning);

                return new FollowUpSuggestion(true, followUpConfig.MediumMaxBacktests, reasoning);
            }
        }
        else if (currentMaxBacktests <= followUpConfig.MediumMaxBacktests)
        {
            // Medium tier - check if should promote to full
            if (ad >= followUpConfig.MediumToFullAdThreshold)
            {
                var reasoning = $"Medium run ({currentMaxBacktests} backtests) achieved AD {ad:F2} >= {followUpConfig.MediumToFullAdThreshold} threshold";
                _logger?.LogInformation(
                    "Suggesting follow-up for {Symbol}/{Timeframe}: {Reasoning}",
                    completedJob.Symbol, completedJob.Timeframe, reasoning);

                return new FollowUpSuggestion(true, followUpConfig.FullMaxBacktests, reasoning);
            }
        }
        // Full tier or above - no further follow-up

        return new FollowUpSuggestion(false, 0, $"AD {ad:F2} below threshold or already at full resolution");
    }

    private string BuildRecommendationReasoning(RankedJob best, int totalPending)
    {
        var parts = new List<string>();

        // Add score context
        if (best.Promise.Score >= 0.7)
        {
            parts.Add("highly promising based on related job performance");
        }
        else if (best.Promise.Score >= 0.5)
        {
            parts.Add("moderately promising");
        }
        else if (best.Promise.Score >= 0.3)
        {
            parts.Add("somewhat promising");
        }
        else
        {
            parts.Add("lower promise but still the best option");
        }

        // Add confidence context
        if (best.Promise.Confidence < 0.3)
        {
            parts.Add("low confidence due to limited related data");
        }
        else if (best.Promise.Confidence >= 0.7)
        {
            parts.Add("high confidence based on abundant data");
        }

        // Add factor highlights
        var topFactor = best.Promise.Factors.OrderByDescending(f => f.Contribution).FirstOrDefault();
        if (topFactor != null && topFactor.Value >= 0.6)
        {
            parts.Add($"strong {topFactor.Name.ToLowerInvariant()}");
        }

        // Add queue context
        if (totalPending > 1)
        {
            parts.Add($"selected from {totalPending} pending jobs");
        }

        return string.Join("; ", parts);
    }
}
