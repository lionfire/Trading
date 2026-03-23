using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Analyzes optimization results to compute per-parameter sensitivity and recommend step sizes.
///
/// Algorithm:
/// 1. Read backtest results with parameter values from CSV
/// 2. For each parameter, sort results by that parameter's value
/// 3. Compute finite-difference gradients between adjacent parameter values (holding conceptually others constant via averaging)
/// 4. Weight gradients by trade-count confidence
/// 5. Aggregate gradient statistics (mean, max, variance) to determine sensitivity level
/// 6. Derive recommended step from sensitivity level and parameter range
/// </summary>
public class SensitivityAnalyzer
{
    private readonly ILogger<SensitivityAnalyzer> _logger;
    private readonly IOptionsMonitor<SensitivityAnalysisOptions> _options;

    public SensitivityAnalyzer(
        ILogger<SensitivityAnalyzer> logger,
        IOptionsMonitor<SensitivityAnalysisOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Analyze sensitivity from a backtest results path (directory or zip file).
    /// </summary>
    public StepRecommendationSet Analyze(string resultsPath, string? planId = null, string? botType = null, string? symbol = null, string? timeFrame = null)
    {
        var options = _options.CurrentValue;
        var results = ParameterAwareResultsReader.ReadFromPath(resultsPath, options);
        return AnalyzeResults(results, options, planId, botType, symbol, timeFrame);
    }

    /// <summary>
    /// Analyze sensitivity from pre-loaded results.
    /// </summary>
    public StepRecommendationSet AnalyzeResults(
        List<BacktestResultWithParameters> results,
        SensitivityAnalysisOptions? options = null,
        string? planId = null,
        string? botType = null,
        string? symbol = null,
        string? timeFrame = null)
    {
        options ??= _options.CurrentValue;

        if (results.Count == 0)
        {
            _logger.LogWarning("No qualifying backtest results to analyze for sensitivity");
            return new StepRecommendationSet
            {
                PlanId = planId,
                BotType = botType,
                Symbol = symbol,
                TimeFrame = timeFrame,
                Metric = options.Metric,
                AnalyzedAt = DateTimeOffset.UtcNow,
                TotalEntries = 0,
                QualifyingEntries = 0,
                MinTradeCount = options.MinTradeCount,
                FullConfidenceTradeCount = options.FullConfidenceTradeCount,
                Recommendations = [],
            };
        }

        // Determine which parameters are present
        var parameterNames = results
            .SelectMany(r => r.ParameterValues.Keys)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        _logger.LogDebug("Analyzing sensitivity for {ParameterCount} parameters across {ResultCount} qualifying results",
            parameterNames.Count, results.Count);

        var sensitivities = new List<ParameterSensitivity>();
        foreach (var paramName in parameterNames)
        {
            var sensitivity = ComputeParameterSensitivity(paramName, results, options);
            if (sensitivity != null)
            {
                sensitivities.Add(sensitivity);
            }
        }

        // Derive step recommendations from sensitivities
        var recommendations = sensitivities
            .Select(s => DeriveStepRecommendation(s, results, options))
            .ToList();

        return new StepRecommendationSet
        {
            PlanId = planId,
            BotType = botType,
            Symbol = symbol,
            TimeFrame = timeFrame,
            Metric = options.Metric,
            AnalyzedAt = DateTimeOffset.UtcNow,
            TotalEntries = results.Count,
            QualifyingEntries = results.Count,
            MinTradeCount = options.MinTradeCount,
            FullConfidenceTradeCount = options.FullConfidenceTradeCount,
            Recommendations = recommendations,
        };
    }

    /// <summary>
    /// Compute sensitivity for a single parameter by analyzing how the metric changes
    /// as the parameter value varies.
    /// </summary>
    private ParameterSensitivity? ComputeParameterSensitivity(
        string parameterName,
        List<BacktestResultWithParameters> results,
        SensitivityAnalysisOptions options)
    {
        // Filter to results that have this parameter
        var withParam = results
            .Where(r => r.ParameterValues.ContainsKey(parameterName))
            .ToList();

        if (withParam.Count < 2)
        {
            _logger.LogDebug("Skipping parameter {ParameterName}: only {Count} results", parameterName, withParam.Count);
            return null;
        }

        // Group by parameter value and compute weighted average metric for each value
        var groupedByValue = withParam
            .GroupBy(r => r.ParameterValues[parameterName])
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var weightedSum = g.Sum(r => GetMetricValue(r, options.Metric) * r.Confidence);
                var weightSum = g.Sum(r => r.Confidence);
                var weightedAverage = weightSum > 0 ? weightedSum / weightSum : 0;

                return new
                {
                    ParameterValue = g.Key,
                    WeightedMetricAverage = weightedAverage,
                    EffectiveWeight = weightSum,
                    Count = g.Count(),
                };
            })
            .ToList();

        if (groupedByValue.Count < 2)
        {
            _logger.LogDebug("Skipping parameter {ParameterName}: only {Count} distinct values", parameterName, groupedByValue.Count);
            return null;
        }

        // Compute finite-difference gradients between adjacent parameter values
        var gradients = new List<double>();
        var gradientWeights = new List<double>();

        for (int i = 1; i < groupedByValue.Count; i++)
        {
            var prev = groupedByValue[i - 1];
            var curr = groupedByValue[i];

            var dParam = curr.ParameterValue - prev.ParameterValue;
            if (Math.Abs(dParam) < 1e-15) continue; // Same parameter value

            var dMetric = curr.WeightedMetricAverage - prev.WeightedMetricAverage;
            var gradient = Math.Abs(dMetric / dParam);

            // Weight by the minimum effective weight of the two endpoints
            var weight = Math.Min(prev.EffectiveWeight, curr.EffectiveWeight);

            gradients.Add(gradient);
            gradientWeights.Add(weight);
        }

        if (gradients.Count == 0) return null;

        // Compute weighted gradient statistics
        var totalWeight = gradientWeights.Sum();
        var weightedMeanGradient = gradients
            .Zip(gradientWeights, (g, w) => g * w)
            .Sum() / totalWeight;

        var maxGradient = gradients.Max();

        var weightedVariance = gradients
            .Zip(gradientWeights, (g, w) => w * (g - weightedMeanGradient) * (g - weightedMeanGradient))
            .Sum() / totalWeight;

        // Classify sensitivity level using the metric range for normalization
        var metricValues = withParam.Select(r => GetMetricValue(r, options.Metric)).Where(v => !double.IsNaN(v)).ToList();
        var metricRange = metricValues.Count > 0 ? metricValues.Max() - metricValues.Min() : 1.0;
        if (metricRange < 1e-10) metricRange = 1.0;

        // Normalize gradient by parameter range to make sensitivity comparable across parameters
        var paramRange = groupedByValue.Last().ParameterValue - groupedByValue.First().ParameterValue;
        if (Math.Abs(paramRange) < 1e-15) paramRange = 1.0;

        // Normalized sensitivity: how much does the metric change (as fraction of range) per unit parameter change (as fraction of range)
        var normalizedMeanGradient = (weightedMeanGradient * paramRange) / metricRange;

        var level = normalizedMeanGradient >= options.HighSensitivityThreshold ? SensitivityLevel.High
            : normalizedMeanGradient >= options.MediumSensitivityThreshold ? SensitivityLevel.Medium
            : SensitivityLevel.Low;

        // Check for localized sensitivity
        var cv = weightedMeanGradient > 1e-15 ? Math.Sqrt(weightedVariance) / weightedMeanGradient : 0;
        var hasLocalizedSensitivity = cv > options.LocalizedSensitivityCvThreshold;

        return new ParameterSensitivity
        {
            ParameterName = parameterName,
            MeanGradientMagnitude = weightedMeanGradient,
            MaxGradientMagnitude = maxGradient,
            GradientVariance = weightedVariance,
            Level = level,
            SampleCount = gradients.Count,
            EffectiveSampleSize = totalWeight,
            HasLocalizedSensitivity = hasLocalizedSensitivity,
        };
    }

    /// <summary>
    /// Derive a step recommendation from sensitivity analysis.
    /// </summary>
    private StepRecommendation DeriveStepRecommendation(
        ParameterSensitivity sensitivity,
        List<BacktestResultWithParameters> results,
        SensitivityAnalysisOptions options)
    {
        // Determine the parameter range from the data
        var paramValues = results
            .Where(r => r.ParameterValues.ContainsKey(sensitivity.ParameterName))
            .Select(r => r.ParameterValues[sensitivity.ParameterName])
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        var paramMin = paramValues.First();
        var paramMax = paramValues.Last();
        var paramRange = paramMax - paramMin;

        if (paramRange < 1e-15)
        {
            // Only one unique value — can't recommend a step
            return new StepRecommendation
            {
                ParameterName = sensitivity.ParameterName,
                RecommendedStep = paramRange,
                RecommendedStepExponent = 1.0,
                Confidence = 0,
                Sensitivity = sensitivity,
            };
        }

        // Target sample points based on sensitivity level
        var targetPoints = sensitivity.Level switch
        {
            SensitivityLevel.High => options.TargetSamplePoints,
            SensitivityLevel.Medium => (options.TargetSamplePoints + options.MinSamplePoints) / 2,
            SensitivityLevel.Low => options.MinSamplePoints,
            _ => options.MinSamplePoints,
        };

        targetPoints = Math.Clamp(targetPoints, options.MinSamplePoints, options.MaxSamplePoints);

        var recommendedStep = paramRange / targetPoints;

        // Round step to a "nice" value (power of 10, or 1/2/5 × power of 10)
        recommendedStep = RoundToNiceStep(recommendedStep);

        // Determine step exponent based on gradient variance pattern
        double recommendedExponent = 1.0;
        if (sensitivity.HasLocalizedSensitivity && paramValues.Count >= 4)
        {
            // Check if sensitivity is higher at low or high end of parameter range
            var midpoint = (paramMin + paramMax) / 2;
            var lowResults = results
                .Where(r => r.ParameterValues.ContainsKey(sensitivity.ParameterName)
                    && r.ParameterValues[sensitivity.ParameterName] < midpoint)
                .ToList();
            var highResults = results
                .Where(r => r.ParameterValues.ContainsKey(sensitivity.ParameterName)
                    && r.ParameterValues[sensitivity.ParameterName] >= midpoint)
                .ToList();

            if (lowResults.Count >= 2 && highResults.Count >= 2)
            {
                var lowVariance = ComputeMetricVariance(lowResults, options.Metric);
                var highVariance = ComputeMetricVariance(highResults, options.Metric);

                if (lowVariance > highVariance * 2)
                {
                    recommendedExponent = 0.7; // More points at low end
                }
                else if (highVariance > lowVariance * 2)
                {
                    recommendedExponent = 1.5; // More points at high end
                }
            }
        }

        // Confidence: based on sample count and effective weight
        var sampleConfidence = Math.Min(1.0, sensitivity.SampleCount / 10.0);
        var weightConfidence = Math.Min(1.0, sensitivity.EffectiveSampleSize / 5.0);
        var confidence = sampleConfidence * weightConfidence;

        return new StepRecommendation
        {
            ParameterName = sensitivity.ParameterName,
            RecommendedStep = recommendedStep,
            RecommendedStepExponent = recommendedExponent,
            Confidence = Math.Round(confidence, 3),
            Sensitivity = sensitivity,
        };
    }

    /// <summary>
    /// Extract the relevant metric value from a result entry.
    /// </summary>
    private static double GetMetricValue(BacktestResultWithParameters result, SensitivityMetric metric)
    {
        return metric switch
        {
            SensitivityMetric.AD => result.Entry.AD,
            SensitivityMetric.Fitness => result.Entry.Fitness,
            SensitivityMetric.NetPnL => result.Entry.AD, // Fallback to AD — NetPnL not in CSV journal yet
            _ => result.Entry.AD,
        };
    }

    /// <summary>
    /// Compute variance of metric values for a set of results.
    /// </summary>
    private static double ComputeMetricVariance(List<BacktestResultWithParameters> results, SensitivityMetric metric)
    {
        var values = results.Select(r => GetMetricValue(r, metric)).Where(v => !double.IsNaN(v)).ToList();
        if (values.Count < 2) return 0;
        var mean = values.Average();
        return values.Sum(v => (v - mean) * (v - mean)) / (values.Count - 1);
    }

    /// <summary>
    /// Round a step value to a "nice" number (1, 2, 5, or 10 × power of 10).
    /// </summary>
    internal static double RoundToNiceStep(double rawStep)
    {
        if (rawStep <= 0) return rawStep;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        var normalized = rawStep / magnitude;

        // Round to nearest nice value: 1, 2, 5, or 10
        double niceNormalized;
        if (normalized < 1.5)
            niceNormalized = 1;
        else if (normalized < 3.5)
            niceNormalized = 2;
        else if (normalized < 7.5)
            niceNormalized = 5;
        else
            niceNormalized = 10;

        return niceNormalized * magnitude;
    }
}
