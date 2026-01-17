using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Optimization.Strategies;
using LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reflection.Emit;
using static LionFire.Trading.Automation.Optimization.Strategies.GridSpaces.GridSearchState;

namespace LionFire.Trading.Automation.Optimization;

public interface ILevelOfDetail
{
    IReadOnlyList<IParameterLevelOfDetailInfo> Parameters { get; }
    double TestPermutationCount { get; }
    int Level { get; }
}

public partial class OptimizerLevelsOfDetail : ReactiveObject, IDisposable
{
    #region Parameters

    #region (Derived)

    public POptimization POptimization { get; }
    public IEnumerable<IParameterOptimizationOptions> Optimizable { get; }

    /// <summary>
    /// Everything up to and including level 0
    /// </summary>
    public double ComprehensiveScanTotal => GetLevel(0).TestPermutationCount;

    /// <summary>
    /// Total of all levels up to 0 until the Max backtests limit is reached 
    /// </summary>
    public long PlannedScanTotal => (long)GetLevel(MinLevel).TestPermutationCount;

    public double ComprehensiveScanPerUn => ComprehensiveScanTotal == 0 ? 0 : (PlannedScanTotal / ComprehensiveScanTotal);

    //Math.Min(POptimization.MaxScanBacktests, Math.Min(POptimization.MaxBacktests, (long)ScanLevels.Select(l => l.TestPermutationCount).Sum()));

    #endregion

    #endregion

    CompositeDisposable? disposables = new();
    public OptimizerLevelsOfDetail(POptimization pOptimization, IEnumerable<IParameterOptimizationOptions> optimizable)
    {
        POptimization = pOptimization;
        Optimizable = optimizable;
        currentLevel = MinLevel;
        //Init();
    }

#if OLD
    void Init() // TODO: Make this class immutable, with no reset. Make a new one instead.
    {
            //Levels = null;
            //zero = null;
#if OLD

        bool firstRun;
        if (disposables != null)
        {
            firstRun = false;
            disposables?.Dispose();
        }
        else
        {
            firstRun = true;
        }
        disposables = new();

        //parameters.Clear();

        #region OptimizableParameters and UnoptimizableParameters

        foreach (var kvp in POptimization.Parameters.KeyValues)

        {
            if (firstRun)
            {
                kvp.Value.PropertyChanged += (s, e) =>
                {
                    Debug.WriteLine("PropertyChanged: " + e.PropertyName);
                };
                kvp.Value.WhenAny(x => x, _ => _.Value).Subscribe(_ =>
                {
                    Reset();
                    POptimization.OnLevelsOfDetailChanged();
                });

                kvp.Value.SomethingChanged.Subscribe(_ =>
                {
                    Reset();
                    POptimization.OnLevelsOfDetailChanged();
                });
                kvp.Value.WhenAny(x => x, x => x).Subscribe(x =>
                {
                    Reset();
                    POptimization.OnLevelsOfDetailChanged();
                }).DisposeWith(disposables);
                disposables.Add(kvp.Value.WhenAnyValue(x => x.MinValueObj).Subscribe(_ =>
                {
                    Reset();
                    POptimization.OnLevelsOfDetailChanged();
                }));
                disposables.Add(kvp.Value.Changed.Subscribe(_ =>
                {
                    Reset();
                    POptimization.OnLevelsOfDetailChanged();
                }));
                disposables.Add(kvp.Value.Changing.Subscribe(_ => // TEMP 
                {
                    Reset();
                    POptimization.OnLevelsOfDetailChanged();
                }));
            }

            //parameters.Add(kvp.Value);
            // OLD
            //if (kvp.Value.IsEligibleForOptimization
            //    && (kvp.Value.EnableOptimization == true
            //    || kvp.Value.info.ParameterAttribute.OptimizePriorityInt >= POptimization.MinParameterPriority
            //    )
            //    )
            //{ OptimizableParameters.Add(kvp.Value); }
            //else { UnoptimizableParameters.Add(kvp.Value); }
        }

        #endregion

#endif
    #region Levels of Detail

        currentLevel = MinLevel;

        //Logger.LogInformation("Level {level} has {count} backtests.", currentLevel, CurrentLevel.TestPermutationCount);

        #endregion

        //this.RaisePropertyChanged(nameof(LevelsOfDetail));
    }
#endif
    public void Dispose()
    {
        if (Levels != null)
        {
            foreach (var level in Levels.Values)
            {
                level.Dispose();
            }
        }
        zero?.Dispose();
        disposables?.Dispose();
    }

    #region State

    #region CurrentLevel

    public GridLevelOfDetail CurrentLevel => GetLevel(currentLevel);
    public int CurrentLevelIndex => currentLevel;
    int currentLevel = 0;

    #endregion

    #region Levels

    public int MinLevel
    {
        get
        {
            var result = 0;

            double priorTestPermutationCount = double.MaxValue;

            for (var level = GetLevel(result); level.TestPermutationCount > POptimization.MaxBacktests; level = GetLevel(--result))
            {
                if (level.TestPermutationCount >= priorTestPermutationCount)
                {
                    // Cannot reduce further - parameter space is at minimum granularity
                    // Accept this level as the minimum achievable, even if above MaxBacktests
                    Debug.WriteLine($"[OptimizerLevelsOfDetail] Reached minimum granularity at level {result} with {level.TestPermutationCount:N0} tests " +
                        $"(MaxBacktests: {POptimization.MaxBacktests:N0}). Cannot reduce further with {level.Parameters.Count} parameters.");
                    return result;
                }
                priorTestPermutationCount = level.TestPermutationCount;
                //if(result < -10_000) { throw new InvalidOperationException("Possible loop detected.  Too many levels of detail being generated."); }
                //Debug.WriteLine($"[OptimizerLevelsOfDetail] Level {result} has {level.TestPermutationCount} backtests but max is {POptimization.MaxBacktests}, so going down a level of detail");
            }
            return result;
        }
    }
    //=> levels?.Count > 0 != true ? 0 : levels.First().Key;

    public GridLevelOfDetail GetLevel(int level)
    {
        if (level == 0) return zero ??= new GridLevelOfDetail(0, this);
        if (Levels == null)
        {
            Levels = new();
            Levels.Add(0, zero);
        }
        if (!Levels.TryGetValue(level, out var result))
        {
            result = new GridLevelOfDetail(level, this);
            Levels.Add(level, result);
        }
        return result;
    }



    //public SortedList<int /* level */, GridLevelOfDetail>? Levels = null;
    [Reactive]
    private SortedList<int /* level */, GridLevelOfDetail>? _levels;
    GridLevelOfDetail? zero;



    public IEnumerable<GridLevelOfDetail> LevelsOfDetail => Levels?.Values ?? [zero];
    public IEnumerable<GridLevelOfDetail> ScanLevels => Levels?.Where(l => l.Key <= 0).Select(l => l.Value) ?? [GetLevel(0)];
    public IEnumerable<GridLevelOfDetail> SearchLevels => Levels?.Where(l => l.Key > 0).Select(l => l.Value) ?? [];


    #endregion

    #endregion
}


#if UNUSED
public class LevelOfDetail<T>
{
    private BitArray completed;

    private BitArray shouldExplore;

}

internal class OptimizationResult
{
    public required int[] Parameters;
    public double Fitness;

    /// <remarks>
    /// From Claude 3.5 Sonnet
    /// 
    /// Starting value (17):
    ///    The hash is initialized with a non-zero value, often a prime number.
    ///    17 is commonly used as this initial value.
    ///    Starting with a non-zero value helps to better distribute hash codes, especially for small or empty collections.
    ///
    ///Multiplier (31):
    ///
    /// In the loop, the current hash is multiplied by 31 before adding the next element's hash code.
    /// 31 is used because:
    ///    a) It's an odd prime number, which is good for distribution in hash functions.
    ///    b) It has a convenient property: 31 * i == (i << 5) - i.This means the multiplication can be optimized by the compiler into a bitwise shift and a subtraction, which is typically faster than multiplication on many processors.
    ///
    /// These specific numbers(17 and 31) are not magical or mandatory, but they are commonly used in hash code implementations across various programming languages and libraries.They provide a good balance of simplicity, performance, and hash distribution.
    /// </remarks>
    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var item in Parameters)
        {
            hash = hash * 31 + item.GetHashCode();
        }
        return hash;
    }
}
#endif