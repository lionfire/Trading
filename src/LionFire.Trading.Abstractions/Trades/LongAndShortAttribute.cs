namespace LionFire.Trading;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class LongAndShortAttribute : Attribute
{
    public LongAndShortAttribute(LongAndShort longAndShort)
    {
        LongAndShort = longAndShort;
    }
    public LongAndShort LongAndShort { get; }
}
