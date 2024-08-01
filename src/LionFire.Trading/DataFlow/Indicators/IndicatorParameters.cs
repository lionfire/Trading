using System.Reflection;
using System.Text;

namespace LionFire.Trading.DataFlow; // TODO: Move to .DataFlow namespace

public class SlottedParameters<TInstance>
{
    #region Identity

    public Type InstanceType => typeof(TInstance);

    #endregion

    #region Unbound

    public virtual IReadOnlyList<InputSlot> InputSlots => InputSlotsReflection.GetInputSlots(this.GetType());

    #endregion
}

// TODO: Move more members into base class
public abstract class IndicatorParameters<TInstance> : SlottedParameters<TInstance>, IIndicatorParameters, IParametersFor<TInstance>, IPInputThatSupportsUnboundInputs
{
    #region Unbound, potentially

    public TimeFrame? TimeFrame { get; set; }

    #endregion

    public abstract Type InputType { get; }
    public virtual IReadOnlyList<Type> SlotTypes
    {
        get
        {
#if DEBUG
            if (InputCount > 1) { throw new NotImplementedException(); }
#endif
            return [InputType];
        }
    }

    public virtual SlotsInfo Slots => SlotsInfo.GetSlotsInfo(this.GetType());

    public abstract Type OutputType { get; }

    public int Memory { get => Lookback + 1; }
    public int Lookback { get; set; } = 0;

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

    public virtual int InputCount => 1; // REVIEW - maybe this could be calculated using reflection if TConcrete was a generic parameter on this type
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
