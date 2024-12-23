﻿using LionFire.ExtensionMethods.Copying;
using System.Numerics;

namespace LionFire.Trading;
public interface IParameterOptimizationOptions
{
    int? OptimizeOrder { get; set; }
    double? DistributionParameter { get; }
    //object? MinStep { get; }
    //object? MaxStep { get; }
    //object? MaxValue { get; }
    //object? MinValue { get; }
    //int? MinProbes { get; }
    //int? MaxProbes { get; }

    double? FitnessOfInterest { get; set; }

    #region Derived

    ulong? EffectiveMinCount { get; }
    ulong EffectiveMaxCount { get; }

    //int StepsPossible { get; }

    bool IsEligibleForOptimization { get; }
    bool? EnableOptimization { get; set; }
    object SingleValue { get; }
    object? DefaultValue { get; }

    #endregion

    IParameterOptimizationOptions Clone();
}

public static class ParameterOptimizationOptions
{
    public static IParameterOptimizationOptions Create(Type valueType)
    {
        if (valueType.IsEnum) { valueType = Enum.GetUnderlyingType(valueType); }

        return (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType))!;
    }
    public static IParameterOptimizationOptions Create(Type valueType, IParameterOptimizationOptions assignFrom)
    {
        return (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType), assignFrom)!;
    }
}

public class ParameterOptimizationOptions<TValue>
    : IParameterOptimizationOptions
    where TValue : struct, INumber<TValue>
{
    public ParameterOptimizationOptions() { }
    public ParameterOptimizationOptions(IParameterOptimizationOptions assignFrom)
    {
        if (assignFrom != null)
        {
            AssignFromExtensions.AssignPropertiesFrom(this, assignFrom);
        }
    }

    #region Derived

    public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : null;
    //public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : ((MaxValue - MinValue) / TValue.CreateChecked(2.0));
    //public TValue? SingleValue => MinValue.HasValue && MinValue == MaxValue ? MinValue : (DefaultValue ?? MinValue);
    //((MaxValue - MinValue) / TValue.CreateChecked(2.0)));

    object IParameterOptimizationOptions.SingleValue => SingleValue ?? throw new InvalidOperationException($"{nameof(SingleValue)} not available when {nameof(IsEligibleForOptimization)} is false");

    public bool IsEligibleForOptimization => !SingleValue.HasValue && EnableOptimization != false;

    public bool? EnableOptimization { get; set; }

    public TValue? Range => MaxValue - MinValue;

    //public Func<(TValue min, TValue max, int count), Func<int, TValue>> GetFunc
    //{
    //    // Use DistributionParameter as the log base
    //    if (DistributionParameter.HasValue)
    //    {
    //        return (min, max, count) => (int i) => min + (max - min) * Math.Pow(DistributionParameter.Value, i / count);
    //    }
    //}

    //return (min, max, count) => (int i) => min + (max - min) * i / count;
    #endregion
    public int? OptimizeOrder { get; set; }

    public TValue? HardMinValue { get; set; }
    public TValue? MinValue { get; set; }
    public TValue? DefaultMin { get; set; }
    public TValue EffectiveMinValue => MinValue ?? DefaultMin ?? HardMinValue ?? (AllowNegativeValues ? DataTypeMinValue : TValue.Zero);
    public TValue? HardMaxValue { get; set; }
    public TValue? MaxValue { get; set; }
    public TValue? DefaultMax { get; set; }
    public TValue? DefaultValue { get; set; }
    object? IParameterOptimizationOptions.DefaultValue => DefaultValue;

    public TValue EffectiveMaxValue => MaxValue ?? DefaultMax ?? HardMaxValue ?? DefaultMaxForDataType;
    public TValue EffectiveRange => (EffectiveMaxValue - EffectiveMinValue) + TValue.One;

    public TValue? MinStep { get; set; }
    public TValue EffectiveMinStep
    {
        get
        {
            if (MinStep.HasValue) return MinStep.Value;
            var range = EffectiveRange;

            if (range < TValue.CreateChecked(10)) { return TValue.CreateChecked(0.1); }

            return TValue.One;
        }
    }
    public TValue? MaxStep { get; set; }

    /// <summary>
    /// When optimizing, don't skip over ranges larger than this amount in order for the optimization to be considered complete.
    /// </summary>
    public TValue? MaxOptimizationStep { get; set; }
    public TValue EffectiveMaxOptimizationStep
    => MaxOptimizationStep ?? DefaultMaxOptimizationStep;
    public TValue? OptimizationStep { get; set; }
    public TValue EffectiveOptimizationStep => OptimizationStep ?? EffectiveMaxOptimizationStep;

    const int DefaultMinOptimizationSteps = 16;
    private TValue DefaultMaxOptimizationStep
    {
        get
        {
            var maxSteps = EffectiveRange / EffectiveMinStep;
            while (maxSteps > TValue.CreateChecked(DefaultMinOptimizationSteps))
            {
                maxSteps /= TValue.CreateChecked(2);
            }
            return EffectiveRange / maxSteps;
        }
    }

    public double? DistributionParameter { get; set; }
    public ulong? MinCount { get; set; }
    public ulong? MaxCount { get; set; }
    public double? FitnessOfInterest { get; set; }

    public bool AllowNegativeValues { get; set; }

    #region Derived

    public static TValue DefaultMaxForDataType
    {
        get
        {
            if (typeof(TValue) == typeof(bool)) return (TValue)(object)true;
            return TValue.CreateChecked(100);
        }
    }

    public static TValue DataTypeMaxValue => (TValue)typeof(TValue).GetField(nameof(int.MaxValue), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!;
    public static TValue DataTypeMinValue => (TValue)typeof(TValue).GetField(nameof(int.MinValue), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!;

    public ulong MinCountFromMaxStep => Convert.ToUInt64(Math.Ceiling(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMaxOptimizationStep)));
    public ulong? EffectiveMinCount => Math.Max(MinCount ?? 0, MinCountFromMaxStep);
    
    public ulong MaxCountFromMinStep => Convert.ToUInt64(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMinStep));
    public ulong EffectiveMaxCount => Math.Min((MaxCount ?? ulong.MaxValue), MaxCountFromMinStep);

    #endregion

    #region Misc

    public IParameterOptimizationOptions Clone() => (IParameterOptimizationOptions)MemberwiseClone();
    public override string ToString() => this.ToXamlProperties();

    #endregion
}

//public class BoolParameterOptimizationOptions : IParameterOptimizationOptions
//{
//    public double? DistributionParameter => throw new NotImplementedException();

//    public double? FitnessOfInterest { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//    public int MinOptimizationTests => throw new NotImplementedException();

//    public int StepsPossible => 2;

//    public bool IsEligibleForOptimization => throw new NotImplementedException();

//    public IParameterOptimizationOptions Clone()
//    {
//        throw new NotImplementedException();
//    }
//}
