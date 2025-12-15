using LionFire.Structures;
using ReactiveUI;

namespace LionFire.Mvvm;

public class KeyedVM<TKey, TValue> : VMBase<TKey, TValue>
  where TKey : notnull
    where TValue : class, IKeyed<TKey>
{
    public override TValue? Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
    private TValue? _value;

    public override TKey Key => _value!.Key;

    public KeyedVM(TValue value)
    {
        this._value = value;
    }
}

