namespace LionFire.Trading.Automation.Optimization;

// ENH: Validatable, make all properties mutable and not required in ctor.  (Or consider a new pattern: a pair of classes, one frozen and one mutable.)
public class POptimization
{
    #region Identity Parameters

    public ExchangeSymbol ExchangeSymbol { get; set; }
    public Type PBotType { get; }

    //public List<Type> BotTypes { get; set; } // ENH maybe someday though probably not, just a thought: OPTIMIZE - Test multiple bot types in parallel

    #endregion

    #region Lifecycle

    public POptimization(Type pBotType, ExchangeSymbol exchangeSymbol)
    {
        PBotType = pBotType;
        ExchangeSymbol = exchangeSymbol;
    }

    #endregion

    #region Execution options

    /// <summary>
    /// Key for Keyed BacktestQueue.  If default, it will use the unkeyed Singleton.
    /// </summary>
    public object? BacktestBatcherName { get; set; }

    public int MaxBatchSize { get; set; } = 32; // ENH - autotune this, and allow user to specify max memory usage

    #endregion

    /// <summary>
    /// True: Test the entire parameter space at regular intervals (as defined by steps).
    /// False: Do a coarse test of entire parameter space, and then do a fine test of the most promising areas.
    /// </summary>
    public bool IsComprehensive { get; set; }

    /// <summary>
    /// Skip backtests that would alter parameters by a sensitivity amount less than this.  
    /// Set to 0 for an exhaustive test.
    /// </summary>
    /// <remarks>
    /// NOT IMPLEMENTED - how would this actually be calculated? 
    /// </remarks>
    public float SensitivityThreshold { get; set; }

    public double GranularityStepMultiplier { get; set; }

    #region Optimization

    public long MaxBacktests { get; set; } = 1_000;

    /// <summary>
    /// (ENH - maybe - NOTIMPLEMENTED) For non-comprehensive tests that have a randomization element, this sets the parameters for the initial coarse test.   I don't like this idea.
    /// </summary>
    //public int SearchSeed { get; set; }

    #endregion

    public required PBacktestBatchTask2 CommonBacktestParameters { get; set; }

    public int MinLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   
    public int MaxLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   

    #region Individual Parameters

    public int EnableParametersAtOrAboveOptimizePriority { get; set; }

    //public IParameterOptimizationOptions? DefaultParameterOptimizationOptions { get; set; }

    // Key: ParameterType
    public Dictionary<string, IParameterOptimizationOptions>? ParameterOptimizationOptions { get; set; }
    //public required List<IPParameterOptimization> ParameterRanges { get; set; }

    #endregion

    #region Journal

    public int MaxDetailedJournals { get; set; }
    
    #endregion

}

//public enum ParameterType
//{
//    Unspecified = 0,
//    Period = 1 << 0, 
//    Enum = 1 << 1,
//    Bool = 1 << 2,
//}


