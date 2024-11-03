using System.Linq;
using System.Collections;
using System.Diagnostics;
using System;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;


public static class ParameterLevelOfDetailInfo
{
    public static IParameterLevelOfDetailInfo Create(int level, HierarchicalPropertyInfo info, IParameterOptimizationOptions options)
    {
        var propertyType = info.LastPropertyInfo!.PropertyType;
        if (propertyType.IsEnum) propertyType = Enum.GetUnderlyingType(propertyType);

        return (IParameterLevelOfDetailInfo)Activator.CreateInstance(typeof(ParameterLevelOfDetailInfo<>).MakeGenericType(propertyType), level, info, options)!;
    }
}

[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(ParameterLevelOfDetailInfo<int>), typeDiscriminator: "int")]
[JsonDerivedType(typeof(ParameterLevelOfDetailInfo<uint>), typeDiscriminator: "uint")]
[JsonDerivedType(typeof(ParameterLevelOfDetailInfo<float>), typeDiscriminator: "float")]
[JsonDerivedType(typeof(ParameterLevelOfDetailInfo<double>), typeDiscriminator: "double")]
[JsonDerivedType(typeof(ParameterLevelOfDetailInfo<decimal>), typeDiscriminator: "decimal")]
public interface IParameterLevelOfDetailInfo
{
    string Key { get; }
    string ValueType { get; }
    int TestCount { get; }
    object GetValue(int index);
}

public class ParameterLevelOfDetailInfo<TValue> : IParameterLevelOfDetailInfo
     where TValue : struct, INumber<TValue>
{
    public string Key => Info.Key;
    
    [JsonIgnore] // Only for debugging
    public string ValueType => typeof(TValue).Name;

    public readonly HierarchicalPropertyInfo Info;
    private readonly ParameterOptimizationOptions<TValue> Options;

    public TValue Max => max;
    private readonly TValue max;
    public TValue Min => min;
    private readonly TValue min;
    private TValue range => max - min;
    public TValue Step => step;
    private readonly TValue step;
    public TValue Exponent => exponent;
    private readonly TValue exponent;
    public IEnumerable<TValue> Values
    {
        get
        {
            for (int i = 0; i < TestCount; i++)
            {
                yield return GetValue(i);
            }
        }
    }
    public double ExponentAdjustment => exponentAdjustment;
    private readonly double exponentAdjustment;
    public double LevelMultiplier => levelMultiplier;
    private readonly double levelMultiplier;

    public ParameterLevelOfDetailInfo(int level, HierarchicalPropertyInfo info, IParameterOptimizationOptions options)
    {
        if (!options.IsEligibleForOptimization) throw new ArgumentException(nameof(options.IsEligibleForOptimization));
        Info = info;
        Options = (ParameterOptimizationOptions<TValue>)options;
        max = Options.EffectiveMaxValue;
        min = Options.EffectiveMinValue;
        step = Options.EffectiveOptimizationStep;

        if (level > 0)
        {
            // We are doing a fine-grained optimization search.
            // This requires an additional input of specific promising areas to search (not implemented yet.)
            throw new NotImplementedException();
        }

        if ((info.ParameterAttribute.OptimizerHints.HasFlag(OptimizationDistributionKind.OrthogonalCategory)))
        {
            // Don't sample these. Always include them all, on every level.
            throw new NotImplementedException();
        }
        else
        {
            var dssc = info.ParameterAttribute.DefaultSearchSpacesCount;
            if (info.ParameterAttribute.IsCategory && dssc > 0)
            {
                var inverseLevel = -level;
                var maxSpaceIndex = dssc - 1;
                var dsscIndex = Math.Min(inverseLevel, maxSpaceIndex);
                var difference = inverseLevel - maxSpaceIndex;
                // ENH: Trim the remaining parameters if difference > 0

                TValue[] values = CategoryValues = (TValue[])info.ParameterAttribute.DefaultSearchSpaces![dsscIndex];
                testCount = values.Length;

                GetValue = i => values[i];

                #region Avoid serializing

                max = TValue.Zero;
                step = TValue.Zero;
                
                #endregion
            }
            else
            {
                if (Options.MinOptimizationValues.HasValue)
                {
                    testCount = (Options.MinOptimizationValues.Value + Options.MaxOptimizationValues) / 2;
                } else
                {
                    testCount = Options.MaxOptimizationValues;
                }
                //testCount = Options.MinOptimizationValues ?? throw new ArgumentNullException(nameof(Options.MinOptimizationValues));
                checked // REVIEW - is this necessary?
                {
                    if (Options.MaxProbes.HasValue)
                    {
                        testCount = Math.Min(testCount, (int)Options.MaxProbes.Value);
                    }
                    if (Options.MinProbes.HasValue)
                    {
                        testCount = Math.Max(testCount, (int)Options.MinProbes.Value);
                    }
                }
                if (Options.MinOptimizationValues.HasValue)
                {
                    testCount = Math.Max(testCount, Options.MinOptimizationValues.Value);
                }
                if(Options.MaxOptimizationValues > 0)
                {
                    testCount = Math.Min(testCount, Options.MaxOptimizationValues);
                }

                levelMultiplier = Math.Pow(2, -level);

                var divisor = Math.Pow(2, -level);
                var remainder = testCount % divisor;
                testCount = (int)double.Ceiling(testCount / divisor);

                // TODO: extra test at the end if it doesn't perfectly line up
                //if (divisor != 0) count++;

                //IsComplete = new BitArray(count);

                if (Options.DistributionParameter.HasValue && Options.DistributionParameter.Value != 1)
                {
                    Debug.WriteLine("UNTESTED: DistributionParameter != 1");

                    var rangeForLog = Math.Log((testCount - 1) * Convert.ToDouble(step));
                    exponentAdjustment = Convert.ToDouble(range) / rangeForLog;

                    GetValue = GetDistributionValue;
                }
                else
                {
                    GetValue = GetLinearValue;
                    exponentAdjustment = 0; // 0 means ignore this parameter (effective exponentAdjustment of 1)
                }

                if (levelMultiplier != 1)
                {
                    var copy = GetValue;
                    GetValue = i => copy((int)(i * levelMultiplier)); // TODO: fix artefacts?
                }
            }
        }
    }


    #region State

    //public readonly BitArray IsComplete;
    public int TestCount => testCount;
    private int testCount;

    private readonly TValue[]? CategoryValues; // UNUSED

    #endregion

    #region GetValue

    object IParameterLevelOfDetailInfo.GetValue(int index) => GetValue(index);
    //public TValue GetValue(int index) => GetValueFunc(index);
    public readonly Func<int, TValue> GetValue;

    public TValue GetLinearValue(int index) => min + TValue.CreateChecked(index) * step;
    public TValue GetDistributionValue(int index) => min
        + TValue.CreateChecked((Math.Log(
            index * Convert.ToDouble(step)))
            * exponentAdjustment);

    #endregion

    //public TValue Min { get; set; }
    //public TValue Max { get; set; }
}

