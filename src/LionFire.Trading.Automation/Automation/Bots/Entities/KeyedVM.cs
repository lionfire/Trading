using LionFire.Structures;

namespace LionFire.Mvvm;

public class KeyedVM<TKey, TValue> : VMBase<TKey, TValue>
  where TKey : notnull
    where TValue : notnull, IKeyed<TKey>
{
    public override TValue Value => value;
    private TValue value;

    public override TKey Key => value.Key;

    public KeyedVM(TValue value) 
    {
        this.value = value;
    }
}

