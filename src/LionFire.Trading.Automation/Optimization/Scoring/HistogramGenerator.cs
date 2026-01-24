namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Generates histograms for metric distributions.
/// </summary>
public static class HistogramGenerator
{
    /// <summary>
    /// Generates an AD histogram from a collection of AD values.
    /// </summary>
    /// <param name="adValues">Collection of AD values</param>
    /// <param name="bucketBoundaries">Bucket boundaries (optional, uses defaults if null)</param>
    /// <returns>Histogram with bucket counts and percentages</returns>
    public static AdHistogram GenerateAdHistogram(
        IReadOnlyList<double> adValues,
        double[]? bucketBoundaries = null)
    {
        bucketBoundaries ??= AdHistogram.DefaultBucketBoundaries;

        var buckets = new List<HistogramBucket>();
        var total = adValues.Count;

        for (int i = 0; i < bucketBoundaries.Length - 1; i++)
        {
            var min = bucketBoundaries[i];
            var max = bucketBoundaries[i + 1];

            var count = adValues.Count(v => v >= min && v < max);
            var percent = total > 0 ? (count * 100.0 / total) : 0;

            buckets.Add(new HistogramBucket
            {
                Range = HistogramBucket.FormatRange(min, max),
                Min = min,
                Max = max,
                Count = count,
                Percent = Math.Round(percent, 1)
            });
        }

        return new AdHistogram
        {
            Buckets = buckets,
            TotalCount = total,
            Metric = "ad"
        };
    }

    /// <summary>
    /// Generates a text-based histogram visualization for console output.
    /// </summary>
    /// <param name="histogram">The histogram to visualize</param>
    /// <param name="maxBarWidth">Maximum width of histogram bars in characters</param>
    /// <returns>Multi-line string with ASCII histogram</returns>
    public static string GenerateTextHistogram(AdHistogram histogram, int maxBarWidth = 30)
    {
        if (histogram.Buckets.Count == 0)
            return "No data for histogram";

        var maxCount = histogram.Buckets.Max(b => b.Count);
        var lines = new List<string>();

        foreach (var bucket in histogram.Buckets)
        {
            var barLength = maxCount > 0
                ? (int)Math.Round((double)bucket.Count / maxCount * maxBarWidth)
                : 0;

            var bar = new string('█', barLength);
            var range = bucket.Range.PadLeft(8);
            var count = bucket.Count.ToString().PadLeft(4);
            var percent = $"({bucket.Percent:F1}%)".PadLeft(8);

            lines.Add($"  {range}: {bar} {count} {percent}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Generates custom buckets based on min/max and bucket count.
    /// </summary>
    public static double[] GenerateEqualWidthBuckets(double min, double max, int bucketCount)
    {
        if (bucketCount <= 0) throw new ArgumentException("Bucket count must be positive", nameof(bucketCount));
        if (max <= min) throw new ArgumentException("Max must be greater than min", nameof(max));

        var width = (max - min) / bucketCount;
        var boundaries = new double[bucketCount + 1];

        boundaries[0] = double.NegativeInfinity;
        for (int i = 1; i < bucketCount; i++)
        {
            boundaries[i] = min + (i * width);
        }
        boundaries[bucketCount] = double.PositiveInfinity;

        return boundaries;
    }
}
