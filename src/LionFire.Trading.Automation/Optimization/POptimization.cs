﻿using DynamicData;
using DynamicData.Binding;
using LionFire.ExtensionMethods.Copying;
using LionFire.Instantiating;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Optimization.Strategies;
using LionFire.Trading.Journal;
using LionFire.Validation;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LionFire.Trading.Automation.Optimization;

public class POptimizationVM : ReactiveObject
{
    public POptimization POptimization { get; }

    public POptimizationVM(POptimization pOptimization)
    {
        POptimization = pOptimization;
    }

}

// ENH: Validatable, make all properties mutable and not required in ctor.  (Or consider a new pattern: a pair of classes, one frozen and one mutable.)
public partial class POptimization : ReactiveObject
    , IValidatable
{
    public ValidationContext ValidateThis(ValidationContext c)
        => c
            ;

    #region Identity Parameters

    #region Bot Type

    // FUTURE: allow it to be different than the MultiSim's PBotType

    //public Type? PBotType { get; set; } 
    //public List<Type> BotTypes { get; set; } // ENH maybe someday though probably not, just a thought: OPTIMIZE - Test multiple bot types in parallel
    
    #endregion

    #endregion

    #region Lifecycle

    CompositeDisposable disposables = new();

    public POptimization(PMultiSim parent)
    {
        PMultiSim = parent ?? throw new ArgumentNullException(nameof(parent));

        var minParameterPriorityChanged = this.WhenAnyValue(x => x.MinParameterPriority);

        // OLD
        //this.WhenAnyValue(x => x.Parent.PMultiSim.PBotType)
        //.Subscribe(t => PBotType = t).DisposeWith(disposables);

        parameters.Connect()
            .AutoRefreshOnObservable(_ => minParameterPriorityChanged)
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.EnableOptimization))
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.IsEligibleForOptimization))
            //.Throttle(TimeSpan.FromMilliseconds(75))
            .Transform(poo =>
            {
                Debug.WriteLine($"POptimization Rx bind for optimizableParameters.  Transforming: {poo.Info.Key} - {EffectiveEnableOptimization(poo)}");
                return (poo, EffectiveEnableOptimization(poo));
            }, true)
            .Filter(tuple => tuple.Item2)
            .Transform(tuple => tuple.poo)
            .SortBy(poo => -(poo.OptimizeOrder ?? 0))
            .SortBy(poo => poo.Info.OptimizeOrderTiebreaker)
            .SortBy(poo => poo.Info.Key)
            //        .PathDictionary
            //            .OrderByDescending(kvp => kvp.Value.options.OptimizeOrder)
            //            .ThenBy(kvp => kvp.Value.info.OptimizeOrderTiebreaker)
            //            .ThenBy(kvp => kvp.Key)
            .Bind(optimizableParameters)
            .Subscribe()
            .DisposeWith(disposables);

        parameters.Connect()
            .AutoRefreshOnObservable(_ => minParameterPriorityChanged)
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.EnableOptimization))
            .AutoRefreshOnObservable(x => x.WhenAnyValue(p => p.IsEligibleForOptimization))
            .Transform(poo =>
            {
                Debug.WriteLine($"POptimization Rx bind for optimizableParameters.  Transforming: {poo.Info.Key} - {EffectiveEnableOptimization(poo)}");
                return (poo, EffectiveEnableOptimization(poo));
            }, true)
            .Filter(tuple => !tuple.Item2)
            .Transform(tuple => tuple.poo)
            .Bind(unoptimizableParameters)
            .Subscribe()
            .DisposeWith(disposables);

        ParametersChanged
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(_ =>
        {
            Debug.WriteLine($"POptimization Rx bind: ParametersChanged => OnLevelsOfDetailChanged.");
            OnLevelsOfDetailInputsChanged();
        }).DisposeWith(disposables);

        //this.WhenAnyValue(x => x.OptimizableParameters)
        //       .Select(parameters => new OptimizerLevelsOfDetail(this, parameters))
        //       .ToProperty(this, x => x.LevelsOfDetail, out _levelsOfDetail)
        //       .DisposeWith(disposables);
        _levelsOfDetail = new(this, []);
        (optimizableParameters).CollectionChanged += (s, e) =>
        {
            InitLevelsOfDetail();
        };

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
            if (minParameterPriority == value) return;
            minParameterPriority = value;

            OnLevelsOfDetailInputsChanged();
            this.RaisePropertyChanged(nameof(MinParameterPriority));
            this.RaisePropertyChanged(nameof(InverseMinParameterPriority));
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

    public IEnumerable<int> LevelsOfDetailRange => Enumerable.Range(LevelsOfDetail.MinLevel, 0); // FUTURE: Levels above 0
    public IEnumerable<ILevelOfDetail> LevelsOfDetailEnumeration => Enumerable.Range(LevelsOfDetail.MinLevel, 1 - LevelsOfDetail.MinLevel).Select(level => LevelsOfDetail.GetLevel(level));

    private void InitLevelsOfDetail()
    {
        LevelsOfDetail = new OptimizerLevelsOfDetail(this, optimizableParameters
            .Where(p => p.IsEligibleForOptimization)
            );
    }

    public void OnLevelsOfDetailInputsChanged()
    {
        //Debug.WriteLine("POptimization.OnLevelsOfDetailChanged: levelsOfDetail invalidated");
        //levelsOfDetail?.Dispose();
        //levelsOfDetail = null;

        lock (_debounceLodLock)
        {
            if (debounceLod == null)
            {
                debounceLod = new Timer(_ =>
                {
                    Debug.WriteLine($"POptimization.OnLevelsOfDetailChanged: notifying. PMultiSim: {Parameters.Count}, Optimizable: {OptimizableParameters.Count}");
                    InitLevelsOfDetail();
                    ((IReactiveObject)this).RaisePropertyChanged(nameof(LevelsOfDetailEnumeration));
                    ((IReactiveObject)this).RaisePropertyChanged(nameof(LevelsOfDetailRange));
                    ((IReactiveObject)this).RaisePropertyChanged(nameof(LevelsOfDetail));

                    debounceLod?.Dispose();
                    debounceLod = null;
                }, null, 300, Timeout.Infinite);
            }
        }
    }
    Timer? debounceLod;
    object _debounceLodLock = new();

    #region Derived

    //public OptimizerLevelsOfDetail LevelsOfDetail => _levelsOfDetail.Value;
    //private readonly ObservableAsPropertyHelper<OptimizerLevelsOfDetail> _levelsOfDetail;
    [Reactive]
    private OptimizerLevelsOfDetail _levelsOfDetail;

    #endregion

    public PMultiSim PMultiSim { get; }

    public bool EffectiveEnableOptimization(IParameterOptimizationOptions options)
    {
        if (!options.IsEligibleForOptimization) return false;
        if (options.EnableOptimization == false) return false;

        return options.EnableOptimization == true || options.Info.ParameterAttribute.OptimizePriorityInt >= MinParameterPriority;
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


