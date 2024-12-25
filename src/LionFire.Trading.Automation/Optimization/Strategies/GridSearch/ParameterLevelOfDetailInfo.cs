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
    ulong TestCount { get; }
    object GetValue(int index);
}

public class ParameterLevelOfDetailInfo<TValue> : IParameterLevelOfDetailInfo
     where TValue : struct, INumber<TValue>
{
    public string Key => Info.Key;

    [JsonIgnore] // Only for debugging
    public string ValueType => typeof(TValue).Name;

    private readonly int Level;
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
            for (ulong i = 0; i < TestCount; i++)
            {
                var result = GetValue((int)i);
                if (result > max) break;
                yield return result;
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
        this.Level = level;
        Info = info;
        Options = (ParameterOptimizationOptions<TValue>)options;
        max = Options.EffectiveMaxValue;
        min = Options.EffectiveMinValue;
        step = Options.EffectiveStep;

        if (level > 0)
        {
            // We are doing a fine-grained optimization search.
            // This requires an additional input of specific promising areas to search (not implemented yet.)
            throw new NotImplementedException();
        }

        #region testCount

        if ((Info.ParameterAttribute.OptimizerHints.HasFlag(OptimizationDistributionKind.OrthogonalCategory)))
        {
            // Don't sample these. Always include them all, on every level.
            throw new NotImplementedException();
        }
        else
        {
            var defaultSearchSpacesCount = Info.ParameterAttribute.DefaultSearchSpacesCount;
            if (Info.ParameterAttribute.IsCategory && defaultSearchSpacesCount > 0)
            {
                var inverseLevel = -Level;
                var maxSpaceIndex = defaultSearchSpacesCount - 1;
                var dsscIndex = Math.Min(inverseLevel, maxSpaceIndex);
                var difference = inverseLevel - maxSpaceIndex;
                // ENH: Trim the remaining parameters if difference > 0

                TValue[] values = CategoryValues = (TValue[])Info.ParameterAttribute.DefaultSearchSpaces![dsscIndex];
                testCount = (ulong)values.Length;

                GetValue = i => values[i];

                #region Avoid serializing

                max = TValue.Zero;
                step = TValue.Zero;

                #endregion
            }
            else
            {
                if (Options.EffectiveMinCount.HasValue)
                {
                    testCount = (Options.EffectiveMinCount.Value + Options.EffectiveMaxCount) / 2;
                }
                else
                {
                    testCount = Options.EffectiveMaxCount;
                }

                #region Enforce hard min/max limits. Min takes precedence.

                checked
                {
                    if (Options.MaxCount.HasValue)
                    {
                        testCount = Math.Min(testCount, Options.MaxCount.Value);
                    }
                    if (Options.MinCount.HasValue)
                    {
                        testCount = Math.Max(testCount, Options.MinCount.Value);
                    }
                }

                #endregion


                #region Enforce Effective min/max limits

                if (Options.EffectiveMinCount.HasValue)
                {
                    testCount = Math.Max(testCount, Options.EffectiveMinCount.Value);
                }
                if (Options.EffectiveMaxCount > 0)
                {
                    testCount = Math.Min(testCount, Options.EffectiveMaxCount);
                }

                #endregion

                levelMultiplier = Math.Pow(2, -Level);

                var divisor = Math.Pow(2, -Level);
                var remainder = testCount % divisor;
                //step = (Max - Min) / TValue.CreateChecked(testCount); 
                testCount = (ulong)double.Ceiling(testCount / divisor);

                step = (Max - Min) / TValue.CreateChecked(testCount);

                if (!IsFloatingPoint<TValue>())
                {
                    step += TValue.CreateChecked(1.0);
                }

                // TODO: extra test at the end if it doesn't perfectly line up
                //if (divisor != 0) count++;

                //IsComplete = new BitArray(count);

                if (Options.DistributionParameter.HasValue && Options.DistributionParameter.Value != 1)
                {
                    Debug.WriteLine("UNTESTED: DistributionParameter != 1");

                    var rangeForLog = Math.Log((testCount - 1) * Convert.ToDouble(step));
                    exponentAdjustment = Convert.ToDouble(range) / rangeForLog;

                    GetValue = GetDistributionValue;
                    testCount = (ulong)(GetLastDistributionValue(max) + 1);
                }
                else
                {
                    GetValue = GetLinearValue;
                    exponentAdjustment = 0; // 0 means ignore this parameter (effective exponentAdjustment of 1)
                }

                //if (levelMultiplier != 1)
                //{
                //    var copy = GetValue;
                //    GetValue = i => copy((int)(i * levelMultiplier)); // TODO: fix artefacts?
                //}
            }
        }

        #endregion

    }
    static bool IsFloatingPoint<T>()
    {
        return 
            typeof(T) == typeof(float)
            || typeof(T) == typeof(double)
            || typeof(T) == typeof(decimal);
    }

    #region State

    //public readonly BitArray IsComplete;
    public ulong TestCount => testCount;
    private ulong testCount;

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
    public int GetLastDistributionValue(TValue max)
    {
        for (int index = 0; index < int.MaxValue; index++)
        {
            if (GetDistributionValue(index) > max)
            {
                return index - 1;
            }
        }
        throw new Exception("Could not determine last value");
    }

    #endregion

    //public TValue Min { get; set; }
    //public TValue Max { get; set; }
}

