using LionFire.ExtensionMethods.Copying;
using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading;

[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ParameterAttribute : Attribute
{

    public ParameterAttribute(string description)
    {
        this.Description = description;
    }
    public ParameterAttribute()
    {
    }

    public string? Description
    {
        get; private set;
    }

    public object? DefaultValue { get; set; }
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    public object? Step { get; set; }
    public object? MinStep { get; set; }
    public object? MaxStep { get; set; }

    public object? DefaultMin { get; set; }
    public object? DefaultMax { get; set; }

    public object[]? DefaultSearchSpace { get; set; }

    /// <summary>
    /// Defaults to e if not set
    /// Not applicable if SearchLogarithmExponent is not set
    /// </summary>
    public object? SearchLogarithmBase { get; set; }

    /// <summary>
    /// If not set, uses linear search
    /// </summary>
    public object? SearchLogarithmExponent { get; set; }

    public OptimizationDistributionKind OptimizerHints { get; set; }

    public object? MinProbes { get; set; }
    public object? MaxProbes { get; set; }
    public object? DistributionParameter { get; set; }

    public IParameterOptimizationOptions GetParameterOptimizationOptions(Type valueType)
    {
        if (parameterOptimizationOptions == null)
        {
            //parameterOptimizationOptions = (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType))!;
            parameterOptimizationOptions = LionFire.Trading.ParameterOptimizationOptions.Create(valueType);
            AssignFromExtensions.AssignNonDefaultPropertiesFrom(parameterOptimizationOptions!, this);
        }
        return parameterOptimizationOptions;
    }
    private IParameterOptimizationOptions? parameterOptimizationOptions;
}

public interface IParameterOptimizationOptions
{
    double? DistributionParameter { get; }
    //object? MinStep { get; }
    //object? MaxStep { get; }
    //object? MaxValue { get; }
    //object? MinValue { get; }
    //int? MinProbes { get; }
    //int? MaxProbes { get; }

    double? FitnessOfInterest { get; set; }


    #region Derived

    int? MinOptimizationTests { get; }
    int MaxOptimizationTests { get; }

    //int StepsPossible { get; }

    bool IsEligibleForOptimization { get; }
    object SingleValue { get; }

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
    object IParameterOptimizationOptions.SingleValue => SingleValue ?? throw new InvalidOperationException($"{nameof(SingleValue)} not available when {nameof(IsEligibleForOptimization)} is false");

    public bool IsEligibleForOptimization => !SingleValue.HasValue;

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

    public TValue? MinValue { get; set; }
    public TValue EffectiveMinValue => MinValue ?? (AllowNegativeValues ? DataTypeMinValue : TValue.Zero);
    public TValue? MaxValue { get; set; }
    public TValue EffectiveMaxValue => MaxValue ?? DataTypeMaxValue;
    public TValue EffectiveRange => EffectiveMaxValue - EffectiveMinValue;

    public TValue? MinStep { get; set; }
    public TValue EffectiveMinStep => MinStep ?? TValue.One;
    public TValue? MaxStep { get; set; }

    /// <summary>
    /// When optimizing, don't skip over ranges larger than this amount in order for the optimization to be considered complete.
    /// </summary>
    public TValue? MaxOptimizationStep { get; set; }
    = DefaultMaxOptimizationStep;

    private static readonly TValue DefaultMaxOptimizationStep = TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One + TValue.One;

    public double? DistributionParameter { get; set; }
    public ulong? MinProbes { get; set; }
    public ulong? MaxProbes { get; set; }
    public double? FitnessOfInterest { get; set; }

    public bool AllowNegativeValues { get; set; }

    #region Derived

    public static TValue DataTypeMaxValue => (TValue)typeof(TValue).GetField(nameof(int.MaxValue), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!;
    public static TValue DataTypeMinValue => (TValue)typeof(TValue).GetField(nameof(int.MinValue), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!;

    public int? MinOptimizationTests => MaxOptimizationStep.HasValue ? (int)Convert.ToUInt32(Math.Ceiling(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(MaxOptimizationStep))) : null;
    public int MaxOptimizationTests => (int)Convert.ToUInt32(Convert.ToSingle(EffectiveRange) / Convert.ToSingle(EffectiveMinStep));

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


public enum OptimizationDistributionKind
{
    Unspecified = 0,

    Period = 1 << 0,


    ///// <summary>
    ///// May have a minor change on results
    ///// </summary>
    //Minor = 1 << 0,

    //Major = 1 << 1,

    /// <summary>
    /// Operates in a completely different mode
    /// </summary>
    Category = 1 << 2,

    /// <summary>
    /// Operates in a completely different mode, but various modes may fall on a spectrum.  For example: moving average types.
    /// </summary>
    SpectralCategory = 1 << 3,

    /// <summary>
    /// A major reversal in logic
    /// </summary>
    Reversal = 1 << 4,
}
