namespace LionFire.Structures;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class KeyNameAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}


