﻿using LionFire.Trading;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace
public interface IPUnboundInput : IPInput
{
    IReadOnlyList<InputSlot> InputSlots { get; }
}

public readonly record struct SlotSource(IPInput? Input, int ParentInputIndex);

[AttributeUsage(AttributeTargets.Property)]
public class SourceAttribute : Attribute
{
    public int Index { get; }

    public SourceAttribute(int index) { Index = index;}
    public SourceAttribute(string sourceUri) {

        // SourceUri:
        // - no scheme (no colon): PropertyName
        // - scheme "s": 
        // -  "s": symbol
        // - "i": indicator
    }
}

public abstract class IndicatorParameters<TIndicator> : IIndicatorParameters, IParametersFor<TIndicator>, IPUnboundInput
{
    public virtual IReadOnlyList<InputSlot> InputSlots => [];

    public Type IndicatorType => typeof(TIndicator);
    public abstract Type InputType { get; }
    public abstract Type OutputType { get; }

    public int Memory { get; set; } = 1;
    public abstract Type ValueType { get; }
    public virtual string Key
    {
        get
        {
            // TODO: Use KeyNameAttribute if available
            // OPTIMIZE: Cache reflection

            var name = this.GetType().Name;
            if (name.StartsWith("P") && name.Length >= 2 && char.IsUpper(name[1]))
            {
                name = name.Substring(1);
            }

            var sb = new StringBuilder();
            foreach (var c in name) { if (char.IsUpper(c)) { sb.Append(c); } }

            var parameters = Parameters;
            if (parameters != null && parameters.Length > 0)
            {
                sb.Append('(');
                bool firstParameter = true;
                foreach (var p in parameters)
                {
                    if (firstParameter) firstParameter = false;
                    else sb.Append(", ");
                    if (p is IKeyed<string> keyed)
                    {
                        sb.Append(keyed.Key);
                    }
                    else
                    {
                        sb.Append(p.ToString());
                    }
                }
                sb.Append(')');
            }
            return sb.ToString();
        }
    }

    public virtual IEnumerable<PropertyInfo> ParameterProperties => this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite);
    public virtual object?[] Parameters => ParameterProperties.Select(p => p.GetValue(this)).ToArray();
}

public abstract class IndicatorParameters<TIndicator, TOutput> : IndicatorParameters<TIndicator>
{
    /// <summary>
    /// Matches TOutput
    /// </summary>
    public override Type InputType => typeof(TOutput);
    public override Type OutputType => typeof(TOutput);
    public override Type ValueType => typeof(TOutput);

}

public abstract class IndicatorParameters<TIndicator, TInput, TOutput> : IndicatorParameters<TIndicator>
{
    /// <summary>
    /// Matches TOutput
    /// </summary>
    public override Type InputType => typeof(TInput);
    public override Type OutputType => typeof(TOutput);
    public override Type ValueType => typeof(TOutput);
}

public interface IParametersFor<T> { }


