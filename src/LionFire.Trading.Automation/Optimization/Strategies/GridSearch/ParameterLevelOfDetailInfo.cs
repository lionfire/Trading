using System.Collections;

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

public interface IParameterLevelOfDetailInfo
{
    int TestCount { get; }
    object GetValue(int index);
}
public class ParameterLevelOfDetailInfo<TValue> : IParameterLevelOfDetailInfo
     where TValue : struct, INumber<TValue>
{
    public readonly HierarchicalPropertyInfo Info;
    private readonly ParameterOptimizationOptions<TValue> Options;

    private readonly TValue max;
    private readonly TValue min;
    private TValue range => max - min;
    private readonly TValue step;
    private readonly TValue exponent;
    private readonly double exponentAdjustment;
    private readonly double levelMultiplier;

    public ParameterLevelOfDetailInfo(int level, HierarchicalPropertyInfo info, IParameterOptimizationOptions options)
    {
        if (!options.IsEligibleForOptimization) throw new ArgumentException(nameof(options.IsEligibleForOptimization));
        Info = info;
        Options = (ParameterOptimizationOptions<TValue>)options;
        max = Options.EffectiveMaxValue;
        min = Options.EffectiveMinValue;

        if (level > 0) throw new NotImplementedException();

        int count = Options.MinOptimizationTests ?? throw new ArgumentNullException(nameof(Options.MinOptimizationTests));
        levelMultiplier = Math.Pow(2, -level);

        count = (int)double.Ceiling(count / Math.Pow(2, -level));

        //for (; level < 0; level++)
        //{
        //    count /= 2;
        //}

        IsComplete = new BitArray(count);

        var rangeForLog = Math.Log((IsComplete.Count - 1) * Convert.ToDouble(step));
        exponentAdjustment = Convert.ToDouble(range) / rangeForLog;

        GetValue = Options.DistributionParameter.HasValue ? GetDistributionValue : GetLinearValue;
        if (levelMultiplier != 1)
        {
            GetValue = i => GetValue((int)(i * levelMultiplier)); // TODO: fix artefacts?
        }
    }

    #region State

    public readonly BitArray IsComplete;

    #region Derived

    public int TestCount => IsComplete.Count;

    #endregion
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

