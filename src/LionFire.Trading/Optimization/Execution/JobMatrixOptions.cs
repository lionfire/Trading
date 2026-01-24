namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Options for job matrix generation.
/// </summary>
public record JobMatrixOptions
{
    /// <summary>
    /// Maximum number of jobs allowed in a single plan execution.
    /// Throws if exceeded to prevent accidental massive job generation.
    /// </summary>
    public int MaxJobs { get; init; } = 10000;

    /// <summary>
    /// Whether to resolve dynamic symbol collections.
    /// If false, uses the snapshot from the plan.
    /// </summary>
    public bool ResolveDynamicSymbols { get; init; } = true;

    /// <summary>
    /// Default exchange name for jobs.
    /// </summary>
    public string DefaultExchange { get; init; } = "Binance";

    /// <summary>
    /// Default exchange area for jobs.
    /// </summary>
    public string DefaultExchangeArea { get; init; } = "futures";

    /// <summary>
    /// Reference time for resolving relative date expressions.
    /// Defaults to current UTC time.
    /// </summary>
    public DateTimeOffset? ReferenceTime { get; init; }
}
