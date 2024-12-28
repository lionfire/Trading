using DynamicData;
using DynamicData.Binding;
using LionFire.ExtensionMethods.Copying;
using LionFire.Instantiating;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Optimization.Strategies;
using LionFire.Trading.Journal;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LionFire.Trading.Automation.Optimization;

// ENH: Validatable, make all properties mutable and not required in ctor.  (Or consider a new pattern: a pair of classes, one frozen and one mutable.)
public partial class POptimization : ReactiveObject
{
    #region Identity Parameters

    [Reactive]
    private Type _pBotType;

    //public List<Type> BotTypes { get; set; } // ENH maybe someday though probably not, just a thought: OPTIMIZE - Test multiple bot types in parallel

    #endregion

    #region Lifecycle

    CompositeDisposable disposables = new();

    public POptimization(PMultiBacktestContext parent)
    {
        Parent = parent;

        var minParameterPriorityChanged = this.WhenAnyValue(x => x.MinParameterPriority);

        this.WhenAnyValue(x => x.Parent.CommonBacktestParameters.PBotType)
            .Subscribe(t => PBotType = t).DisposeWith(disposables);

        parameters.Connect()
            .AutoRefreshOnObservable(_ => minParameterPriorityChanged)
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.EnableOptimization))
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.IsEligibleForOptimization))
            .Transform(poo => (poo, EffectiveEnableOptimization(poo)), true)
            .Filter(tuple => tuple.Item2)
            .Transform(tuple => tuple.poo)
            .Bind(optimizableParameters)
            .Subscribe()
            .DisposeWith(disposables);

        parameters.Connect()
            .AutoRefreshOnObservable(_ => minParameterPriorityChanged)
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.EnableOptimization))
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.IsEligibleForOptimization))
            .Transform(poo => (poo, EffectiveEnableOptimization(poo)), true)
            .Filter(tuple => !tuple.Item2)
            .Transform(tuple => tuple.poo)
            .Bind(unoptimizableParameters)
            .Subscribe()
            .DisposeWith(disposables);


        this.WhenAnyValue(x => x.PBotType)
            .Select(BotParameterPropertiesInfo.SafeGet)
            .Subscribe(properties =>
            {
                parameters.Edit(u =>
                {
                    u.Clear();
                    if (properties != null)
                    {
                        u.AddOrUpdate(properties.PathDictionary.Values
                            .Where(info => info.IsOptimizable
                                && info.LastPropertyInfo!.PropertyType != typeof(bool) // NOTIMPLEMENTED yet
                            )
                            .Select(info =>
                            {
                                var poo = CreatePoo(info);
                                poo.SomethingChanged.Subscribe(_ => RaiseParametersChanged()).DisposeWith(disposables);
                                return poo;
                            }));
                    }
                    this.RaisePropertyChanged(nameof(Parameters));
                });
            })
            .DisposeWith(disposables);

        // Prior logic, not sure where it goes now:
        //        .PathDictionary
        //            .OrderByDescending(kvp => kvp.Value.options.OptimizeOrder)
        //            .ThenBy(kvp => kvp.Value.info.OptimizeOrderTiebreaker)
        //            .ThenBy(kvp => kvp.Key)
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

#if UNUSED // Reconsider both of these
    public int MinLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   
    public int MaxLevelOfDetail { get; set; } = 3; // TEMP, default can be higher   
#endif

    #region Individual Parameters

    public IObservable<Unit> ParametersChanged => parametersChanged;
    private Subject<Unit> parametersChanged = new();
    public void RaiseParametersChanged() => parametersChanged.OnNext(Unit.Default);

    /// <summary>
    /// Optimize parameters with an OptimizePriority greater than or equal to this value.
    /// This is only a default starting point: individual parameters can be enabled or disabled to override this.
    /// </summary>
    public int MinParameterPriority
    {
        get => minParameterPriority;
        set
        {
            minParameterPriority = value;
            levelsOfDetail = null;
            this.RaiseAndSetIfChanged(ref minParameterPriority, value);
        }
    }
    private int minParameterPriority;
    public int InverseMinParameterPriority { get => -MinParameterPriority; set => MinParameterPriority = -value; }

    //public IParameterOptimizationOptions? DefaultParameterOptimizationOptions { get; set; }

    // Key: ParameterType
    public Dictionary<string, IParameterOptimizationOptions>? ParameterOptimizationOptions { get; set; }
    //public required List<IPParameterOptimization> ParameterRanges { get; set; }

    public IObservableCache<IParameterOptimizationOptions, string> Parameters => parameters;
    private SourceCache<IParameterOptimizationOptions, string> parameters = new(poo => poo.Path);
    public IObservableCollection<IParameterOptimizationOptions> OptimizableParameters => optimizableParameters;
    private ObservableCollectionExtended<IParameterOptimizationOptions> optimizableParameters = new();
    public IObservableCollection<IParameterOptimizationOptions> UnoptimizableParameters => unoptimizableParameters;
    private ObservableCollectionExtended<IParameterOptimizationOptions> unoptimizableParameters = new();

    #endregion

    #region Journal

    public TradeJournalOptions TradeJournalOptions { get => tradeJournalOptions ??= new(); set => tradeJournalOptions = value; }

    private TradeJournalOptions? tradeJournalOptions;

    #endregion

    /// <summary>
    /// Initialize ParameterOptimizationOptions if needed, from various sources, in the following order:
    /// - ParameterAttribute
    /// //- EnableOptimization from this.MinParameterPriority
    /// //- POptimizationStrategy.Parameters
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static IParameterOptimizationOptions CreatePoo(HierarchicalPropertyInfo info)
    {
        //ParameterOptimizationOptions ??= new();

        //var fromPOptimization = ParameterOptimizationOptions.TryGetValue(info.Path)
        //?? ParameterOptimizationOptions?.TryGetValue(info.Key)
        ;
        //if (fromPOptimization != null)
        //{
        //    return fromPOptimization;
        //}

        IParameterOptimizationOptions parameterOptimizationOptions = LionFire.Trading.ParameterOptimizationOptions.Create(info);
        

        //ParameterOptimizationOptions.Create(info.ValueType, "<AttributePrototype>");

        AssignFromExtensions.AssignNonDefaultPropertiesFrom(parameterOptimizationOptions!, info.ParameterAttribute);

        #region Attribute

        //IParameterOptimizationOptions fromAttribute = info.ParameterAttribute.GetParameterOptimizationOptions(info.LastPropertyInfo!.PropertyType);
        //ArgumentNullException.ThrowIfNull(fromAttribute);

        //var clone = fromAttribute.Clone();

        #endregion

        //if (!clone.EnableOptimization.HasValue)
        //{
        //    clone.EnableOptimization = info.ParameterAttribute.OptimizePriorityInt >= MinParameterPriority;
        //}

        #region POptimizationStrategy

        //IParameterOptimizationOptions? fromOptimizationParameters = POptimizationStrategy.Parameters.TryGetValue(info.Path);

        //// FUTURE: Clone per-strategy options somehow 
        ////clone.FitnessOfInterest ??= gridSearchStrategy.Parameters.FitnessOfInterest;

        //if (fromOptimizationParameters != null)
        //{
        //    AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromOptimizationParameters);
        //}

        #endregion

        #region ParameterOptimizationOptions

        //clone.Path = info.Path;
        //ParameterOptimizationOptions.TryAdd(info.Path, clone);

        //var fromPOptimization = ParameterOptimizationOptions?.TryGetValue(info.Path) ?? ParameterOptimizationOptions?.TryGetValue(info.Key);
        //if (fromPOptimization != null)
        //{
        //    AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromPOptimization);
        //}

        #endregion

        return parameterOptimizationOptions;
        //return ParameterOptimizationOptions[info.Path];
    }

    public IEnumerable<int> LevelsOfDetailRange => Enumerable.Range(LevelsOfDetail.MinLevel, 0); // FUTURE: Levels above 0
    public IEnumerable<ILevelOfDetail> LevelsOfDetailEnumeration => Enumerable.Range(LevelsOfDetail.MinLevel, 1 - LevelsOfDetail.MinLevel).Select(level => LevelsOfDetail.GetLevel(level));

    public void OnLevelsOfDetailChanged()
    {
        levelsOfDetail = null;
        ((IReactiveObject)this).RaisePropertyChanged(nameof(LevelsOfDetailEnumeration));
        ((IReactiveObject)this).RaisePropertyChanged(nameof(LevelsOfDetailRange));
        ((IReactiveObject)this).RaisePropertyChanged(nameof(LevelsOfDetail));
    }

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

    public bool EffectiveEnableOptimization(IParameterOptimizationOptions options)
    {
        if (!options.IsEligibleForOptimization) return false;
        if (options.EnableOptimization == false) return false;

        return options.Info.ParameterAttribute.OptimizePriorityInt >= MinParameterPriority;
    }

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


