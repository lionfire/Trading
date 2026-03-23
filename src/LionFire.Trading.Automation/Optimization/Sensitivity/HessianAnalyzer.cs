using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Computes cross-parameter interactions using finite-difference approximations of the Hessian
/// (mixed partial derivatives) on optimization result grids.
///
/// Algorithm:
/// 1. Select parameter pairs from univariate sensitivity results, filtering by MinSensitivityLevel
///    and capping at MaxPairs (sorted by product of individual gradient magnitudes, descending).
/// 2. For each pair (A, B), enumerate all 2x2 subgrids of (valueA, valueB) where all four corners
///    have at least one backtest result. Compute the mixed partial derivative via:
///       d²f/dAdB ≈ (f(a+h, b+k) - f(a+h, b) - f(a, b+k) + f(a, b)) / (h * k)
///    Each subgrid cell uses the confidence-weighted average metric value.
/// 3. Aggregate mixed partials: mean magnitude, sign consistency for direction classification.
/// 4. Score: InteractionScore = meanMixedPartialMagnitude / (meanGradientA * meanGradientB).
/// 5. Classify strength (Independent / Weak / Strong) via threshold comparison.
/// </summary>
public class HessianAnalyzer
{
    private readonly ILogger<HessianAnalyzer> _logger;
    private readonly IOptionsMonitor<HessianAnalysisOptions> _hessianOptions;
    private readonly IOptionsMonitor<SensitivityAnalysisOptions> _sensitivityOptions;

    public HessianAnalyzer(
        ILogger<HessianAnalyzer> logger,
        IOptionsMonitor<HessianAnalysisOptions> hessianOptions,
        IOptionsMonitor<SensitivityAnalysisOptions> sensitivityOptions)
    {
        _logger = logger;
        _hessianOptions = hessianOptions;
        _sensitivityOptions = sensitivityOptions;
    }

    /// <summary>
    /// Analyze parameter interactions from a backtest results path (directory or zip file).
    /// </summary>
    public HessianAnalysisResult Analyze(
        string resultsPath,
        List<ParameterSensitivity> univariateSensitivities,
        HessianAnalysisOptions? options = null,
        string? planId = null,
        string? botType = null,
        string? symbol = null)
    {
        var sensitivityOptions = _sensitivityOptions.CurrentValue;
        var results = ParameterAwareResultsReader.ReadFromPath(resultsPath, sensitivityOptions);
        return Analyze(results, univariateSensitivities, options, planId, botType, symbol);
    }

    /// <summary>
    /// Analyze parameter interactions from pre-loaded backtest results and univariate sensitivities.
    /// </summary>
    public HessianAnalysisResult Analyze(
        List<BacktestResultWithParameters> results,
        List<ParameterSensitivity> univariateSensitivities,
        HessianAnalysisOptions? options = null,
        string? planId = null,
        string? botType = null,
        string? symbol = null)
    {
        options ??= _hessianOptions.CurrentValue;
        var metricSelector = CreateMetricSelector(_sensitivityOptions.CurrentValue.Metric);

        if (results.Count == 0 || univariateSensitivities.Count == 0)
        {
            _logger.LogWarning("Insufficient data for Hessian analysis: {ResultCount} results, {SensitivityCount} sensitivities",
                results.Count, univariateSensitivities.Count);
            return EmptyResult(options, planId, botType, symbol);
        }

        // Step 1: Select parameter pairs
        var pairs = SelectParameterPairs(univariateSensitivities, options);

        if (pairs.Count == 0)
        {
            _logger.LogDebug("No parameter pairs qualify for Hessian analysis (MinSensitivityLevel={Level})",
                options.MinSensitivityLevel);
            return EmptyResult(options, planId, botType, symbol);
        }

        _logger.LogDebug("Analyzing {PairCount} parameter pairs for cross-parameter interactions", pairs.Count);

        // Step 2-5: Compute interactions for each pair
        var interactions = new List<ParameterInteraction>();
        foreach (var (sensA, sensB) in pairs)
        {
            var interaction = ComputePairInteraction(sensA, sensB, results, options, metricSelector);
            if (interaction != null)
            {
                interactions.Add(interaction);
            }
        }

        var analyzedParameters = pairs
            .SelectMany(p => new[] { p.SensA.ParameterName, p.SensB.ParameterName })
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        _logger.LogInformation("Hessian analysis complete: {InteractionCount} interactions computed, {SignificantCount} significant",
            interactions.Count, interactions.Count(i => i.Strength != InteractionStrength.Independent));

        return new HessianAnalysisResult
        {
            PlanId = planId,
            BotType = botType,
            Symbol = symbol,
            AnalyzedAt = DateTimeOffset.UtcNow,
            AnalyzedParameters = analyzedParameters,
            Interactions = interactions,
        };
    }

    /// <summary>
    /// Select parameter pairs for analysis, filtered by sensitivity level and capped at MaxPairs.
    /// Pairs are sorted by the product of individual gradient magnitudes (most sensitive pairs first).
    /// </summary>
    private static List<(ParameterSensitivity SensA, ParameterSensitivity SensB)> SelectParameterPairs(
        List<ParameterSensitivity> sensitivities,
        HessianAnalysisOptions options)
    {
        var qualifying = sensitivities
            .Where(s => s.Level >= options.MinSensitivityLevel)
            .OrderByDescending(s => s.MeanGradientMagnitude)
            .ToList();

        if (qualifying.Count < 2) return [];

        // Generate all pairs (N choose 2), sorted by product of gradient magnitudes descending
        var pairs = new List<(ParameterSensitivity SensA, ParameterSensitivity SensB, double Score)>();
        for (int i = 0; i < qualifying.Count; i++)
        {
            for (int j = i + 1; j < qualifying.Count; j++)
            {
                var score = qualifying[i].MeanGradientMagnitude * qualifying[j].MeanGradientMagnitude;
                pairs.Add((qualifying[i], qualifying[j], score));
            }
        }

        return pairs
            .OrderByDescending(p => p.Score)
            .Take(options.MaxPairs)
            .Select(p => (p.SensA, p.SensB))
            .ToList();
    }

    /// <summary>
    /// Compute the interaction between two parameters using 2x2 finite-difference subgrids.
    /// </summary>
    private ParameterInteraction? ComputePairInteraction(
        ParameterSensitivity sensA,
        ParameterSensitivity sensB,
        List<BacktestResultWithParameters> results,
        HessianAnalysisOptions options,
        Func<BacktestResultWithParameters, double> metricSelector)
    {
        var paramA = sensA.ParameterName;
        var paramB = sensB.ParameterName;

        // Filter results that have both parameters
        var withBoth = results
            .Where(r => r.ParameterValues.ContainsKey(paramA) && r.ParameterValues.ContainsKey(paramB))
            .ToList();

        if (withBoth.Count < 4)
        {
            _logger.LogDebug("Skipping pair ({ParamA}, {ParamB}): only {Count} results with both parameters",
                paramA, paramB, withBoth.Count);
            return null;
        }

        // Build a lookup: (valueA, valueB) -> list of (metricValue, confidence)
        var grid = new Dictionary<(double A, double B), (double WeightedMetric, double TotalWeight, int Count)>();
        foreach (var r in withBoth)
        {
            var key = (r.ParameterValues[paramA], r.ParameterValues[paramB]);
            var metric = metricSelector(r);
            if (double.IsNaN(metric)) continue;

            if (grid.TryGetValue(key, out var existing))
            {
                grid[key] = (
                    existing.WeightedMetric + metric * r.Confidence,
                    existing.TotalWeight + r.Confidence,
                    existing.Count + 1);
            }
            else
            {
                grid[key] = (metric * r.Confidence, r.Confidence, 1);
            }
        }

        // Get sorted distinct values for each parameter
        var valuesA = grid.Keys.Select(k => k.A).Distinct().OrderBy(v => v).ToList();
        var valuesB = grid.Keys.Select(k => k.B).Distinct().OrderBy(v => v).ToList();

        if (valuesA.Count < 2 || valuesB.Count < 2)
        {
            _logger.LogDebug("Skipping pair ({ParamA}, {ParamB}): insufficient distinct values ({CountA}x{CountB})",
                paramA, paramB, valuesA.Count, valuesB.Count);
            return null;
        }

        // Enumerate all 2x2 subgrids and compute mixed partial derivatives
        var mixedPartials = new List<(double Value, double Weight)>();

        for (int i = 0; i < valuesA.Count - 1; i++)
        {
            for (int j = 0; j < valuesB.Count - 1; j++)
            {
                var a0 = valuesA[i];
                var a1 = valuesA[i + 1];
                var b0 = valuesB[j];
                var b1 = valuesB[j + 1];

                // All four corners must exist
                if (!grid.TryGetValue((a0, b0), out var c00)) continue;
                if (!grid.TryGetValue((a1, b0), out var c10)) continue;
                if (!grid.TryGetValue((a0, b1), out var c01)) continue;
                if (!grid.TryGetValue((a1, b1), out var c11)) continue;

                var h = a1 - a0;
                var k = b1 - b0;
                if (Math.Abs(h) < 1e-15 || Math.Abs(k) < 1e-15) continue;

                // Compute weighted averages for each corner
                var f00 = c00.WeightedMetric / c00.TotalWeight;
                var f10 = c10.WeightedMetric / c10.TotalWeight;
                var f01 = c01.WeightedMetric / c01.TotalWeight;
                var f11 = c11.WeightedMetric / c11.TotalWeight;

                // Mixed partial: d²f/dAdB ≈ (f(a+h,b+k) - f(a+h,b) - f(a,b+k) + f(a,b)) / (h*k)
                var mixedPartial = (f11 - f10 - f01 + f00) / (h * k);

                // Weight by the minimum confidence of the four corners
                var weight = Math.Min(
                    Math.Min(c00.TotalWeight, c10.TotalWeight),
                    Math.Min(c01.TotalWeight, c11.TotalWeight));

                mixedPartials.Add((mixedPartial, weight));
            }
        }

        if (mixedPartials.Count == 0)
        {
            _logger.LogDebug("Skipping pair ({ParamA}, {ParamB}): no complete 2x2 subgrids found",
                paramA, paramB);
            return null;
        }

        // Aggregate: weighted mean magnitude, sign consistency
        var totalWeight = mixedPartials.Sum(mp => mp.Weight);
        var weightedMeanMagnitude = mixedPartials.Sum(mp => Math.Abs(mp.Value) * mp.Weight) / totalWeight;
        var weightedMeanSigned = mixedPartials.Sum(mp => mp.Value * mp.Weight) / totalWeight;

        // Direction: check sign consistency
        var positiveWeight = mixedPartials.Where(mp => mp.Value > 0).Sum(mp => mp.Weight);
        var negativeWeight = mixedPartials.Where(mp => mp.Value < 0).Sum(mp => mp.Weight);
        var dominantFraction = Math.Max(positiveWeight, negativeWeight) / totalWeight;

        // If >75% of weight agrees on sign, classify as synergistic or antagonistic
        const double signConsistencyThreshold = 0.75;
        InteractionDirection direction;
        if (dominantFraction >= signConsistencyThreshold)
        {
            direction = weightedMeanSigned > 0 ? InteractionDirection.Synergistic : InteractionDirection.Antagonistic;
        }
        else
        {
            direction = InteractionDirection.Mixed;
        }

        // Score: normalize by product of individual gradient magnitudes
        var gradientProduct = sensA.MeanGradientMagnitude * sensB.MeanGradientMagnitude;
        var interactionScore = gradientProduct > 1e-15
            ? weightedMeanMagnitude / gradientProduct
            : 0.0;

        // Classify strength
        var strength = interactionScore >= options.StrongInteractionThreshold ? InteractionStrength.Strong
            : interactionScore >= options.WeakInteractionThreshold ? InteractionStrength.Weak
            : InteractionStrength.Independent;

        // Confidence: based on sample count relative to MinSampleCount
        var sampleConfidence = Math.Min(1.0, (double)mixedPartials.Count / options.MinSampleCount);
        var weightConfidence = Math.Min(1.0, totalWeight / (options.MinSampleCount * 2.0));
        var confidence = Math.Round(sampleConfidence * weightConfidence, 3);

        _logger.LogDebug("Pair ({ParamA}, {ParamB}): score={Score:F4}, strength={Strength}, direction={Direction}, samples={Samples}",
            paramA, paramB, interactionScore, strength, direction, mixedPartials.Count);

        return new ParameterInteraction
        {
            ParameterA = paramA,
            ParameterB = paramB,
            InteractionMagnitude = weightedMeanMagnitude,
            InteractionScore = interactionScore,
            Strength = strength,
            Direction = direction,
            SampleCount = mixedPartials.Count,
            Confidence = confidence,
        };
    }

    /// <summary>
    /// Create a metric extraction function for the configured metric.
    /// </summary>
    private static Func<BacktestResultWithParameters, double> CreateMetricSelector(SensitivityMetric metric)
    {
        return metric switch
        {
            SensitivityMetric.AD => r => r.Entry.AD,
            SensitivityMetric.Fitness => r => r.Entry.Fitness,
            SensitivityMetric.NetPnL => r => r.Entry.AD, // Fallback to AD — NetPnL not in CSV journal yet
            _ => r => r.Entry.AD,
        };
    }

    private static HessianAnalysisResult EmptyResult(
        HessianAnalysisOptions options,
        string? planId,
        string? botType,
        string? symbol)
    {
        return new HessianAnalysisResult
        {
            PlanId = planId,
            BotType = botType,
            Symbol = symbol,
            AnalyzedAt = DateTimeOffset.UtcNow,
            AnalyzedParameters = [],
            Interactions = [],
        };
    }
}
