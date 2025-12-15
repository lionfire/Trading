using ReactiveUI;

namespace LionFire.Mvvm;

public class KeyValueVM<TKey, TValue> : VMBase<TKey, TValue>
   where TKey : notnull
   where TValue : class
{
    public override TKey Key { get; }

    public override TValue? Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
    private TValue? _value;

    public KeyValueVM(TKey key, TValue? value)
    {
        this.Key = key;
        this._value = value;
    }
}

