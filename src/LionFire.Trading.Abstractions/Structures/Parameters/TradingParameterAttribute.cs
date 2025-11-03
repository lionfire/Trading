using LionFire.ExtensionMethods.Copying;
using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LionFire.Trading;

[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TradingParameterAttribute : Attribute
{

    public TradingParameterAttribute(string description)
    {
        this.Description = description;
    }
    public TradingParameterAttribute()
    {
    }

    public string? Description
    {
        get; private set;
    }

    public object? DefaultValue { get; set; }
    public object? HardValueMin { get; set; } 
    public object? HardValueMax { get; set; } 
    public object? ValueMax { get; set; }
    public object? ValueMin { get; set; }
    public object? Step { get; set; }
    public object? MinStep { get; set; }
    public object? MaxStep { get; set; }

    public object? DefaultMin { get; set; }
    public object? DefaultMax { get; set; }

    /// <summary>
    /// Can be a single array, or an array of arrays.
    /// If array of arrays:
    /// - index is the inverse of the level of detail.  First array is the most detailed (Level 0), last array is the least detailed (Level -N).
    /// </summary>
    public object[]? DefaultSearchSpaces { get; set; }

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

    /// <summary>
    /// Lower values will be optimized first
    /// </summary>
    public object? OptimizeOrderTiebreaker { get; set; }

    /// <summary>
    /// Recommended: negative whole numbers
    /// Default: 0
    /// Set this to below 0 to avoid optimizing for this parameter when a limited optimization is desired
    /// </summary>
    public object? OptimizePriority { get; set; }

    public int OptimizePriorityInt => Convert.ToInt32(OptimizePriority);

    public object? MinProbes { get; set; }
    public object? MaxProbes { get; set; }
    public object? Exponent { get; set; }
    public object? DefaultExponent { get; set; }
    public object? MinExponent { get; set; }
    public object? MaxExponent { get; set; }

    public bool IsCategory => OptimizerHints == OptimizationDistributionKind.Category || OptimizerHints == OptimizationDistributionKind.SpectralCategory;

    public int DefaultSearchSpacesCount => DefaultSearchSpaces?.Length ?? 0;


    //public IParameterOptimizationOptions GetParameterOptimizationOptions(Type valueType)
    //{
    //    if (parameterOptimizationOptions == null)
    //    {
    //        //parameterOptimizationOptions = (IParameterOptimizationOptions)Activator.CreateInstance(typeof(ParameterOptimizationOptions<>).MakeGenericType(valueType))!;
    //        parameterOptimizationOptions = LionFire.Trading.ParameterOptimizationOptions.Create(valueType, "<AttributePrototype>");
    //        AssignFromExtensions.AssignNonDefaultPropertiesFrom(parameterOptimizationOptions!, this);
    //    }
    //    return parameterOptimizationOptions;
    //}
    //private IParameterOptimizationOptions? parameterOptimizationOptions;
}



