namespace LionFire.Trading.Automation;

public class PBacktestBatchQueue
{
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Within each batch, Jobs must share the same date range.  (Lookback can always be different.)
    /// </summary>
    public bool SingleDateRange { get; set; } = true;

    #region Clients

    /// <summary>
    /// Maximum number of jobs waiting in queue. Creates backpressure when exceeded.
    /// </summary>
    public int MaxQueuedJobs { get; set; } = 100;

    /// <summary>
    /// Maximum number of jobs executing concurrently. Each job loads historical data
    /// and can consume significant memory. Default is half the processor count.
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = Math.Max(1, Environment.ProcessorCount - 1);

    #endregion

    public BacktestExecutionOptions BacktestExecutionOptions { get; set; } = new();


    public bool AutoStart { get;set; } = true;
}
