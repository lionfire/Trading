namespace LionFire.Trading.Automation;

public class BacktestExecutionOptions
{
    /// <summary>
    /// Backtest optimization: use short Chunks instead of long
    /// </summary>
    public bool ShortChunks { get; }

    //public bool PreloadNextChunk { get; set; } = true; // TOOPTIMIZE - implement this. probably don't need the option.

    #region CPU

    //public int BacktestsPerThread { get; set; } = 2;

    /// <summary>
    /// 0: One per thread
    /// -1: One per thread, 
    /// </summary>
    public int Threads { get; set; } = 0;

    /// <summary>
    /// 0: no max, can use all threads (typically 2 per core)
    /// 1: 1 thread per core
    /// -1: All but one: i.e. 1 thread per core for cores that support 2 threads
    /// </summary>
    public int MaxThreadsPerCore { get; set; } = 0;

    /// <summary>
    /// For CPUs that have both performance and efficiency cores, use performance cores
    /// </summary>
    public bool UsePerformanceCores { get; set; } = true;

    /// <summary>
    /// For CPUs that have both performance and efficiency cores, use efficiency cores
    /// </summary>
    public bool UseEfficiencyCores { get; set; } = true;

    /// <summary>
    /// If null, place no restrictions on which VCores (CPU threads) may be used.
    /// If not null, only these VCores can be used.
    /// </summary>
    public List<int>? VCoreIdWhitelist { get; set; }

    #endregion

    #region Memory

    #endregion
}
