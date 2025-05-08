namespace LionFire.Trading.Automation;

[Flags]
public enum BotHarnessFeatures
{
    Unspecified = 0,
    Bars = 1 << 0,
    Ticks = 1 << 1,
    /// <summary>
    /// Refuse to run if order book info is not available for the symbol being traded
    /// </summary>
    OrderBook = 1 << 2,
}

public static class BotHarnessFeaturesExtensions
{
    public static bool HasFlag(this BotHarnessFeatures features, BotHarnessFeatures flag)
    {
        return (features & flag) == flag;
    }

    public static bool Ticks(this BotHarnessFeatures features) => features.HasFlag(BotHarnessFeatures.Ticks);
}
