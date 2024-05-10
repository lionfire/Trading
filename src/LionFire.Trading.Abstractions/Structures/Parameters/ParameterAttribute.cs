using System;
using System.Collections.Generic;
using System.Linq;
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

    public string? Description {
        get; private set;
    }

    public object? DefaultValue { get; set; }
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    public object? Step { get; set; }

    public object? DefaultMin { get; set; }
    public object? DefaultMax { get; set; }
    

    public OptimizerHintFlags OptimizerHints { get; set; }

}

public enum OptimizerHintFlags
{
    Unspecified = 0,

    /// <summary>
    /// May have a minor change on results
    /// </summary>
    Minor = 1 << 0,

    Major = 1 << 1,

    /// <summary>
    /// Operates in a completely different mode
    /// </summary>
    Modal = 1 << 2,

    /// <summary>
    /// Operates in a completely different mode, but various modes may fall on a spectrum.  For example: moving average types.
    /// </summary>
    ModalSpectrum = 1 << 3,

    /// <summary>
    /// A major reversal in logic
    /// </summary>
    Reversal = 1 << 4,
}
