namespace LionFire.Trading.Streaming;

/// <summary>
/// Severity classification for sequence gaps.
/// </summary>
public enum GapSeverity
{
    /// <summary>Small gap (&lt;10 items), often recoverable.</summary>
    Small,

    /// <summary>Medium gap (&lt;100 items), may indicate transient issue.</summary>
    Medium,

    /// <summary>Large gap (100+ items), likely requires resync.</summary>
    Large
}

/// <summary>
/// Represents a detected gap in a sequence of messages.
/// </summary>
/// <param name="Expected">The sequence ID that was expected.</param>
/// <param name="Received">The sequence ID that was actually received.</param>
/// <param name="DetectedAt">Timestamp when the gap was detected.</param>
public record SequenceGap(
    long Expected,
    long Received,
    DateTime DetectedAt)
{
    /// <summary>
    /// Gets the size of the gap (number of missing sequences).
    /// </summary>
    public long GapSize => Received - Expected;

    /// <summary>
    /// Gets the severity classification of this gap.
    /// </summary>
    public GapSeverity Severity => GapSize switch
    {
        < 10 => GapSeverity.Small,
        < 100 => GapSeverity.Medium,
        _ => GapSeverity.Large
    };
}

/// <summary>
/// Event arguments for gap detection events.
/// </summary>
/// <param name="Gap">The detected gap.</param>
/// <param name="StreamName">Optional name of the stream where the gap occurred.</param>
/// <param name="TotalGapsDetected">Total gaps detected so far.</param>
public record SequenceGapEventArgs(
    SequenceGap Gap,
    string? StreamName,
    long TotalGapsDetected);

/// <summary>
/// Metrics interface for sequence tracker observability.
/// </summary>
public interface ISequenceTrackerMetrics
{
    /// <summary>Total messages processed.</summary>
    long TotalProcessed { get; }

    /// <summary>Total gaps detected.</summary>
    long TotalGaps { get; }

    /// <summary>Total duplicate sequences rejected.</summary>
    long TotalDuplicates { get; }

    /// <summary>Largest gap seen.</summary>
    long LargestGap { get; }

    /// <summary>Gap percentage (gaps / total).</summary>
    double GapPercentage { get; }

    /// <summary>Current sequence number.</summary>
    long CurrentSequence { get; }
}
