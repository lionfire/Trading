using LionFire.ExtensionMethods.Copying;
using LionFire.Trading.Automation.Optimization.Strategies;
using LionFire.Trading.Journal;

namespace LionFire.Trading.Automation.Optimization;

// ENH: Validatable, make all properties mutable and not required in ctor.  (Or consider a new pattern: a pair of classes, one frozen and one mutable.)
public class POptimization
{
    #region Identity Parameters

    public Type PBotType => Parent.CommonBacktestParameters.PBotType;

    //public List<Type> BotTypes { get; set; } // ENH maybe someday though probably not, just a thought: OPTIMIZE - Test multiple bot types in parallel

    #endregion

    #region Lifecycle

    public POptimization(PMultiBacktestContext parent)
    {
        Parent = parent;
    }
    //public POptimization(PMultiBacktestContext parent, Type pBotType, ExchangeSymbol exchangeSymbol) : this(parent)
    //{
    //    PBotType = pBotType;
    //    ExchangeSymbol = exchangeSymbol;
    //}

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

    public IPOptimizationStrategy POptimizationStrategy { get; set; } = new PGridSearchStrategy();

    public long MaxBacktests { get; set; } = 1_000;
    public long MaxSearchBacktests { get; set; } = 1_000_000;
    public long MaxScanBacktests { get; set; } = 1_000_000;

    /// <summary>
    /// (ENH - maybe - NOTIMPLEMENTED) For non-comprehensive tests that have a randomization element, this sets the parameters for the initial coarse test.   I don't like this idea.
    /// </summary>
    //public int SearchSeed { get; set; }

    #endregion

    public PBacktestBatchTask2 CommonBacktestParameters => Parent.CommonBacktestParameters;

    public int MinLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   
    public int MaxLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   

    #region Individual Parameters

    /// <summary>
    /// Optimize parameters with an OptimizePriority greater than or equal to this value.
    /// This is only a default starting point: individual parameters can be enabled or disabled to override this.
    /// </summary>
    public int MinParameterPriority
    {
        get => minParameterPriority; set
        {
            minParameterPriority = value;
            levelsOfDetail = null;
        }
    }
    private int minParameterPriority;

    //public IParameterOptimizationOptions? DefaultParameterOptimizationOptions { get; set; }

    // Key: ParameterType
    public Dictionary<string, IParameterOptimizationOptions>? ParameterOptimizationOptions { get; set; }
    //public required List<IPParameterOptimization> ParameterRanges { get; set; }

    #endregion

    #region Journal

    public TradeJournalOptions TradeJournalOptions { get => tradeJournalOptions ??= new(); set => tradeJournalOptions = value; }

    private TradeJournalOptions? tradeJournalOptions;

    #endregion

    /// <summary>
    /// Compile parameter optimization options from various sources, in the following order:
    /// - ParameterAttribute
    /// - EnableOptimization from this.MinParameterPriority
    /// - POptimizationStrategy.Parameters
    /// - this.ParameterOptimizationOptions
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public IParameterOptimizationOptions GetEffectiveOptions2(HierarchicalPropertyInfo info)
    {
        #region Attribute

        IParameterOptimizationOptions fromAttribute = info.ParameterAttribute.GetParameterOptimizationOptions(info.LastPropertyInfo!.PropertyType);
        ArgumentNullException.ThrowIfNull(fromAttribute);

        var clone = fromAttribute.Clone();

        #endregion

        if (!clone.EnableOptimization.HasValue)
        {
            clone.EnableOptimization = info.ParameterAttribute.OptimizePriorityInt >= MinParameterPriority;
        }

        #region POptimizationStrategy

        IParameterOptimizationOptions? fromOptimizationParameters = POptimizationStrategy.Parameters.TryGetValue(info.Path);

        // FUTURE: Clone per-strategy options somehow 
        //clone.FitnessOfInterest ??= gridSearchStrategy.Parameters.FitnessOfInterest;

        if (fromOptimizationParameters != null)
        {
            AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromOptimizationParameters);
        }

        #endregion

        #region ParameterOptimizationOptions

        var fromPOptimization = ParameterOptimizationOptions?.TryGetValue(info.Path) ?? ParameterOptimizationOptions?.TryGetValue(info.Key);
        if (fromPOptimization != null)
        {
            AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromPOptimization);
        }

        #endregion

        return clone;
    }

    public IEnumerable<int> LevelsOfDetailRange => Enumerable.Range(LevelsOfDetail.MinLevel, 0); // FUTURE: Levels above 0
    public IEnumerable<ILevelOfDetail> LevelsOfDetailEnumeration => Enumerable.Range(LevelsOfDetail.MinLevel, 1 - LevelsOfDetail.MinLevel).Select(level => LevelsOfDetail.GetLevel(level));

    public OptimizerLevelsOfDetail LevelsOfDetail
    {
        get
        {
            if (levelsOfDetail == null)
            {
                levelsOfDetail = new(this);
            }
            return levelsOfDetail;
        }
        set
        {
            levelsOfDetail = value;
        }
    }

    public PMultiBacktestContext Parent { get; }

    private OptimizerLevelsOfDetail? levelsOfDetail;

    #region Misc

    public override string ToString() => this.ToXamlProperties();

    #endregion
}

//public enum ParameterType
//{
//    Unspecified = 0,
//    Period = 1 << 0, 
//    Enum = 1 << 1,
//    Bool = 1 << 2,
//}


