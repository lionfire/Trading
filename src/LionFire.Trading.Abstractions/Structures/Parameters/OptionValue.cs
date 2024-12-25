using System.Numerics;

namespace LionFire.Trading;

public class OptionValueModel<TValue>
    where TValue : struct, INumber<TValue>
{

    public OptionValueModel(TValue dataTypeValue, Func<TValue> getFallbackEffectiveValue)
    {
        DataTypeValue = dataTypeValue;
        GetFallbackEffectiveValue = getFallbackEffectiveValue;
    }
    public TValue? Hard { get; set; }
    public TValue? Value { get; set; }
    public bool HasValue => Value.HasValue;
    public TValue? Default { get; set; }
    public TValue EffectiveValue
    {
        get => Value ?? Default ?? Hard ?? GetFallbackEffectiveValue();
        set => Value = value;
    }
    public Func<TValue> GetFallbackEffectiveValue { get; }

    public TValue DataTypeValue { get; set; }

    public bool AllowNegativeValues { get; set; }
}

