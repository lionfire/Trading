namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Histogram showing the distribution of AD (Annualized ROI / Drawdown) values across backtests.
/// </summary>
public record AdHistogram
{
    /// <summary>
    /// Default bucket boundaries for AD histogram.
    /// </summary>
    /// <remarks>
    /// Buckets: &lt;0, 0-0.5, 0.5-1.0, 1.0-2.0, 2.0-3.0, 3.0-5.0, 5.0+
    /// - AD &lt; 0: Losing strategies
    /// - AD 0-0.5: Poor risk-adjusted return
    /// - AD 0.5-1.0: Marginal
    /// - AD 1.0-2.0: Acceptable (annualized gain exceeds max drawdown)
    /// - AD 2.0-3.0: Good
    /// - AD 3.0-5.0: Strong
    /// - AD 5.0+: Exceptional (verify not overfitting)
    /// </remarks>
    public static readonly double[] DefaultBucketBoundaries =
        [double.NegativeInfinity, 0, 0.5, 1.0, 2.0, 3.0, 5.0, double.PositiveInfinity];

    /// <summary>
    /// The histogram buckets with counts and percentages.
    /// </summary>
    public IReadOnlyList<HistogramBucket> Buckets { get; init; } = [];

    /// <summary>
    /// Total number of backtests in the histogram.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The metric this histogram represents (typically "ad").
    /// </summary>
    public string Metric { get; init; } = "ad";
}

/// <summary>
/// A single bucket in a histogram, representing a range of values and the count of items in that range.
/// </summary>
public record HistogramBucket
{
    /// <summary>
    /// Human-readable range description (e.g., "1.0-2.0", "&lt; 0", "5.0+").
    /// </summary>
    public string Range { get; init; } = "";

    /// <summary>
    /// Minimum value (inclusive) for this bucket.
    /// </summary>
    public double Min { get; init; }

    /// <summary>
    /// Maximum value (exclusive) for this bucket.
    /// </summary>
    public double Max { get; init; }

    /// <summary>
    /// Number of items in this bucket.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Percentage of total items in this bucket.
    /// </summary>
    public double Percent { get; init; }

    /// <summary>
    /// Creates a formatted range string based on min/max values.
    /// </summary>
    public static string FormatRange(double min, double max)
    {
        if (double.IsNegativeInfinity(min))
            return $"< {max:F1}";
        if (double.IsPositiveInfinity(max))
            return $"{min:F1}+";
        return $"{min:F1}-{max:F1}";
    }
}
