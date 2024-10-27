namespace LionFire.Trading.Automation.Optimization;

public class POptimization
{
    /// <summary>
    /// True: Test the entire parameter space at regular intervals (as defined by steps).
    /// False: Do a coarse test of entire parameter space, and then do a fine test of the most promising areas.
    /// </summary>
    public bool IsComprehensive { get; set; }

    /// <summary>
    /// (TODO - NOTIMPLEMENTED) For non-comprehensive tests, this sets the parameters for the initial coarse test.
    /// </summary>
    public int SearchSeed { get; set; }

    /// <summary>
    /// Skip backtests that would alter parameters by a sensitivity amount less than this.  
    /// Set to 0 for an exhaustive test.
    /// </summary>
    /// <remarks>
    /// NOT IMPLEMENTED - how would this actually be calculated? 
    /// </remarks>
    public float SensitivityThreshold { get; set; }

    public required List<IPParameterOptimization> ParameterRanges { get; set; }

    public required Type BotParametersType { get; set; }
    //public List<Type> BotTypes { get; set; } // ENH probably not: OPTIMIZE - Test multiple bot types in parallel

    /// <summary>
    /// If default, it will use the unkeyed Singleton for BacktestBatchQueue
    /// </summary>
    public object? BacktestBatcherName { get; set; }

    public double GranularityStepMultiplier { get; set; }

    public long MaxBacktests { get; set; } = 1_000;

    public required PBacktestBatchTask2 CommonBacktestParameters { get; set; }
    public int MaxBatchSize { get; set; } = 32;

    public int MinLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   
    public int MaxLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   

    public IParameterOptimizationOptions? DefaultParameterOptimizationOptions { get; set; }

    // Key: ParameterType
    public Dictionary<string, IParameterOptimizationOptions>? ParameterOptimizationOptions { get; set; }
    public int MaxDetailedJournals { get; set; }
}

//public enum ParameterType
//{
//    Unspecified = 0,
//    Period = 1 << 0, 
//    Enum = 1 << 1,
//    Bool = 1 << 2,
//}


