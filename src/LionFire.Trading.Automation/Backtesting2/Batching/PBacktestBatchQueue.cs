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
    /// Not counting the job that is currently running.  Set to 0 to force one at a time.
    /// </summary>
    public int MaxQueuedJobs { get; set; } = int.MaxValue;
    public int MaxConcurrentJobs => 1;

    #endregion

    public BacktestExecutionOptions BacktestExecutionOptions { get; set; } = new();


    public bool AutoStart { get;set; } = true;
}
