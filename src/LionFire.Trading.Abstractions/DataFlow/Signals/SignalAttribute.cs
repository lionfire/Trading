namespace LionFire.Trading;

/// <summary>
/// Belongs on the Bot properties (IBot2). Has no effect on Bot Parameter (IPBot2) properties
/// </summary>
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

/// <summary>
/// (Optional, currently, as a tip to developers)
/// (ENH: If IPBot2 has PSignal with no matching Signal on IBot2, then throw an exception)
/// Marker to indicate that the corresponding property on the Bot Type should have a [Signal(...)] attribute
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PSignalAttribute : Attribute
{
}
