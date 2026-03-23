using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Cached summary of a single optimization run's backtest results.
/// Written as a .summary.json file next to the run's zip/directory to avoid
/// re-parsing the full CSV on subsequent loads.
/// </summary>
public record RunSummary
{
    /// <summary>
    /// Schema version for forward compatibility. Bump when fields change.
    /// v1: initial, v2: added IsAborted to CSV writer, v3: infer IsAborted from AD=NaN for old CSVs
    /// </summary>
    public int Version { get; init; } = 3;

    public double BestAd { get; init; }
    public double AverageAd { get; init; }

    /// <summary>
    /// Sum of AD values (for combining multiple runs: combined avg = totalAdSum / totalBacktests).
    /// </summary>
    public double AdSum { get; init; }

    public int TotalBacktests { get; init; }
    public int AbortedBacktests { get; init; }
    public int PassingCount { get; init; }
}
