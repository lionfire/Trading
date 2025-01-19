namespace LionFire.Mvvm;

public class KeyValueVM<TKey, TValue> : VMBase<TKey, TValue>
   where TKey : notnull
   where TValue : notnull
{
    public override TKey Key { get; }
    public override TValue Value { get; }

    public KeyValueVM(IServiceProvider serviceProvider, TKey key, TValue value) : base(serviceProvider)
    {
        this.Key = key;
        this.Value = value;
    }
}

