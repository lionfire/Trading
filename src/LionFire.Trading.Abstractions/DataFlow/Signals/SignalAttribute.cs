namespace LionFire.Trading;

[AttributeUsage(AttributeTargets.Property)]
public class SignalAttribute : Attribute
{
    public int Index { get; }

    public SignalAttribute(int index) { Index = index; }
    public SignalAttribute(string sourceUri)
    {

        // SourceUri:
        // - no scheme (no colon): PropertyName
        // - scheme "s": 
        // -  "s": symbol
        // - "i": indicator
    }
}

