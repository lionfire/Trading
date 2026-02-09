using LionFire.Trading.Optimization.Execution;

namespace LionFire.Trading.Automation.Optimization.Prioritization;

/// <summary>
/// Calculates promise scores for pending optimization jobs based on related completed job results.
/// </summary>
public class PromiseScoreCalculator
{
    /// <summary>
    /// AD thresholds for normalizing scores (aligned with HeatmapColorScale).
    /// </summary>
    private const double PoorAD = 0.5;
    private const double MarginalAD = 1.0;
    private const double GoodAD = 2.0;
    private const double ExcellentAD = 3.0;

    /// <summary>
    /// Calculate promise score for a pending job based on related completed jobs.
    /// </summary>
    /// <param name="pendingJob">The pending job to score.</param>
    /// <param name="completedJobs">All completed jobs to analyze for patterns.</param>
    /// <param name="weights">Optional custom weights. Uses defaults if null.</param>
    /// <returns>The calculated promise score with factors and reasoning.</returns>
    public PromiseScore CalculatePromise(
        OptimizationJob pendingJob,
        IReadOnlyList<OptimizationJob> completedJobs,
        PrioritizationWeights? weights = null)
    {
        weights ??= PrioritizationWeights.Default;

        if (completedJobs.Count == 0)
        {
            return PromiseScore.Neutral;
        }

        var factors = new List<PromiseFactor>();
        var reasoningParts = new List<string>();
        var totalDataPoints = 0;

        // Factor 1: Symbol Performance
        var symbolRelated = GetRelatedBySymbol(pendingJob.Symbol, completedJobs);
        if (symbolRelated.Any())
        {
            var symbolStats = CalculateAggregateStats(symbolRelated);
            var symbolValue = NormalizeAD(symbolStats.AverageAD);
            var symbolContribution = symbolValue * weights.SymbolPerformance;

            factors.Add(new PromiseFactor
            {
                Name = "Symbol Performance",
                Weight = weights.SymbolPerformance,
                Value = symbolValue,
                Contribution = symbolContribution,
                Description = $"{symbolRelated.Count} jobs on {pendingJob.Symbol}, avg AD: {symbolStats.AverageAD:F2}, best: {symbolStats.BestAD:F2}"
            });

            if (symbolStats.AverageAD >= GoodAD)
            {
                reasoningParts.Add($"{pendingJob.Symbol} shows strong performance (avg AD {symbolStats.AverageAD:F2})");
            }
            else if (symbolStats.AverageAD >= MarginalAD)
            {
                reasoningParts.Add($"{pendingJob.Symbol} has acceptable performance (avg AD {symbolStats.AverageAD:F2})");
            }
            else
            {
                reasoningParts.Add($"{pendingJob.Symbol} has weak performance (avg AD {symbolStats.AverageAD:F2})");
            }

            totalDataPoints += symbolRelated.Count;
        }
        else
        {
            factors.Add(new PromiseFactor
            {
                Name = "Symbol Performance",
                Weight = weights.SymbolPerformance,
                Value = 0.5, // Neutral
                Contribution = 0.5 * weights.SymbolPerformance,
                Description = $"No prior jobs for {pendingJob.Symbol}"
            });
        }

        // Factor 2: Timeframe Performance
        var timeframeRelated = GetRelatedByTimeframe(pendingJob.Timeframe, completedJobs);
        if (timeframeRelated.Any())
        {
            var tfStats = CalculateAggregateStats(timeframeRelated);
            var tfValue = NormalizeAD(tfStats.AverageAD);
            var tfContribution = tfValue * weights.TimeframePerformance;

            factors.Add(new PromiseFactor
            {
                Name = "Timeframe Performance",
                Weight = weights.TimeframePerformance,
                Value = tfValue,
                Contribution = tfContribution,
                Description = $"{timeframeRelated.Count} jobs on {pendingJob.Timeframe}, avg AD: {tfStats.AverageAD:F2}, best: {tfStats.BestAD:F2}"
            });

            if (tfStats.AverageAD >= GoodAD)
            {
                reasoningParts.Add($"{pendingJob.Timeframe} timeframe performs well (avg AD {tfStats.AverageAD:F2})");
            }

            totalDataPoints += timeframeRelated.Count;
        }
        else
        {
            factors.Add(new PromiseFactor
            {
                Name = "Timeframe Performance",
                Weight = weights.TimeframePerformance,
                Value = 0.5, // Neutral
                Contribution = 0.5 * weights.TimeframePerformance,
                Description = $"No prior jobs for {pendingJob.Timeframe}"
            });
        }

        // Factor 3: Market Characteristics (symbol+timeframe combo)
        var comboRelated = GetRelatedBySymbolAndTimeframe(pendingJob.Symbol, pendingJob.Timeframe, completedJobs);
        if (comboRelated.Any())
        {
            var comboStats = CalculateAggregateStats(comboRelated);
            var comboValue = NormalizeAD(comboStats.AverageAD);
            var comboContribution = comboValue * weights.MarketCharacteristics;

            factors.Add(new PromiseFactor
            {
                Name = "Market Characteristics",
                Weight = weights.MarketCharacteristics,
                Value = comboValue,
                Contribution = comboContribution,
                Description = $"{comboRelated.Count} exact matches ({pendingJob.Symbol}/{pendingJob.Timeframe}), avg AD: {comboStats.AverageAD:F2}"
            });

            if (comboStats.AverageAD >= GoodAD)
            {
                reasoningParts.Add($"Exact combo {pendingJob.Symbol}/{pendingJob.Timeframe} historically strong");
            }

            totalDataPoints += comboRelated.Count * 2; // Weight exact matches more heavily
        }
        else
        {
            factors.Add(new PromiseFactor
            {
                Name = "Market Characteristics",
                Weight = weights.MarketCharacteristics,
                Value = 0.5, // Neutral
                Contribution = 0.5 * weights.MarketCharacteristics,
                Description = $"No prior jobs for {pendingJob.Symbol}/{pendingJob.Timeframe} combo"
            });
        }

        // Factor 4: Recency - prefer jobs with recent successful results
        var recentRelated = GetRecentRelated(pendingJob, completedJobs, TimeSpan.FromDays(30));
        if (recentRelated.Any())
        {
            var recentStats = CalculateAggregateStats(recentRelated);
            var recencyValue = NormalizeAD(recentStats.AverageAD);
            var recencyContribution = recencyValue * weights.Recency;

            factors.Add(new PromiseFactor
            {
                Name = "Recency",
                Weight = weights.Recency,
                Value = recencyValue,
                Contribution = recencyContribution,
                Description = $"{recentRelated.Count} recent related jobs (last 30 days), avg AD: {recentStats.AverageAD:F2}"
            });

            totalDataPoints += recentRelated.Count;
        }
        else
        {
            factors.Add(new PromiseFactor
            {
                Name = "Recency",
                Weight = weights.Recency,
                Value = 0.5, // Neutral
                Contribution = 0.5 * weights.Recency,
                Description = "No recent related jobs"
            });
        }

        // Calculate final score
        var totalScore = factors.Sum(f => f.Contribution);

        // Calculate confidence based on data availability
        var confidence = CalculateConfidence(totalDataPoints, symbolRelated.Count > 0, timeframeRelated.Count > 0);

        // Build reasoning string
        var reasoning = reasoningParts.Count > 0
            ? string.Join("; ", reasoningParts)
            : "Insufficient data for detailed reasoning";

        return new PromiseScore
        {
            Score = Math.Clamp(totalScore, 0.0, 1.0),
            Confidence = confidence,
            Factors = factors,
            Reasoning = reasoning
        };
    }

    /// <summary>
    /// Find jobs with the same symbol.
    /// </summary>
    public IReadOnlyList<OptimizationJob> GetRelatedBySymbol(string symbol, IReadOnlyList<OptimizationJob> completedJobs)
    {
        return completedJobs
            .Where(j => j.Status == JobStatus.Completed &&
                       j.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                       j.BestAD.HasValue)
            .ToList();
    }

    /// <summary>
    /// Find jobs with the same timeframe.
    /// </summary>
    public IReadOnlyList<OptimizationJob> GetRelatedByTimeframe(string timeframe, IReadOnlyList<OptimizationJob> completedJobs)
    {
        return completedJobs
            .Where(j => j.Status == JobStatus.Completed &&
                       j.Timeframe.Equals(timeframe, StringComparison.OrdinalIgnoreCase) &&
                       j.BestAD.HasValue)
            .ToList();
    }

    /// <summary>
    /// Find jobs with the same symbol AND timeframe.
    /// </summary>
    public IReadOnlyList<OptimizationJob> GetRelatedBySymbolAndTimeframe(
        string symbol, string timeframe, IReadOnlyList<OptimizationJob> completedJobs)
    {
        return completedJobs
            .Where(j => j.Status == JobStatus.Completed &&
                       j.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                       j.Timeframe.Equals(timeframe, StringComparison.OrdinalIgnoreCase) &&
                       j.BestAD.HasValue)
            .ToList();
    }

    /// <summary>
    /// Find related jobs completed within a time window.
    /// </summary>
    private IReadOnlyList<OptimizationJob> GetRecentRelated(
        OptimizationJob pendingJob,
        IReadOnlyList<OptimizationJob> completedJobs,
        TimeSpan window)
    {
        var cutoff = DateTimeOffset.UtcNow - window;
        return completedJobs
            .Where(j => j.Status == JobStatus.Completed &&
                       j.CompletedAt >= cutoff &&
                       j.BestAD.HasValue &&
                       (j.Symbol.Equals(pendingJob.Symbol, StringComparison.OrdinalIgnoreCase) ||
                        j.Timeframe.Equals(pendingJob.Timeframe, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Calculate aggregate statistics for a set of jobs.
    /// </summary>
    public (double AverageAD, double BestAD, int Count) CalculateAggregateStats(IReadOnlyList<OptimizationJob> jobs)
    {
        if (jobs.Count == 0)
        {
            return (0, 0, 0);
        }

        var adsWithValues = jobs.Where(j => j.BestAD.HasValue).Select(j => j.BestAD!.Value).ToList();
        if (adsWithValues.Count == 0)
        {
            return (0, 0, jobs.Count);
        }

        return (
            AverageAD: adsWithValues.Average(),
            BestAD: adsWithValues.Max(),
            Count: jobs.Count
        );
    }

    /// <summary>
    /// Normalize AD value to 0-1 range.
    /// </summary>
    private double NormalizeAD(double ad)
    {
        // Map AD to 0-1 range:
        // AD <= 0.5 (Poor): 0.0-0.25
        // AD 0.5-1.0 (Marginal): 0.25-0.5
        // AD 1.0-2.0 (Good): 0.5-0.75
        // AD 2.0-3.0 (Excellent): 0.75-0.9
        // AD >= 3.0 (Outstanding): 0.9-1.0

        return ad switch
        {
            <= 0 => 0,
            <= PoorAD => 0.25 * (ad / PoorAD),
            <= MarginalAD => 0.25 + 0.25 * ((ad - PoorAD) / (MarginalAD - PoorAD)),
            <= GoodAD => 0.5 + 0.25 * ((ad - MarginalAD) / (GoodAD - MarginalAD)),
            <= ExcellentAD => 0.75 + 0.15 * ((ad - GoodAD) / (ExcellentAD - GoodAD)),
            _ => Math.Min(1.0, 0.9 + 0.1 * Math.Min(1.0, (ad - ExcellentAD) / 2.0))
        };
    }

    /// <summary>
    /// Calculate confidence based on data availability.
    /// </summary>
    private double CalculateConfidence(int totalDataPoints, bool hasSymbolData, bool hasTimeframeData)
    {
        // Base confidence from data volume
        var volumeConfidence = Math.Min(1.0, totalDataPoints / 10.0);

        // Boost for having both types of data
        var coverageBoost = (hasSymbolData && hasTimeframeData) ? 0.2 : 0.0;

        return Math.Min(1.0, volumeConfidence * 0.8 + coverageBoost);
    }
}
