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
