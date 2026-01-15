using DynamicData;
using DynamicData.Binding;
using LionFire.ExtensionMethods.Copying;
using LionFire.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;

/// <summary>
/// Level 0 is the minimum resolution of testing in order for the optimization to be considered complete.
/// 
/// Negative levels 
/// - (ENH) can be used for preview purposes, and to optimize promising areas first
/// - (ENH) can be used if level 0 would result in more tests than the options allow.  (A note will be made in optimization results to indicate a full grid search wasn't completed, and ideally say what the max gaps were in testing.)
/// 
/// </summary>
public partial class GridSearchState
{
    #region Relationships

    public GridSearchStrategy GridSearchStrategy { get; }

    #region Derived

    public POptimization POptimization => GridSearchStrategy.OptimizationParameters;
    private ILogger Logger => GridSearchStrategy.Logger;

    #endregion

    #endregion

    #region TEMP - convenience

    //public IReadOnlyList<(HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> PMultiSim => GridSearchStrategy.OptimizationParameters.PMultiSim;

    public IObservableCollection<IParameterOptimizationOptions > OptimizableParameters => GridSearchStrategy.OptimizationParameters.OptimizableParameters;

    public IObservableCollection<IParameterOptimizationOptions> UnoptimizableParameters => GridSearchStrategy.OptimizationParameters.UnoptimizableParameters;

    #endregion

    #region Lifecycle

    public GridSearchState(GridSearchStrategy gridSearchStrategy)
    {
        GridSearchStrategy = gridSearchStrategy;

        if (OptimizableParameters.Count == 0)
        {
            Logger.LogInformation("No parameters to optimize");
        }
        else
        {
            //while (CurrentLevel.TestPermutationCount > gridSearchStrategy.OptimizationParameters.MaxBacktests)
            while (CurrentLevel.Level > proxy.MinLevel)
            {
                Logger.LogInformation("Level {level} has {count} backtests but max is {maxBacktests}, so going down a level of detail", currentLevel, CurrentLevel.TestPermutationCount, gridSearchStrategy.OptimizationParameters.MaxBacktests);
                currentLevel--;
            }
        }
        Logger.LogInformation("Level {level} has {count} backtests.", currentLevel, CurrentLevel.TestPermutationCount);
    }

    #endregion

    #region State

    #region CurrentLevel

    public GridLevelOfDetail CurrentLevel => GetLevel(currentLevel);
    public int CurrentLevelIndex => currentLevel;
    int currentLevel = 0;

    #endregion

    #region Levels

    // TODO - cleanup this proxy - RENAME
    OptimizerLevelsOfDetail proxy => GridSearchStrategy.OptimizationParameters.LevelsOfDetail;
    public GridLevelOfDetail GetLevel(int level)=> proxy.GetLevel(level);
    //{
    //    if (level == 0) return zero ??= new LevelOfDetail(0, LevelsOfDetail2);
    //    if (levels == null)
    //    {
    //        levels = new();
    //        levels.Add(0, zero);
    //    }
    //    if (!levels.TryGetValue(level, out var result))
    //    {
    //        result = new LevelOfDetail(level, this);
    //        levels.Add(level, result);
    //    }
    //    return result;
    //}
    //public SortedList<int /* level */, LevelOfDetail>? levels = null;
    //LevelOfDetail zero => proxy.GetLevel(0);

    public IEnumerable<GridLevelOfDetail> LevelsOfDetail => proxy.LevelsOfDetail;// levels?.Values ?? [zero];

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