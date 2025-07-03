namespace LionFire.Trading.Automation;

[Flags]
public enum SimFeatures
{
    Unspecified = 0,
    Bars = 1 << 0,
    Ticks = 1 << 1,
    /// <summary>
    /// Refuse to run if order book info is not available for the symbol being traded
    /// </summary>
    OrderBook = 1 << 2,
}

public static class SimFeaturesX
{
    public static bool HasFlag(this SimFeatures features, SimFeatures flag)
    {
        return (features & flag) == flag;
    }

    public static bool Ticks(this SimFeatures features) => features.HasFlag(SimFeatures.Ticks);


}
