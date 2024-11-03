using CryptoExchange.Net.CommonObjects;
using LionFire.ExtensionMethods.Copying;
using LionFire.Serialization.Csv;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private readonly GridSearchStrategy gridSearchStrategy;

    #endregion

    #region Parameters

    #region (Derived)

    // OPTIMIZE: Make immutable array
    // NOTE: at certain levels, optimizableParameters may only have a single value and not be optimizable at that level
    public readonly List<(HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> optimizableParameters = new();
    public readonly List<(HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> unoptimizableParameters = new();

    #endregion

    #endregion

    #region Lifecycle

    public GridSearchState(GridSearchStrategy gridSearchStrategy)
    {
        this.gridSearchStrategy = gridSearchStrategy;

        var x = gridSearchStrategy.OptimizationParameters.EnableParametersAtOrAboveOptimizePriority;


        foreach (var kvp in BotParameterPropertiesInfo.Get(gridSearchStrategy.OptimizationParameters.PBotType)
                .PathDictionary
                    .Where(kvp => kvp.Value.IsOptimizable
                        && kvp.Value.LastPropertyInfo!.PropertyType != typeof(bool) // NOTIMPLEMENTED yet
                        )
                    .Select(kvp
                    => new KeyValuePair<string, (HierarchicalPropertyInfo info, IParameterOptimizationOptions options)>(kvp.Key,
                        (info: kvp.Value,
                         options: GetEffectiveOptions(kvp.Value,
                                        kvp.Value.ParameterAttribute.GetParameterOptimizationOptions(kvp.Value.LastPropertyInfo!.PropertyType),
                                        gridSearchStrategy.Parameters.Parameters.TryGetValue(kvp.Key),
                                        gridSearchStrategy.OptimizationParameters))))
                    .OrderByDescending(kvp => kvp.Value.options.OptimizeOrder)
                    .ThenBy(kvp => kvp.Value.info.OptimizeOrderTiebreaker)
                    .ThenBy(kvp => kvp.Key)
            )
        {
            if (kvp.Value.options.IsEligibleForOptimization
                && (kvp.Value.options.EnableOptimization == true
                || kvp.Value.info.ParameterAttribute.OptimizePriorityInt >= gridSearchStrategy.OptimizationParameters.EnableParametersAtOrAboveOptimizePriority
                )
                )
            { optimizableParameters.Add(kvp.Value); }
            else { unoptimizableParameters.Add(kvp.Value); }
        }

        if (optimizableParameters.Count == 0) throw new ArgumentException("No parameters to optimize");

        zero = new LevelOfDetail(0, this);

        while (CurrentLevel.TestPermutationCount > gridSearchStrategy.OptimizationParameters.MaxBacktests)
        {
            currentLevel--;
        }
    }


    private IParameterOptimizationOptions GetEffectiveOptions(HierarchicalPropertyInfo info, IParameterOptimizationOptions fromAttribute, IParameterOptimizationOptions? fromOptimizationParameters, POptimization pOptimization)
    {
        ArgumentNullException.ThrowIfNull(fromAttribute);

        var clone = fromAttribute.Clone();

        clone.FitnessOfInterest ??= gridSearchStrategy.Parameters.FitnessOfInterest;

        if (fromOptimizationParameters != null)
        {
            AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromOptimizationParameters);
        }

        var fromPOptimization = pOptimization.ParameterOptimizationOptions?.TryGetValue(info.Path) ?? pOptimization.ParameterOptimizationOptions?.TryGetValue(info.Key);
        if (fromPOptimization != null)
        {
            AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromPOptimization);
        }

        return clone;
    }

    #endregion

    #region State

    #region CurrentLevel

    public LevelOfDetail CurrentLevel => currentLevel == 0 ? zero : GetLevel(currentLevel);
    public int CurrentLevelIndex => currentLevel;
    int currentLevel = 0;

    #endregion

    #region Levels

    public LevelOfDetail GetLevel(int level)
    {
        if (levels == null)
        {
            levels = new();
            levels.Add(0, zero);
        }
        if (!levels.TryGetValue(level, out var result))
        {
            result = new LevelOfDetail(level, this);
            levels.Add(level, result);
        }
        return result;
    }
    public SortedList<int /* level */, LevelOfDetail>? levels = null;
    LevelOfDetail zero;

    public IEnumerable<LevelOfDetail> LevelsOfDetail => levels?.Values ?? [zero];

    #endregion

    #endregion

}

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
