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
    public object? OptimizePriority { get; set; }

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



